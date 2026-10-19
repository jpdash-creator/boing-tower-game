# Other open items waiting on the owner

| Item | Detail | Since |
|---|---|---|
| Unity license inactive for batch runs | Headless Unity exits 198: "No valid Unity Editor license found". Owner: open Unity Hub, confirm sign-in, make sure a Unity Personal license is active, then tell Claude. Blocks compile check and project setup. | 2026-10-19 |
| **BLOCKER for T1:** owner re-picks art images | Owner will re-pick all 66 assets; T1 (art pipeline) and sprite-dependent tasks T5/T7 wait for the final picks. Default image 1 is a placeholder only. | 2026-10-19 |
| T3/T4 owner questions (design) | (1) Coin-star targets (25-95) are mostly met just by climbing, since landing coins give about 1 per row: raise targets or cut landing coins? (2) Speedrun "90 floors in 45 s" is infeasible at doc 08 gaps (builder halves the gaps): change content value or physics? (3) Does a level end at goal completion (then star 2 needs the run to continue)? (4) Hidden gem: add 1 gem to the wallet or only count it? (5) Combo landing coin = 2 total (not 1+2)? (6) Gong wave on every region for SurviveWave, ok? Details: `Assets/_Game/Simulation/NOTES.md`. | 2026-10-19 |
| T8 economy assumptions | Star bonus +10 percent per extra star; failed run pays nothing; pickups pay in full on replay; gem skip only reaches the NEXT region; levels unlock in order; hidden gem pays no gems; playable-region limit (5) is code, content still marks regions 6-20 live. Details: `Assets/_Game/Economy/NOTES.md`. | 2026-10-19 |
| Verify default asset picks | See `asset-picks.md` (66 assets) | 2026-10-19 |
| Second Unity Editor 6000.6.6f1 | Installed by the Hub queue (about 5 GB on E:). Project uses 6000.3.26f1. Keep or uninstall? | 2026-10-18 |
| Windows hibernation file | `hiberfil.sys` is 12.8 GB on C:. Owner can free it with `powercfg /h off` (administrator terminal). Claude will not change system settings. | 2026-10-18 |
| Unidentified temp files on C: | Webex `wbxtra_*.wbt` (about 1 GB), VS Code installer (232 MB), Diagnostics (208 MB). Not Claude's files; delete only with owner OK. | 2026-10-18 |
| Signature platform kinds in the generator | Region special platforms are not generated or bot-verified yet. | 2026-10-18 |
| Remaining art (not approved) | Outfits, boots, hats, accessories, extra trails, 15 characters, regions 6-20, shop and pack art. Each batch needs a new approval packet. | 2026-10-18 |
