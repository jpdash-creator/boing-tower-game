"""Cuts the OpenArt round close (X) icon (T32) out of its grey background.
    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/process_close_icon.py [pick]      (default pick 1)
Output: Assets/Art/UI/ui_close_icon/ui_close_icon_01.png"""
import os
import sys

sys.path.insert(0, "E:/pylibs")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402
from scipy import ndimage as ndi  # noqa: E402
from process_art import key_image, PAD  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


def main():
    pick = int(sys.argv[1]) if len(sys.argv) > 1 else 1
    src = os.path.join(ROOT, "Art", "_candidates", "close_icon", "%d.png" % pick)
    rgba, _ = key_image(np.asarray(Image.open(src).convert("RGB")))
    lab, n = ndi.label(rgba[:, :, 3] > 24)
    biggest = 1 + int(np.argmax(ndi.sum(np.ones_like(lab), lab, range(1, n + 1))))
    rgba[lab != biggest, 3] = 0
    ys, xs = np.nonzero(rgba[:, :, 3] > 24)
    out = rgba[max(0, ys.min() - PAD): ys.max() + 1 + PAD, max(0, xs.min() - PAD): xs.max() + 1 + PAD]
    d = os.path.join(ROOT, "Assets", "Art", "UI", "ui_close_icon")
    os.makedirs(d, exist_ok=True)
    Image.fromarray(out).save(os.path.join(d, "ui_close_icon_01.png"))
    print(out.shape[1], "x", out.shape[0])


if __name__ == "__main__":
    main()
