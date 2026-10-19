"""One step of the chained world-map generation (T33 v5).

    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/chain_step.py pick <N> <url1> <url2>
        Downloads the two candidates of segment N to Art/_candidates/map_v5_NN/, scores each against the previous segment's top strip
        (mean absolute colour difference over the 20 percent overlap, lower is better), saves the better one as Art/Map/segments/map_seg_NN.png,
        writes its top 18 percent strip to E:/Unity/logs/strip_in.png (the next job's 'Previous segment') and a thumbnail to E:/Unity/qa.
        A different candidate can be forced with --force 1|2.
"""
import os
import sys
import urllib.request

sys.path.insert(0, "E:/pylibs")
import numpy as np  # noqa: E402
from PIL import Image, ImageFilter  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))
SEG = os.path.join(ROOT, "Art", "Map", "segments")
OVERLAP = 0.20


def mad(prev, cand):
    n = int(min(prev.height, cand.height) * OVERLAP)
    a = np.asarray(prev.crop((0, 0, prev.width, n)).filter(ImageFilter.GaussianBlur(6))).astype(np.float32)
    b = np.asarray(cand.crop((0, cand.height - n, cand.width, cand.height)).filter(ImageFilter.GaussianBlur(6))).astype(np.float32)
    return float(np.abs(a - b).mean())


def main():
    n = int(sys.argv[2])
    urls = [u for u in sys.argv[3:] if u.startswith("http")]
    force = int(sys.argv[sys.argv.index("--force") + 1]) if "--force" in sys.argv else 0
    d = os.path.join(ROOT, "Art", "_candidates", "map_v5_%02d" % n)
    os.makedirs(d, exist_ok=True)
    prev = Image.open(os.path.join(SEG, "map_seg_%02d.png" % (n - 1))).convert("RGB")
    scores = []
    for i, u in enumerate(urls, start=1):
        p = os.path.join(d, "%d.png" % i)
        req = urllib.request.Request(u, headers={"User-Agent": "Mozilla/5.0"})
        with urllib.request.urlopen(req) as r, open(p, "wb") as fh:
            fh.write(r.read())
        c = Image.open(p).convert("RGB")
        if c.width != prev.width:
            c = c.resize((prev.width, round(c.height * prev.width / c.width)))
        scores.append(mad(prev, c))
        print("candidate %d seamMAD %.1f" % (i, scores[-1]))
    best = force if force else 1 + int(np.argmin(scores))
    img = Image.open(os.path.join(d, "%d.png" % best)).convert("RGB")
    img.save(os.path.join(SEG, "map_seg_%02d.png" % n))
    W, H = img.size
    img.crop((0, 0, W, int(0.18 * H))).save("E:/Unity/logs/strip_in.png")
    th = img.copy()
    th.thumbnail((560, 996))
    both = Image.new("RGB", (th.width * 2 + 10, th.height), (20, 20, 20))
    both.paste(th, (0, 0))
    o = Image.open(os.path.join(d, "%d.png" % (3 - best if len(urls) == 2 else best))).convert("RGB")
    o.thumbnail((560, 996))
    both.paste(o, (th.width + 10, 0))
    both.save("E:/Unity/qa/map_v5_%02d.png" % n)
    print("selected candidate", best, "-> map_seg_%02d.png; strip bytes" % n, os.path.getsize("E:/Unity/logs/strip_in.png"))


if __name__ == "__main__":
    main()
