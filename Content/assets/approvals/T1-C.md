# Approval packet: T1-C - Tier 1, packet C: pickups, currency basics, VFX, default trail, app icon

> Prepared with READ-ONLY OpenArt calls. Nothing has been generated. Credits are spent only after the owner replies with a clear yes to THIS packet.

## 1. What and why
- Assets: 17 (pck_coin_sheet, pck_gem_sheet, pck_orbs, cur_coin_single, cur_gem_single, cur_star_filled, cur_star_empty, vfx_shockwave_ring, vfx_speed_streaks, vfx_confetti, vfx_sparkles, vfx_dust_puff, vfx_coin_burst, vfx_wall_spark, vfx_glow_blobs, trl_rainbow_ribbon, fnd_app_icon)
- Calls: 17 (one per asset), 42 images in total
- Locked model: `byte-plus-seedream-5-lite` (owner pick in B00). Style reference: B01a image 4 (resource `vVnbzqvg1L5uSFCKydUE`).

## 2. MCP and tools
- OpenArt MCP (`mcp__73f004bd-92f2-4e13-8abf-f0c13bc6ba8f__`), `openart_generate_image` only, 17 calls.

## 3. Calls (exact settings and prompts)

| # | Asset id | Mode | Resolution | Aspect | Images | Credits |
|---|---|---|---|---|---|---|
| 1 | pck_coin_sheet | image2image | 2K | 16:9 | 3 | 45 |
| 2 | pck_gem_sheet | image2image | 2K | 16:9 | 3 | 45 |
| 3 | pck_orbs | image2image | 2K | 4:3 | 3 | 45 |
| 4 | cur_coin_single | image2image | 2K | 1:1 | 2 | 30 |
| 5 | cur_gem_single | image2image | 2K | 1:1 | 2 | 30 |
| 6 | cur_star_filled | image2image | 2K | 1:1 | 2 | 30 |
| 7 | cur_star_empty | image2image | 2K | 1:1 | 2 | 30 |
| 8 | vfx_shockwave_ring | image2image | 2K | 4:3 | 2 | 30 |
| 9 | vfx_speed_streaks | image2image | 2K | 4:3 | 2 | 30 |
| 10 | vfx_confetti | image2image | 2K | 4:3 | 2 | 30 |
| 11 | vfx_sparkles | image2image | 2K | 4:3 | 2 | 30 |
| 12 | vfx_dust_puff | image2image | 2K | 4:3 | 2 | 30 |
| 13 | vfx_coin_burst | image2image | 2K | 4:3 | 2 | 30 |
| 14 | vfx_wall_spark | image2image | 2K | 4:3 | 2 | 30 |
| 15 | vfx_glow_blobs | image2image | 2K | 4:3 | 2 | 30 |
| 16 | trl_rainbow_ribbon | image2image | 2K | 16:9 | 3 | 45 |
| 17 | fnd_app_icon | image2image | 2K | 1:1 | 6 | 90 |

Every call: `autoEnhancePrompt: false`, no seed field exists, `visualReferences` = the style bible (id/url above).

### Call 1 - pck_coin_sheet (Coin sprite sheet)
- Save to: `Art/Pickups/pickup_coin_sheet.png`
- Purpose: Spinning coin frames (6) used for pickups.
- Mode / aspect / images: image2image / 16:9 / 3
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Sprite sheet of a shiny gold coin spinning, six rotation frames in a row, equal spacing, centered, glowing rim light, star emblem without text. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 2 - pck_gem_sheet (Gem sprite sheet (3 sizes))
- Save to: `Art/Pickups/pickup_gem_sheet.png`
- Purpose: Faceted blue-pink gem in small/medium/large (pickup + UI).
- Mode / aspect / images: image2image / 16:9 / 3
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game currency: a single faceted blue-pink gem shown at three sizes (small, medium, large) side by side, glossy highlights, sparkle accents. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 3 - pck_orbs (In-run power-up orbs (6))
- Save to: `Art/Pickups/pickup_orbs_sheet.png`
- Purpose: Free in-run temporary power-ups: spring boots, rocket, magnet, shield, slow time, double coins.
- Mode / aspect / images: image2image / 4:3 / 3
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Sprite sheet of six glossy round power-up orbs in a 3 by 2 grid, each with a distinct simple symbol inside: boots on a spring, small rocket, horseshoe magnet, shield bubble, snail-curl clock without digits, two coins; bright rarity ring, readable at 64 px. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 4 - cur_coin_single (Currency art: coin single)
- Save to: `Art/Currency/cur_coin_single.png`
- Purpose: UI/currency illustration.
- Mode / aspect / images: image2image / 1:1 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game currency illustration: a single large gold coin, glossy highlights, sparkle accents. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 5 - cur_gem_single (Currency art: gem single)
- Save to: `Art/Currency/cur_gem_single.png`
- Purpose: UI/currency illustration.
- Mode / aspect / images: image2image / 1:1 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game currency illustration: a single large faceted blue-pink gem, glossy highlights, sparkle accents. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 6 - cur_star_filled (Currency art: star filled)
- Save to: `Art/Currency/cur_star_filled.png`
- Purpose: UI/currency illustration.
- Mode / aspect / images: image2image / 1:1 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game currency illustration: a glossy gold star, filled, glossy highlights, sparkle accents. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 7 - cur_star_empty (Currency art: star empty)
- Save to: `Art/Currency/cur_star_empty.png`
- Purpose: UI/currency illustration.
- Mode / aspect / images: image2image / 1:1 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game currency illustration: a glossy outline star, empty slot, glossy highlights, sparkle accents. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 8 - vfx_shockwave_ring (VFX sheet: shockwave ring)
- Save to: `Art/VFX/vfx_shockwave_ring.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: expanding shockwave rings in 6 frames, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 9 - vfx_speed_streaks (VFX sheet: speed streaks)
- Save to: `Art/VFX/vfx_speed_streaks.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: motion speed-streak lines in 4 intensity levels, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 10 - vfx_confetti (VFX sheet: confetti)
- Save to: `Art/VFX/vfx_confetti.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: confetti pieces in many colours, 12 shapes, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 11 - vfx_sparkles (VFX sheet: sparkles)
- Save to: `Art/VFX/vfx_sparkles.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: sparkle and star particle shapes, 12 variants, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 12 - vfx_dust_puff (VFX sheet: dust puff)
- Save to: `Art/VFX/vfx_dust_puff.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: landing dust puffs in 6 frames, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 13 - vfx_coin_burst (VFX sheet: coin burst)
- Save to: `Art/VFX/vfx_coin_burst.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: coin collect burst in 6 frames, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 14 - vfx_wall_spark (VFX sheet: wall spark)
- Save to: `Art/VFX/vfx_wall_spark.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: wall bounce spark burst in 6 frames, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 15 - vfx_glow_blobs (VFX sheet: glow blobs)
- Save to: `Art/VFX/vfx_glow_blobs.png`
- Purpose: Particle sprites. Colour-shift and glow are finished in Unity.
- Mode / aspect / images: image2image / 4:3 / 2
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game VFX sprite sheet: soft round glow blobs in 6 colours for baked glow on low-end devices, evenly spaced on a grid, glowing neon look. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 16 - trl_rainbow_ribbon (Trail: Rainbow Ribbon)
- Save to: `Art/Items/trail/trail_rainbow_ribbon_sheet.png`
- Purpose: Light-trail ribbon + particle sprites (doc 08 signature visual).
- Mode / aspect / images: image2image / 16:9 / 3
- Reference image(s): Style reference (vVnbzqvg1L5uSFCKydUE)
```
Match the art style of the reference image exactly (outline weight, neon rim light and glow, glossy shading, colour saturation), but draw only the new subject described here, not the reference's layout or objects. Game light-trail effect sprite sheet 'Rainbow Ribbon': smooth rainbow gradient ribbon, one seamless horizontally tileable ribbon segment and four small matching particle sprites below it, glowing neon edges. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, solid flat #808080 mid-grey background, no text, no watermark
```

### Call 17 - fnd_app_icon (App icon concepts)
- Save to: `Art/Marketing/icon/app_icon_concept_v1.png`
- Purpose: 1024x1024 app icon; Momo face bouncing, bold readable shapes.
- Mode / aspect / images: image2image / 1:1 / 6
- Reference image(s): Momo reference (JXcAM1vuW99E8S4E8AO8), Style reference (vVnbzqvg1L5uSFCKydUE)
```
Use the first reference image as the exact design of Momo and the second reference image as the art style guide (outline weight, neon rim light and glow, glossy shading, colour saturation). Mobile app icon art: Momo the round chibi cloud-fox face and shoulders, big happy eyes, magenta scarf, bouncing upward with motion sparkles, centered, bold simple shapes readable at 64 px, glowing neon rim light, full-bleed gradient sky-blue to violet background, no text. cute chibi cartoon with neon rim light and glow, thick consistent dark-hue outline, saturated gradient colors, glossy sparkle accents, top-left light, clean vector-like edges, no watermark
```

## 4. Cost
- Per image: 15 credits (flat, from `openart_model_cost`). Images: 42. **Total: 630 credits.**
- Balance now: 8105 | after: 7475

## 5. After generation
- Owner picks one image per asset (by number). Picks are logged in `assets_log.csv`.
- Post-processing (key removal, trim, mirror boots, slice) costs no credits.

## 6. Approval
Reply "approve T1-C" to run exactly these 17 calls and 630 credits. Any change needs a new packet.
