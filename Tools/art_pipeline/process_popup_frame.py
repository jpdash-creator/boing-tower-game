"""Cuts the OpenArt popup frame (T28) out of its grey background and trims it to the frame, ready for 9-slice use.
    set PYTHONPATH=E:/pylibs
    python Tools/art_pipeline/process_popup_frame.py [pick]      (default pick 2: the flat, evenly bordered frame)
Output: Assets/Art/UI/ui_popup_frame/ui_popup_frame_01.png. Borders (140 px) and the sprite import are set by ArtImportPostprocessor.
"""
import os
import sys

sys.path.insert(0, "E:/pylibs")
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import numpy as np  # noqa: E402
from PIL import Image  # noqa: E402
from process_art import key_image, PAD  # noqa: E402

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


def main():
    pick = int(sys.argv[1]) if len(sys.argv) > 1 else 2
    src = os.path.join(ROOT, "Art", "_candidates", "popup_frame", "%d.png" % pick)
    rgba, _bg = key_image(np.asarray(Image.open(src).convert("RGB")))
    from scipy import ndimage as ndi
    lab, n = ndi.label(rgba[:, :, 3] > 24)
    biggest = 1 + int(np.argmax(ndi.sum(np.ones_like(lab), lab, range(1, n + 1))))   # the frame; stray specks are dropped
    rgba[lab != biggest, 3] = 0
    ys, xs = np.nonzero(rgba[:, :, 3] > 24)
    out = rgba[max(0, ys.min() - PAD): ys.max() + 1 + PAD, max(0, xs.min() - PAD): xs.max() + 1 + PAD]
    dst_dir = os.path.join(ROOT, "Assets", "Art", "UI", "ui_popup_frame")
    os.makedirs(dst_dir, exist_ok=True)
    dst = os.path.join(dst_dir, "ui_popup_frame_01.png")
    Image.fromarray(out).save(dst)
    print(dst, out.shape[1], "x", out.shape[0])


if __name__ == "__main__":
    main()
