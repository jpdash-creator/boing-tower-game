"""Art pipeline for the owner-picked Tier 1 images (roadmap task T1). Costs nothing (no OpenArt).

    set PYTHONPATH=E:/pylibs   (SciPy lives on E:, not C:)
    python Tools/art_pipeline/process_art.py            # all 66 assets
    python Tools/art_pipeline/process_art.py reg001_props ui_panel_glass   # only these

Reads the picked candidate (Content/assets/picks.json -> Art/_candidates/<id>/<n>.png), never the Assets copy, so it is re-runnable.
Modes:
  full   keep as is (backgrounds, map, loading screen, app icon): copied to the final path
  single key the background and trim (currency icons)
  sheet  key the background and split into one PNG per object: Assets/.../<dir>/<id>/<id>_NN.png (+ sprites.json)
The generated art does NOT use #808080: each sheet has its own flat blue-grey, so the key colour is the median of the image border.
Soft neon glow edges are kept as partial alpha. Interior pixels that merely resemble the background (silver buttons) stay opaque
because only background connected to the border is removed.
"""
import json
import os
import shutil
import sys

sys.path.insert(0, "E:/pylibs")
import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402
from scipy import ndimage as ndi  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
KEY_TOL = 24          # colour distance treated as pure background
GLOW_SOFT = 46        # distance over which alpha ramps from 0 to 1 near the background
BAND_PX = 14          # only pixels this close to the background get soft alpha
MERGE_GAP = 8          # px: parts of one object (glow, sparkles, arrows) closer than 2x this are merged
PAD = 10

# Sheets whose object count is known: if keying merges touching objects, the widest sprite is cut at its narrowest column.
EXPECTED = {"pck_coin_sheet": 6}

FULL = {"fnd_app_icon", "map_region_map", "ui_loading_screen_art"}
SINGLE = {"cur_coin_single", "cur_gem_single", "cur_star_empty", "cur_star_filled"}


def mode_of(asset_id):
    if asset_id in FULL or "_bg_" in asset_id:
        return "full"
    if asset_id in SINGLE:
        return "single"
    return "sheet"


def key_image(rgb):
    """Returns RGBA uint8 with the background removed, plus the key colour."""
    arr = rgb.astype(np.float32)
    h, w, _ = arr.shape
    border = np.concatenate([arr[:4].reshape(-1, 3), arr[-4:].reshape(-1, 3), arr[:, :4].reshape(-1, 3), arr[:, -4:].reshape(-1, 3)])
    bg = np.median(border, axis=0)
    dist = np.sqrt(((arr - bg) ** 2).sum(axis=2))

    near = dist < KEY_TOL
    lab, _ = ndi.label(near)
    edge_labels = np.unique(np.concatenate([lab[0], lab[-1], lab[:, 0], lab[:, -1]]))
    edge_labels = edge_labels[edge_labels != 0]
    bgmask = np.isin(lab, edge_labels)

    d_to_bg = ndi.distance_transform_edt(~bgmask)
    alpha = np.ones((h, w), dtype=np.float32)
    alpha[bgmask] = 0.0
    band = (~bgmask) & (d_to_bg <= BAND_PX)
    t = np.clip((dist - KEY_TOL) / GLOW_SOFT, 0.0, 1.0)
    soft = t * t * (3 - 2 * t)  # smoothstep
    alpha[band] = np.minimum(alpha[band], soft[band])
    rgba = np.dstack([rgb, np.round(alpha * 255).astype(np.uint8)])
    return rgba, bg


def split_sheet(rgba):
    """Returns a list of (bbox, sprite RGBA) ordered row-major."""
    alpha = rgba[:, :, 3]
    solid = alpha > 128
    area_min = max(2500, int(solid.size * 0.0007))
    d = ndi.distance_transform_edt(~solid)
    merged = d <= MERGE_GAP
    lab, n = ndi.label(merged)
    objs = ndi.find_objects(lab)
    sprites = []
    for i, sl in enumerate(objs, start=1):
        comp = (lab[sl] == i)
        if int((solid[sl] & comp).sum()) < area_min:
            continue
        y0 = max(0, sl[0].start - PAD)
        y1 = min(rgba.shape[0], sl[0].stop + PAD)
        x0 = max(0, sl[1].start - PAD)
        x1 = min(rgba.shape[1], sl[1].stop + PAD)
        region = np.zeros(rgba.shape[:2], dtype=bool)
        region[sl] = comp
        region = ndi.binary_dilation(region[y0:y1, x0:x1], iterations=PAD)
        crop = rgba[y0:y1, x0:x1].copy()
        crop[:, :, 3] = np.where(region, crop[:, :, 3], 0)
        sprites.append(((x0, y0, x1 - x0, y1 - y0), crop))
    if not sprites:
        return []
    heights = sorted(b[3] for b, _ in sprites)
    row_tol = heights[len(heights) // 2] * 0.6
    sprites.sort(key=lambda s: s[0][1] + s[0][3] / 2)
    rows, current = [], [sprites[0]]
    for s in sprites[1:]:
        if abs((s[0][1] + s[0][3] / 2) - (current[-1][0][1] + current[-1][0][3] / 2)) <= row_tol:
            current.append(s)
        else:
            rows.append(current)
            current = [s]
    rows.append(current)
    ordered = []
    for r in rows:
        ordered.extend(sorted(r, key=lambda s: s[0][0]))
    return ordered


def split_at_valley(sprite):
    """Cuts one sprite into two at the column with the least opaque pixels in its middle 30-70 percent."""
    bbox, crop = sprite
    cols = (crop[:, :, 3] > 128).sum(axis=0).astype(np.float32)
    k = max(3, crop.shape[1] // 40)
    smooth = np.convolve(cols, np.ones(k) / k, mode="same")
    lo, hi = int(crop.shape[1] * 0.3), int(crop.shape[1] * 0.7)
    cut = lo + int(np.argmin(smooth[lo:hi]))
    left, right = crop.copy(), crop.copy()
    left[:, cut:, 3] = 0
    right[:, :cut, 3] = 0
    out = []
    for part, x_off in ((left, 0), (right, cut)):
        sub = trim(part)
        ys, xs = np.nonzero(part[:, :, 3] > 8)
        y0 = max(0, ys.min() - PAD)
        x0 = max(0, xs.min() - PAD)
        out.append(((bbox[0] + x0, bbox[1] + y0, sub.shape[1], sub.shape[0]), sub))
    return out


def trim(rgba):
    ys, xs = np.nonzero(rgba[:, :, 3] > 8)
    if len(ys) == 0:
        return rgba
    y0, y1, x0, x1 = max(0, ys.min() - PAD), min(rgba.shape[0], ys.max() + 1 + PAD), max(0, xs.min() - PAD), min(rgba.shape[1], xs.max() + 1 + PAD)
    return rgba[y0:y1, x0:x1]


def process(asset_id, entry):
    cand = os.path.join(ROOT, entry["sourceCandidate"])
    final = os.path.join(ROOT, entry["finalPath"])
    mode = mode_of(asset_id)
    outdir = os.path.join(os.path.dirname(final), asset_id)
    report = {"asset": asset_id, "mode": mode, "pick": entry["pick"], "sprites": []}

    if mode == "full":
        os.makedirs(os.path.dirname(final), exist_ok=True)
        shutil.copyfile(cand, final)
        return report

    rgb = np.asarray(Image.open(cand).convert("RGB"))
    rgba, bg = key_image(rgb)
    report["keyColour"] = [int(round(c)) for c in bg]
    os.makedirs(outdir, exist_ok=True)
    for f in os.listdir(outdir):
        if f.lower().endswith(".png") or f.lower() == "sprites.json":
            os.remove(os.path.join(outdir, f))

    if mode == "single":
        out = trim(rgba)
        Image.fromarray(out).save(os.path.join(outdir, asset_id + ".png"))
        report["sprites"].append({"file": asset_id + ".png", "w": int(out.shape[1]), "h": int(out.shape[0])})
    else:
        found = split_sheet(rgba)
        want = EXPECTED.get(asset_id)
        while want and len(found) < want:
            widest = max(range(len(found)), key=lambda i: found[i][0][2])
            found = found[:widest] + split_at_valley(found[widest]) + found[widest + 1:]
        for n, (bbox, crop) in enumerate(found, start=1):
            name = "%s_%02d.png" % (asset_id, n)
            Image.fromarray(crop).save(os.path.join(outdir, name))
            report["sprites"].append({"file": name, "x": bbox[0], "y": bbox[1], "w": bbox[2], "h": bbox[3]})

    with open(os.path.join(outdir, "sprites.json"), "w", encoding="utf-8", newline="\n") as f:
        json.dump(report, f, indent=2, default=int)
        f.write("\n")
    # The raw sheet/single copy under Assets is replaced by the sprites (candidates keep the originals).
    for p in (final, final + ".meta"):
        if os.path.exists(p):
            os.remove(p)
    return report


def main():
    with open(os.path.join(ROOT, "Content", "assets", "picks.json"), encoding="utf-8") as f:
        picks = json.load(f)["picks"]
    only = set(sys.argv[1:])
    summary = []
    for asset_id in sorted(picks):
        if only and asset_id not in only:
            continue
        r = process(asset_id, picks[asset_id])
        summary.append(r)
        print("%-24s %-6s pick %d  sprites %2d  %s" % (asset_id, r["mode"], r["pick"], len(r["sprites"]), r.get("keyColour", "")))
    out = os.path.join(ROOT, "Art", "_pick", "sprites_report.json")
    os.makedirs(os.path.dirname(out), exist_ok=True)
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        json.dump(summary, f, indent=1, default=int)


if __name__ == "__main__":
    main()
