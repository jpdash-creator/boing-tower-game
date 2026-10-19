# Art in Unity (roadmap T1)

Final art = the owner's picks (`Content/assets/owner_picks_2026-10-19.txt`) run through `Tools/art_pipeline/process_art.py`
(`set PYTHONPATH=E:/pylibs`, SciPy lives on E:). Originals stay in `Art/_candidates/` (outside Unity). Re-run is safe; it reads the picked candidate, not this folder.

## Layout
- `full` images (backgrounds, world map, loading screen, app icon): one PNG at the final path.
- `single` (coin, gem, stars, Momo): `<dir>/<asset id>/<asset id>.png`, background removed and trimmed.
- `sheet` (tilesets, props, signature art, pickups, UI sheets, VFX, trail): `<dir>/<asset id>/<asset id>_NN.png` (NN row-major) plus `sprites.json` (source boxes).
- Tilesets also have `tileset_map.json`: 1-based sprite index per kind (normal, narrow, moving, crumble, bounce, wallL, wallR) for each region.
- Import rules (`Assets/Editor/ProjectBootstrap/ArtImportPostprocessor.cs`): Sprite, 256 px per unit, no mipmaps, ASTC 6x6 on Android and iOS.
- 66 assets processed: 296 sprites from 44 sheets, plus 16 full images and 5 singles (incl. Momo). Tests: `ArtImportTests` (4) in Unity EditMode.

## Keying
Each sheet has its own flat blue-grey background (NOT #808080): the key colour is the median of the image border. Only background connected to the border is removed, so silver or grey interior areas stay opaque; neon glow keeps soft partial alpha. Touching objects are split at the thinnest row/column for sheets with a known count (coin sheet 6, orbs 6, rarity frames 5).

## Known limitations (owner decisions listed in .claude/pendingdecisions/open-items.md)
1. Platforms are single non-tileable images with clouds, arrows and scenery painted in (the moving platform has a left-right arrow, the bounce pad an up arrow). Scale them in X for platform width; expect squash on very wide platforms.
2. Region 3 (Sunny Beach) and region 5 (Bamboo Temple) tilesets are irregular (palm island, water pool, curved ramps, baked-in bamboo scenery). The map notes explain the index choices. Candidates for regeneration (needs an approval packet and credits).
3. Parallax mid and near backgrounds are opaque full images: the model did not produce transparent skies or centres, so they cannot be overlaid as separate layers without extra masking. Use them as stacked or crossfaded layers for now.
4. Momo is one flat sprite (no paper-doll layers). Animation is procedural (squash and stretch) until a rig exists.
5. UI sheets are sliced into elements but 9-slice borders are not defined yet (panels, buttons): do that when the UI is built (T7).

## Art QA (CUX-9) - `Tools/art_pipeline/qa_art.py`
Run after `process_art.py` (`PYTHONPATH=E:/pylibs`). It writes `Assets/Art/QA_REPORT.md` and one contact sheet per sheet to `E:/Unity/qa` and exits 1 on clipped or grey-halo sprites.
Fixed on 2026-10-19: glow sheets (`vfx_*`, `ui_hud_icons`) are keyed globally so the grey disc inside rings disappears; hairline debris (crosshairs) is no longer cut as a sprite.
Known issues kept on purpose (none is used by the 5-region build):
- `vfx_coin_burst`: grey square backdrop survives (white-frame key). Do not use until regenerated.
- `vfx_shockwave_ring_03`: a thin arc fragment after global keying; skip it.
- Beach (reg003) and bamboo (reg005) tilesets: irregular silhouettes, arrow baked into the moving platform; T15 regeneration.
- `ui_panel_popup`: baked "X" in the corner; the game overlays an invisible real close button on it (`UiKit.AddHitArea`).
