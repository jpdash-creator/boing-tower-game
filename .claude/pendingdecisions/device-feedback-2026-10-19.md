# Owner device test, OnePlus 13 (CPH2649), first Android build, 2026-10-19

Verbatim themes (10 points) and the answers given via AskUserQuestion. Wording about other games is intentionally not recorded here (originality rule): the owner wants the classic tower-jumper feel.

1. UI looks OK but has many flaws; test it hard on device.
2. Visible bottom buttons are needed: Left and Right (bottom-left) and Jump (bottom-right). The owner could not understand the gameplay. DECIDED: three on-screen buttons, touch zones removed, left-handed swaps sides.
3. Platforms look squished. DECIDED: stop stretching; use the art at its real aspect ratio (variant chosen by width). Test once; if still bad, regenerate with OpenArt (needs a credit approval packet).
4. Not polished on every screen. DECIDED: a screen-by-screen polish pass on device screenshots.
5. Wants an automated Android test kit (Playwright-like). Plan: adb-driven harness plus an in-game UI-tree dump over logcat (see roadmap T19).
6. No platform below the player did not kill. DECIDED: auto-scroll upward; falling below the bottom of the screen (the danger line) ends the run.
7. Character floats above the platform. DECIDED: align the feet to the platform surface using measured surface offsets per sprite.
8. Platforms must keep moving down the screen so there is always fear of dying (auto-scroll that speeds up with height).
9. Timer not visible; remove the timed option. DECIDED: remove Speedrun goal and the time star; Speedrun levels become reach-floor goals; third star becomes a skill star; revive with gems stays.
10. Make it feel real like the classic tower-jumper (momentum, combos, wall bounce, rising pressure).

## Second device session (same day), points and decisions
1. World map: numbers drawn over the region icons and padlocks. DECIDED (by me, reversible): remove the numbers on map nodes; the name label stays.
2. Fonts are not liked anywhere. DECIDED: free OFL game fonts for all text (needs the owner's OK to download the font files), TextMeshPro or Font assets, sample 3 pairings first. OpenArt/Seedance is not used for fonts (Seedance Lite is a video model). Optional OpenArt logo wordmark was NOT chosen.
3. Level 1 node not centered in its bubble: fix centering of the number inside the node art.
4. The screen must scroll: after the first jump the camera climbs, platforms and background slide down, speed steps up every 5 s; dropping below the bottom edge ends the run. No red band or hazard from below. DECIDED.
5. Coins must not sit inside a platform: on a platform top or in clear air. DECIDED.
6. Galaxy portal at the top: the goal; Momo jumps into it to clear the level. DECIDED.
7. Platform count x5: DECIDED as longer levels plus dense patches of SMALLER platforms (jump choice matters, some can fall/crumble); first few platforms fixed, then a randomizer changes the pattern every 15 s. Smaller platform art: use code-drawn slabs first, OpenArt only if needed (approval packet first).

## Issue list 4 (same day) and decisions
1. Map region names: ribbon banner art under each node, no translucent black boxes. DECIDED.
2. Scroll speed: base +20% over the current speed, 5 selectable difficulty levels in Settings (dropdown) with growth per 5 s of 5, 10, 15, 20, 25 percent. DECIDED by the owner.
   Gem chance in place of a coin: 5% default (levels 5% and 10%), 7% at 15%, 9% at 20%, 10% at 25% (max 10%). Later in-app purchases may sell extra gem chance (recorded with purchases).
   RISK TO REVIEW: 5-10% of coins becoming gems is a large hard-currency inflow (about 10 gems per level) versus doc 08/09 where gems are scarce and monetised; needs an economy check (per-level gem cap or coin-count scaling) before launch.
3. Text overlapping images: add automated text-vs-image overlap tests everywhere.
4. Popups (you fell, level clear, pause, level info) look stretched and the painted X does not work. DECIDED: new clean popup frame via OpenArt (needs an approval packet; nothing generated yet). Until then the code-drawn panel with a real X button is the stopgap.
5. Revive cost: 2 gems the first time, 5 gems the second (max 2 revives per run). DECIDED.
6. Feet and coins must stand ON the platform surface, not on the glow line: per-piece surface offset (walkable line is the top of the grass, not the neon rim).

## Issue list 5 (2026-10-20) and decisions
1. Narrow platforms need smaller gaps so one jump always works (the owner lost 3 runs). DECIDED: gap and sideways offset limits shrink with platform width; bot-verified.
2. ECONOMY V2 (owner): coins are NOT collected in the level. They are paid at the end.
   - Each level has 3 collectible stars (side routes and risky spots) plus occasional gems with rarity.
   - End payout: clear 30 coins + 40 per star collected + 5 per combo step. Scaled by region as today.
   - Gem rarity: Common 1, Rare 2, Epic 5, Legendary 10 gems, chances 70/22/7/1 percent per gem spot. Difficulty setting raises the number of gem spots (5 to 10 percent).
   - After a level has 3 stars: replays pay 50% coins and drop no gems. Levels with fewer stars keep full rewards.
   - The old coin-pickup stars (clear/coins/no revive) and the CollectCoins goal type are retired.
3. The 'You fell!' popup and the X still look bad: DECIDED compact card (no big ribbon, big title in the display font, round X icon generated with OpenArt, panel only as tall as its content). OpenArt packet needed.
4. Level-select wallet coin icon looks out of place: fix wallet icon/number alignment.
5. WORLD MAP V2: scrollable vertical map with all 30 regions and many 'coming soon'. DECIDED by the owner: 10 chained vertical OpenArt segments of 3 regions each (richer detail, higher cost accepted). Region marker = small pin on the path plus a slim name tag beside it, never covering the art. Needs an OpenArt approval packet.
6. Map icons did not fit the map art (Meadow Springs and the locked ones): redo markers with the new map.
7. Level info popup (Goal, stars, PLAY): fix position and fonts, center content.

## Map stitching (2026-10-20)
- Owner: do not stitch the 10 segments directly; separate them with a thin stream of clouds. Implemented in build_world_map.py (segments butt together, a 240 px procedural cloud band is laid over every joint; free, no OpenArt credits). Joint positions are written to world_map.json (cloudJoints). Owner to review the look on the full map.
- Owner (seg 7 pick): region order must follow the picture (Moon Carnival, Dragon Peak, Space Station from bottom); content region order + golden hashes to be updated after the chain.
