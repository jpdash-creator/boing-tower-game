"""Measures the walkable island of every platform sprite so the game can draw it at its real aspect ratio.

    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/platform_metrics.py            # all regions, writes surface.json next to each tileset
    python Tools/art_pipeline/platform_metrics.py --debug E:/Unity/qa   # also saves debug overlays

Per sprite (platform kinds only, indices from tileset_map.json) it writes pixel values in the sprite's own image space:
  bodyLeft/bodyRight  horizontal extent of the island (props such as clouds, arrows and springs are ignored)
  surfaceY            y (from the top) of the walking surface: where the character's feet stand
  bodyBottom          lowest row of the island body
The island is found from the row profile: rows holding at least 45 percent of the widest row are body rows; the extent is measured on the
upper half of the body rows, which excludes props hanging below (springs) or sitting to the side lower down.
"""
import glob
import json
import os
import sys

sys.path.insert(0, "E:/pylibs")
import numpy as np  # noqa: E402
from PIL import Image, ImageDraw  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
KINDS = ["normal", "narrow", "moving", "crumble", "bounce"]


def measure(path):
    im = Image.open(path).convert("RGBA")
    a = np.asarray(im)[:, :, 3] > 128
    h, w = a.shape
    rows = a.sum(axis=1)
    widest = rows.max()
    body_rows = np.nonzero(rows >= 0.45 * widest)[0]
    # keep the longest contiguous run (the island), not a cloud band elsewhere
    runs, start = [], body_rows[0]
    for i in range(1, len(body_rows)):
        if body_rows[i] != body_rows[i - 1] + 1:
            runs.append((start, body_rows[i - 1]))
            start = body_rows[i]
    runs.append((start, body_rows[-1]))
    top, bottom = max(runs, key=lambda r: r[1] - r[0])
    upper = a[top: top + max(2, (bottom - top) // 2)]
    cols = np.nonzero(upper.sum(axis=0) >= max(2, 0.25 * upper.shape[0]))[0]
    left, right = int(cols.min()), int(cols.max())
    thick = bottom - top
    # The grass blades start above 'top'; the surface the character stands on is a little below the first full row.
    surface = int(top + 0.12 * thick)
    return {"w": w, "h": h, "bodyLeft": left, "bodyRight": right, "surfaceY": surface, "bodyBottom": int(bottom)}


KEEP_FRACTION = 0.55   # share of the island body height kept below the surface (the dirt is cut and faded: platforms stay thin)
TOP_MARGIN = 0.10      # share of body height kept above the first full row (grass blades)
FADE_FRACTION = 0.30   # the lowest part of the kept strip fades out


def palette(path, m):
    """Median colours of the island's top (grass, sand, ...) and side (dirt, ...): used to colour the clean code-drawn platforms."""
    im = np.asarray(Image.open(path).convert("RGBA"))
    thick = m["bodyBottom"] - m["surfaceY"]
    def med(y0, y1):
        reg = im[max(0, int(y0)):int(y1), m["bodyLeft"]:m["bodyRight"]]
        px = reg[reg[:, :, 3] > 200][:, :3]
        return [int(v) for v in np.median(px, axis=0)] if len(px) else [128, 128, 128]
    return {"top": med(m["surfaceY"], m["surfaceY"] + 0.25 * thick), "side": med(m["surfaceY"] + 0.6 * thick, m["bodyBottom"] - 0.05 * thick)}


def make_strip(path, m, out_path):
    """Crops the island to a thin slab: props removed (side columns dropped), dirt cut and faded. Returns metrics of the new image."""
    im = Image.open(path).convert("RGBA")
    thick = m["bodyBottom"] - m["surfaceY"]
    top = max(0, int(m["surfaceY"] - 0.12 * (m["bodyBottom"] - m["surfaceY"]) / 0.88 - TOP_MARGIN * thick))
    bottom = min(im.height, int(m["surfaceY"] + KEEP_FRACTION * thick))
    left = max(0, m["bodyLeft"] - 6)
    right = min(im.width, m["bodyRight"] + 6)
    strip = np.array(im.crop((left, top, right, bottom)))
    n = strip.shape[0]
    fade_rows = max(2, int(n * FADE_FRACTION))
    ramp = np.linspace(1.0, 0.0, fade_rows)
    strip[n - fade_rows:, :, 3] = (strip[n - fade_rows:, :, 3] * ramp[:, None]).astype(np.uint8)
    os.makedirs(os.path.dirname(out_path), exist_ok=True)
    Image.fromarray(strip).save(out_path)
    return {"w": int(strip.shape[1]), "h": int(strip.shape[0]), "bodyLeft": m["bodyLeft"] - left, "bodyRight": m["bodyRight"] - left,
            "surfaceY": int(m["surfaceY"] - top), "bodyBottom": int(n)}


def main():
    debug = sys.argv[sys.argv.index("--debug") + 1] if "--debug" in sys.argv else None
    for tmap in sorted(glob.glob(os.path.join(ROOT, "Assets", "Art", "Regions", "*", "tiles", "*", "tileset_map.json"))):
        d = os.path.dirname(tmap)
        info = json.load(open(tmap))
        asset = info["asset"]
        out = {"asset": asset, "platforms": {}}
        for kind in KINDS:
            idx = info.get(kind)
            if not idx:
                continue
            f = os.path.join(d, "%s_%02d.png" % (asset, idx))
            m = measure(f)
            m["index"] = idx
            if kind == "normal":
                out["palette"] = palette(f, m)
            sm = make_strip(f, m, os.path.join(d, "plat", "%s_%s.png" % (asset, kind)))
            sm["index"] = idx
            sm["file"] = "plat/%s_%s.png" % (asset, kind)
            out["platforms"][kind] = sm
            if debug:
                os.makedirs(debug, exist_ok=True)
                im = Image.open(f).convert("RGBA")
                bg = Image.new("RGBA", im.size, (170, 170, 170, 255))
                bg.alpha_composite(im)
                dr = ImageDraw.Draw(bg)
                dr.rectangle([m["bodyLeft"], m["surfaceY"], m["bodyRight"], m["bodyBottom"]], outline=(255, 0, 0, 255), width=3)
                dr.line([0, m["surfaceY"], m["w"], m["surfaceY"]], fill=(0, 0, 255, 255), width=3)
                bg.convert("RGB").save(os.path.join(debug, "%s_%s.png" % (asset, kind)))
        with open(os.path.join(d, "surface.json"), "w", encoding="utf-8", newline="\n") as fh:
            json.dump(out, fh, indent=2)
            fh.write("\n")
        print(asset, {k: (v["bodyRight"] - v["bodyLeft"], v["bodyBottom"] - v["surfaceY"]) for k, v in out["platforms"].items()})


if __name__ == "__main__":
    main()
