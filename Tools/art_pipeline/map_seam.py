"""Seam tools for the chained world-map segments (T33).

    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/map_seam.py score <previous.png> <candidate1.png> [candidate2.png ...]
        Scores how well each candidate's bottom fifth continues the previous segment's top fifth (mean absolute colour difference
        after a small blur; lower is better) and prints the horizontal position of the road at both edges.
    python Tools/art_pipeline/map_seam.py stitch <out.png> <seg1.png> <seg2.png> ...    (segments bottom to top)
        Stacks the segments with a 20 percent overlap, cross-fading the overlap, and shifting each segment sideways so the road meets.
"""
import sys

sys.path.insert(0, "E:/pylibs")
import numpy as np  # noqa: E402
from PIL import Image, ImageFilter  # noqa: E402

OVERLAP = 0.20


def load(path, width=None):
    im = Image.open(path).convert("RGB")
    if width and im.width != width:
        im = im.resize((width, round(im.height * width / im.width)), Image.LANCZOS)
    return im


def road_x(im, top):
    """Horizontal centre (0..1) of the road in the band at the top or bottom edge: the green road has white border lines, so look for
    near-white pixels flanking strongly green ones; falls back to the centroid of saturated green."""
    a = np.asarray(im).astype(np.int32)
    h = a.shape[0]
    band = a[: int(h * 0.04)] if top else a[int(h * 0.96):]
    r, g, b = band[..., 0], band[..., 1], band[..., 2]
    green = (g > 150) & (g - r > 40) & (g - b > 40)
    cols = green.sum(axis=0).astype(np.float64)
    if cols.sum() < 20:
        return 0.5
    # the road is the widest dense run of green columns: use a smoothed profile and take its peak
    k = max(5, a.shape[1] // 60)
    sm = np.convolve(cols, np.ones(k) / k, mode="same")
    return float(np.argmax(sm)) / a.shape[1]


def score(prev_path, cand_paths):
    prev = load(prev_path)
    for p in cand_paths:
        cand = load(p, prev.width)
        n = int(min(prev.height, cand.height) * OVERLAP)
        a = np.asarray(prev.crop((0, 0, prev.width, n)).filter(ImageFilter.GaussianBlur(6))).astype(np.float32)
        b = np.asarray(cand.crop((0, cand.height - n, cand.width, cand.height)).filter(ImageFilter.GaussianBlur(6))).astype(np.float32)
        mad = float(np.abs(a - b).mean())
        print("%s  seamMAD %.1f  prev-top road x %.3f  cand-bottom road x %.3f  cand-top road x %.3f" % (p, mad, road_x(prev, True), road_x(cand, False), road_x(cand, True)))


def stitch(out, paths):
    segs = [load(paths[0])]
    width = segs[0].width
    segs += [load(p, width) for p in paths[1:]]
    heights = [s.height for s in segs]
    overlap = [int(min(heights[i], heights[i + 1]) * OVERLAP) for i in range(len(segs) - 1)]
    total = sum(heights) - sum(overlap)
    canvas = np.zeros((total, width, 3), np.float32)
    # place from the bottom up: segment 0 at the bottom
    y = total
    prev_top_road = None
    prev_arr = None
    for i, s in enumerate(segs):
        arr = np.asarray(s).astype(np.float32)
        dx = 0
        if prev_top_road is not None:
            dx = int(round((prev_top_road - road_x(s, False)) * width))
            arr = np.roll(arr, dx, axis=1)
        h = arr.shape[0]
        top = y - h
        if i == 0:
            canvas[top:y] = arr
        else:
            ov = overlap[i - 1]
            alpha = np.linspace(1.0, 0.0, ov)[:, None, None]       # new segment fades out toward its bottom overlap, old fades in
            canvas[top + h - ov: top + h] = canvas[top + h - ov: top + h] * (1 - alpha[::-1] * 0 - alpha) + 0  # placeholder replaced below
            region = canvas[top + h - ov: top + h]
            canvas[top: top + h - ov] = arr[: h - ov]
            canvas[top + h - ov: top + h] = arr[h - ov:] * alpha + region * (1 - alpha) if False else arr[h - ov:] * (1 - alpha) + region * alpha
        prev_top_road = road_x(Image.fromarray(arr.astype(np.uint8)), True)
        y = top + ov if i > 0 else top + 0
        y = top + (overlap[i] if i < len(overlap) else 0)
    Image.fromarray(canvas.clip(0, 255).astype(np.uint8)).save(out)
    print(out, canvas.shape[1], "x", canvas.shape[0])


if __name__ == "__main__":
    cmd = sys.argv[1]
    if cmd == "score":
        score(sys.argv[2], sys.argv[3:])
    elif cmd == "stitch":
        stitch(sys.argv[2], sys.argv[3:])
