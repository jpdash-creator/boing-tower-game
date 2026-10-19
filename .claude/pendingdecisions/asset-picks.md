# Pending decision: verify default asset picks (image 1)

Status: **CLOSED 2026-10-19 (owner picked all 66 assets; see Content/assets/owner_picks_2026-10-19.txt)**. Original request: - requested by the owner on 2026-10-19: "Start with image 1 for default, but log an input from my side ... for final verification".

All 66 Tier 1 assets use **image 1** as a placeholder pick. Nothing is final until the owner verifies each one. Unverified picks must not be used for store submission.

> **2026-10-19 (owner):** re-picked all 66 assets; picks are final and applied to `Assets/Art`. T1 is unblocked. Further swaps: tell Claude `asset_id: n`.

**How to review:** open `Art/_pick/index.html` (local contact sheet; regenerate with `python Tools/asset_cli/make_contact_sheet.py`), or the files in `Art/_candidates/<asset id>/`. Reply with e.g. `reg001_tileset: 3` to change a pick, or `ok <asset id>` / `ok all high` to confirm.

**Check each image for:** stray text or labels, copied reference layout (single subject expected, not a sheet copy), matches the style bible and Momo, clean outline, glow staying off the grey background, readable at small size, tiles with usable edges.

**Not yet reviewed by Claude or the owner.** Transparency ("transparent sky/centre" prompts) was NOT achieved by the model; background removal happens in post-processing.

| Verified | Priority | Asset id | Class | Default pick | Candidates | Placeholder path |
|---|---|---|---|---|---|---|
| [x] | High | cur_coin_single | currency | 1 | 2 | `Assets/Art/Currency/cur_coin_single.png` |
| [x] | High | cur_gem_single | currency | 1 | 2 | `Assets/Art/Currency/cur_gem_single.png` |
| [x] | High | cur_star_empty | currency | 1 | 2 | `Assets/Art/Currency/cur_star_empty.png` |
| [x] | High | cur_star_filled | currency | 1 | 2 | `Assets/Art/Currency/cur_star_filled.png` |
| [x] | High | fnd_app_icon | foundation | 2 | 6 | `Assets/Art/Marketing/icon/app_icon_concept_v1.png` |
| [x] | High | map_region_map | map | 4 | 4 | `Assets/Art/Map/map_world_v1.png` |
| [x] | High | pck_coin_sheet | pickup | 3 | 3 | `Assets/Art/Pickups/pickup_coin_sheet.png` |
| [x] | High | pck_gem_sheet | pickup | 1 | 3 | `Assets/Art/Pickups/pickup_gem_sheet.png` |
| [x] | High | reg001_tileset | region_tileset | 3 | 4 | `Assets/Art/Regions/region_001_meadow_springs/tiles/tileset_meadow_springs_v1.png` |
| [x] | High | reg002_tileset | region_tileset | 4 | 4 | `Assets/Art/Regions/region_002_mushroom_grove/tiles/tileset_mushroom_grove_v1.png` |
| [x] | High | reg003_tileset | region_tileset | 3 | 4 | `Assets/Art/Regions/region_003_sunny_beach/tiles/tileset_sunny_beach_v1.png` |
| [x] | High | reg004_tileset | region_tileset | 3 | 4 | `Assets/Art/Regions/region_004_candy_canyon/tiles/tileset_candy_canyon_v1.png` |
| [x] | High | reg005_tileset | region_tileset | 3 | 4 | `Assets/Art/Regions/region_005_bamboo_temple/tiles/tileset_bamboo_temple_v1.png` |
| [x] | High | trl_rainbow_ribbon | trail | 1 | 3 | `Assets/Art/Items/trail/trail_rainbow_ribbon_sheet.png` |
| [x] | High | ui_buttons_primary | ui_kit | 3 | 3 | `Assets/Art/UI/ui_buttons_primary.png` |
| [x] | High | ui_hud_icons | ui_kit | 3 | 3 | `Assets/Art/UI/ui_hud_icons.png` |
| [x] | High | ui_level_nodes | ui_kit | 1 | 3 | `Assets/Art/UI/ui_level_nodes.png` |
| [x] | High | ui_loading_screen_art | ui_kit | 1 | 3 | `Assets/Art/UI/ui_loading_screen_art.png` |
| [x] | High | ui_map_nodes | ui_kit | 2 | 3 | `Assets/Art/UI/ui_map_nodes.png` |
| [x] | High | ui_panel_glass | ui_kit | 1 | 3 | `Assets/Art/UI/ui_panel_glass.png` |
| [x] | Medium | pck_orbs | pickup | 1 | 3 | `Assets/Art/Pickups/pickup_orbs_sheet.png` |
| [x] | Medium | reg001_bg_far | region_background | 1 | 3 | `Assets/Art/Regions/region_001_meadow_springs/bg_far/bg_meadow_springs_far_v1.png` |
| [x] | Medium | reg001_bg_mid | region_background | 2 | 3 | `Assets/Art/Regions/region_001_meadow_springs/bg_mid/bg_meadow_springs_mid_v1.png` |
| [x] | Medium | reg001_bg_near | region_background | 3 | 3 | `Assets/Art/Regions/region_001_meadow_springs/bg_near/bg_meadow_springs_near_v1.png` |
| [x] | Medium | reg001_props | region_props | 1 | 3 | `Assets/Art/Regions/region_001_meadow_springs/props/props_meadow_springs_v1.png` |
| [x] | Medium | reg001_signature | region_signature | 1 | 3 | `Assets/Art/Regions/region_001_meadow_springs/hazards/signature_meadow_springs_v1.png` |
| [x] | Medium | reg002_bg_far | region_background | 1 | 3 | `Assets/Art/Regions/region_002_mushroom_grove/bg_far/bg_mushroom_grove_far_v1.png` |
| [x] | Medium | reg002_bg_mid | region_background | 1 | 3 | `Assets/Art/Regions/region_002_mushroom_grove/bg_mid/bg_mushroom_grove_mid_v1.png` |
| [x] | Medium | reg002_bg_near | region_background | 2 | 3 | `Assets/Art/Regions/region_002_mushroom_grove/bg_near/bg_mushroom_grove_near_v1.png` |
| [x] | Medium | reg002_props | region_props | 1 | 3 | `Assets/Art/Regions/region_002_mushroom_grove/props/props_mushroom_grove_v1.png` |
| [x] | Medium | reg002_signature | region_signature | 3 | 3 | `Assets/Art/Regions/region_002_mushroom_grove/hazards/signature_mushroom_grove_v1.png` |
| [x] | Medium | reg003_bg_far | region_background | 1 | 3 | `Assets/Art/Regions/region_003_sunny_beach/bg_far/bg_sunny_beach_far_v1.png` |
| [x] | Medium | reg003_bg_mid | region_background | 1 | 3 | `Assets/Art/Regions/region_003_sunny_beach/bg_mid/bg_sunny_beach_mid_v1.png` |
| [x] | Medium | reg003_bg_near | region_background | 2 | 3 | `Assets/Art/Regions/region_003_sunny_beach/bg_near/bg_sunny_beach_near_v1.png` |
| [x] | Medium | reg003_props | region_props | 3 | 3 | `Assets/Art/Regions/region_003_sunny_beach/props/props_sunny_beach_v1.png` |
| [x] | Medium | reg003_signature | region_signature | 3 | 3 | `Assets/Art/Regions/region_003_sunny_beach/hazards/signature_sunny_beach_v1.png` |
| [x] | Medium | reg004_bg_far | region_background | 3 | 3 | `Assets/Art/Regions/region_004_candy_canyon/bg_far/bg_candy_canyon_far_v1.png` |
| [x] | Medium | reg004_bg_mid | region_background | 1 | 3 | `Assets/Art/Regions/region_004_candy_canyon/bg_mid/bg_candy_canyon_mid_v1.png` |
| [x] | Medium | reg004_bg_near | region_background | 2 | 3 | `Assets/Art/Regions/region_004_candy_canyon/bg_near/bg_candy_canyon_near_v1.png` |
| [x] | Medium | reg004_props | region_props | 3 | 3 | `Assets/Art/Regions/region_004_candy_canyon/props/props_candy_canyon_v1.png` |
| [x] | Medium | reg004_signature | region_signature | 2 | 3 | `Assets/Art/Regions/region_004_candy_canyon/hazards/signature_candy_canyon_v1.png` |
| [x] | Medium | reg005_bg_far | region_background | 1 | 3 | `Assets/Art/Regions/region_005_bamboo_temple/bg_far/bg_bamboo_temple_far_v1.png` |
| [x] | Medium | reg005_bg_mid | region_background | 3 | 3 | `Assets/Art/Regions/region_005_bamboo_temple/bg_mid/bg_bamboo_temple_mid_v1.png` |
| [x] | Medium | reg005_bg_near | region_background | 2 | 3 | `Assets/Art/Regions/region_005_bamboo_temple/bg_near/bg_bamboo_temple_near_v1.png` |
| [x] | Medium | reg005_props | region_props | 2 | 3 | `Assets/Art/Regions/region_005_bamboo_temple/props/props_bamboo_temple_v1.png` |
| [x] | Medium | reg005_signature | region_signature | 2 | 3 | `Assets/Art/Regions/region_005_bamboo_temple/hazards/signature_bamboo_temple_v1.png` |
| [x] | Medium | ui_banners_ribbons | ui_kit | 3 | 3 | `Assets/Art/UI/ui_banners_ribbons.png` |
| [x] | Medium | ui_buttons_icon_round | ui_kit | 2 | 3 | `Assets/Art/UI/ui_buttons_icon_round.png` |
| [x] | Medium | ui_dividers_corners | ui_kit | 2 | 3 | `Assets/Art/UI/ui_dividers_corners.png` |
| [x] | Medium | ui_frames_rarity | ui_kit | 3 | 3 | `Assets/Art/UI/ui_frames_rarity.png` |
| [x] | Medium | ui_keyboard_free_input | ui_kit | 1 | 3 | `Assets/Art/UI/ui_keyboard_free_input.png` |
| [x] | Medium | ui_loading_spinner | ui_kit | 2 | 3 | `Assets/Art/UI/ui_loading_spinner.png` |
| [x] | Medium | ui_notification_badges | ui_kit | 2 | 3 | `Assets/Art/UI/ui_notification_badges.png` |
| [x] | Medium | ui_panel_popup | ui_kit | 3 | 3 | `Assets/Art/UI/ui_panel_popup.png` |
| [x] | Medium | ui_progress_bars | ui_kit | 2 | 3 | `Assets/Art/UI/ui_progress_bars.png` |
| [x] | Medium | ui_settings_icons | ui_kit | 3 | 3 | `Assets/Art/UI/ui_settings_icons.png` |
| [x] | Medium | ui_toggles_sliders | ui_kit | 1 | 3 | `Assets/Art/UI/ui_toggles_sliders.png` |
| [x] | Medium | ui_tutorial_hints | ui_kit | 2 | 3 | `Assets/Art/UI/ui_tutorial_hints.png` |
| [x] | Medium | vfx_coin_burst | vfx | 1 | 2 | `Assets/Art/VFX/vfx_coin_burst.png` |
| [x] | Medium | vfx_confetti | vfx | 1 | 2 | `Assets/Art/VFX/vfx_confetti.png` |
| [x] | Medium | vfx_dust_puff | vfx | 1 | 2 | `Assets/Art/VFX/vfx_dust_puff.png` |
| [x] | Medium | vfx_glow_blobs | vfx | 1 | 2 | `Assets/Art/VFX/vfx_glow_blobs.png` |
| [x] | Medium | vfx_shockwave_ring | vfx | 2 | 2 | `Assets/Art/VFX/vfx_shockwave_ring.png` |
| [x] | Medium | vfx_sparkles | vfx | 1 | 2 | `Assets/Art/VFX/vfx_sparkles.png` |
| [x] | Medium | vfx_speed_streaks | vfx | 1 | 2 | `Assets/Art/VFX/vfx_speed_streaks.png` |
| [x] | Medium | vfx_wall_spark | vfx | 1 | 2 | `Assets/Art/VFX/vfx_wall_spark.png` |

Data file: `Content/assets/picks.json` (one entry per asset, `owner_verified: false`).
