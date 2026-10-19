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
