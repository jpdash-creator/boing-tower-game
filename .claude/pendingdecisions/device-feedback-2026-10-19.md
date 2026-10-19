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
