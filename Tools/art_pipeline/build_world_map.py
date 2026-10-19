"""Stitches the chained world-map segments into the scrolling map the game loads (T33).

    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/build_world_map.py

Reads Art/Map/segments/map_seg_01.png, 02, ... (bottom to top). Each segment overlaps the one below by 20 percent of its height; the overlap is
cross-faded with a smooth ramp. The result is scaled to 1080 px wide and cut into chunks of 3840 px (a phone texture limit is safer than one
giant image). Writes Assets/Resources/Map/world_chunk_NN.png (chunk 01 = the bottom) and Assets/Resources/Map/world_map.json.
"""
import glob
import json
import os
import sys

sys.path.insert(0, "E:/pylibs")
import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
OVERLAP = 0.20
WIDTH = 1080
CHUNK = 3840


def main():
    files = sorted(glob.glob(os.path.join(ROOT, "Art", "Map", "segments", "map_seg_*.png")))
    segs = []
    for f in files:
        im = Image.open(f).convert("RGB")
        im = im.resize((WIDTH, round(im.height * WIDTH / im.width)), Image.LANCZOS)
        segs.append(np.asarray(im).astype(np.float32))
    ov = [int(min(segs[i].shape[0], segs[i + 1].shape[0]) * OVERLAP) for i in range(len(segs) - 1)]
    total = sum(s.shape[0] for s in segs) - sum(ov)
    canvas = np.zeros((total, WIDTH, 3), np.float32)
    y = total                                   # bottom of the next segment to place (canvas y grows downward)
    seg_bottoms = []
    for i, s in enumerate(segs):
        h = s.shape[0]
        top = y - h
        seg_bottoms.append(y)
        if i == 0:
            canvas[top:y] = s
        else:
            o = ov[i - 1]
            t = np.linspace(0.0, 1.0, o)[:, None, None]
            w = t * t * (3 - 2 * t)             # smoothstep: weight of the NEW segment rises toward the top of... see below
            # new segment's bottom o rows overlay the old segment's top o rows; new weight is 1 at the bottom, 0 at the top of the overlap? no:
            # the old segment is complete below the overlap, the new one is complete above it, so the new weight rises going UP.
            w = w[::-1]
            region = canvas[top + h - o: top + h]
            canvas[top + h - o: top + h] = s[h - o:] * (1 - w) * 0 + region * w + s[h - o:] * (1 - w)
            canvas[top: top + h - o] = s[: h - o]
        y = top + (ov[i] if i < len(ov) else 0)
    out_dir = os.path.join(ROOT, "Assets", "Resources", "Map")
    os.makedirs(out_dir, exist_ok=True)
    for old in glob.glob(os.path.join(out_dir, "world_chunk_*.png")):
        os.remove(old)
    chunks = []
    n = 1
    bottom = total
    while bottom > 0:
        top = max(0, bottom - CHUNK)
        Image.fromarray(canvas[top:bottom].clip(0, 255).astype(np.uint8)).save(os.path.join(out_dir, "world_chunk_%02d.png" % n))
        chunks.append({"file": "world_chunk_%02d" % n, "height": bottom - top})
        bottom = top
        n += 1
    meta = {"width": WIDTH, "totalHeight": total, "segments": len(segs), "chunks": chunks}
    with open(os.path.join(out_dir, "world_map.json"), "w", encoding="utf-8", newline="\n") as fh:
        json.dump(meta, fh, indent=2)
    print("segments", len(segs), "total", WIDTH, "x", total, "chunks", len(chunks))


if __name__ == "__main__":
    main()
