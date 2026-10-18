"""Builds an owner-approval packet (Markdown) for a set of manifest assets. Read-only: spends nothing.

    python Tools/asset_cli/make_packet.py <packet-name> "<title>" <balance> <credits-per-image> <asset-id> [<asset-id> ...]

Prompts are copied from Content/assets/manifest.json programmatically so they are exact (never retyped).
The cost line uses the flat per-image price you pass in (verify it first with openart_model_cost).
"""
import json
import os
import sys

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "..", ".."))


def main():
    name, title, balance, per_image = sys.argv[1], sys.argv[2], int(sys.argv[3]), int(sys.argv[4])
    ids = sys.argv[5:]
    with open(os.path.join(ROOT, "Content", "assets", "manifest.json"), encoding="utf-8") as f:
        m = {a["id"]: a for a in json.load(f)["assets"]}
    calls = [m[i] for i in ids]
    total_imgs = sum(c.get("fixed_images", c["candidates"]) for c in calls)
    total = total_imgs * per_image
    L = ["# Approval packet: %s - %s" % (name, title), "",
         "> Prepared with READ-ONLY OpenArt calls. Nothing has been generated. Credits are spent only after the owner replies with a clear yes to THIS packet.", "",
         "## 1. What and why",
         "- Assets: %d (%s)" % (len(calls), ", ".join(ids)),
         "- Calls: %d (one per asset), %d images in total" % (len(calls), total_imgs),
         "- Locked model: `byte-plus-seedream-5-lite` (owner pick in B00). Style reference: B01a image 4 (resource `%s`)." %
         (calls[0].get("visualReferences", [{}])[-1].get("id", "")), "",
         "## 2. MCP and tools",
         "- OpenArt MCP (`mcp__73f004bd-92f2-4e13-8abf-f0c13bc6ba8f__`), `openart_generate_image` only, %d calls." % len(calls), "",
         "## 3. Calls (exact settings and prompts)", "",
         "| # | Asset id | Mode | Resolution | Aspect | Images | Credits |", "|---|---|---|---|---|---|---|"]
    for n, c in enumerate(calls, 1):
        k = c.get("fixed_images", c["candidates"])
        L.append("| %d | %s | %s | 2K | %s | %d | %d |" % (n, c["id"], c["mode"], c["aspect"], k, k * per_image))
    L += ["", "Every call: `autoEnhancePrompt: false`, no seed field exists, `visualReferences` = the style bible (id/url above).", ""]
    for n, c in enumerate(calls, 1):
        refs = ", ".join("%s (%s)" % (v["label"], v["id"]) for v in c.get("visualReferences", []))
        L += ["### Call %d - %s (%s)" % (n, c["id"], c["name"]), "- Save to: `%s`" % c["path"], "- Purpose: " + c["purpose"],
              "- Mode / aspect / images: %s / %s / %d" % (c["mode"], c["aspect"], c.get("fixed_images", c["candidates"])), "- Reference image(s): " + (refs or "none"), "```", c["prompt"], "```", ""]
    L += ["## 4. Cost",
          "- Per image: %d credits (flat, from `openart_model_cost`). Images: %d. **Total: %d credits.**" % (per_image, total_imgs, total),
          "- Balance now: %d | after: %d" % (balance, balance - total), "",
          "## 5. After generation", "- Owner picks one image per asset (by number). Picks are logged in `assets_log.csv`.",
          "- Post-processing (key removal, trim, mirror boots, slice) costs no credits.", "",
          "## 6. Approval", "Reply \"approve %s\" to run exactly these %d calls and %d credits. Any change needs a new packet." % (name, len(calls), total), ""]
    out = os.path.join(ROOT, "Content", "assets", "approvals", name + ".md")
    with open(out, "w", encoding="utf-8", newline="\n") as f:
        f.write("\n".join(L))
    print(out, "calls", len(calls), "images", total_imgs, "credits", total)


if __name__ == "__main__":
    main()
