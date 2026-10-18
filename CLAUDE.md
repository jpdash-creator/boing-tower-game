# Boing Tower - project guide for Claude

2D vertical-jumper for Android + iOS. Unity 6 LTS (C#), 2D URP. Data-driven: regions, levels, items and offers are JSON plus asset bundles, never hardcoded in scenes.

**Design docs** (source of truth, read before changing rules or numbers): `E:\Source\Game Ideas\boing-tower-docs\docs\`. Doc 08 (rules addendum) wins over docs 01, 02, 05. Doc 09 records the accepted review decisions.

## Hard rules
- **Originality:** never mention, name or imitate any existing jumper game in code, art, prompts, store text or comments. Generic genre mechanics only.
- **Determinism:** the simulation and the level generator use fixed-point/integer math and the seeded RNG (xoshiro). No `float`, `UnityEngine.Random`, `Time.deltaTime` or `Physics` in `Simulation/` or generator code. Same seed gives the same level on every device and on the server.
- **Server authority:** currencies, purchases, chests and leaderboard scores are decided server-side. No secrets in the client.
- **Cosmetics only:** items never change gameplay stats. Ranked runs disallow consumables and permanent upgrades.
- **IDs are permanent:** content IDs (`region_021_dragon_peak`, `lvl_021_01`, `boots_cloud_runner`) never change once shipped.
- **No allocations** in gameplay loops. Pool platforms and particles.
- Behaviour is code (rare app updates); content is data + bundles (frequent). Only assets and data are downloaded, never code.

## Layout
```
Assets/_Game/{Core,Gameplay,Simulation,Content,Economy,Backend,Monetization,UI,Audio,Tools}  one asmdef per module (add when code appears)
Assets/Art/                        source art, built into Addressables groups
Content/schemas/*.schema.json      JSON schemas (RegionDef, LevelDef, ItemDef, GemPackDef, SignatureModuleDef)
Content/{regions,levels,items,economy,config}  shipped content data
Tools/content_cli/                 gen_content.py (baseline generator), content.py (validator)
Tests/{content,EditMode,PlayMode,Simulation}
```

## Commands (run from repo root; needs Python 3.10+, stdlib only)
```
python Tools/content_cli/content.py validate         # schemas + doc 08 rule checks; run before every commit
python -m unittest discover -s Tests/content -v      # CLI and shipped-content tests
python Tools/content_cli/gen_content.py              # regenerate baseline content from doc 08 (overwrites Content/regions, levels, economy, items, config)
```
`gen_content.py` regenerates generated files; if you hand-edit content, change the generator or stop using it for that file.

## Key numbers (doc 08)
- 30 regions x 15 levels; regions 1-20 `live`, 21-30 `teaser`. Level classes: L5/L10 complex, L15 highly complex.
- Region offset `d_eff = d + offset * (1 - d)`, offset 0.0 to 0.25 across regions 1-30.
- Endless speed: +4% per 25 floors, cap +150%. Constants in `Content/config/remote_config.json`.
- Coins : gems : USD = 100 : 1 : about $0.011. Gem packs in `Content/economy/gem_packs.json`.
- Score = floors x 10 + combo^2 x 5 + coins. Floor = platform row index. Screen about 10 units wide. These are prototype values.

## Open / next
- Physics constants are undefined until the prototype (doc 09 D4). Freeze them before building the generator and solvability bot.
- Unity project files (ProjectSettings, manifest) are not created yet: open the folder in Unity 6 LTS Hub to generate them.
- Signature modules (`Content/config/modules.json`) are all `planned`; track `buildStatus` there.

## Conventions
C# 9+, assembly definitions per module, DI via VContainer or a small service locator plus an event bus, no scattered singletons. Tests: EditMode for pure logic, a Simulation suite with golden replays. Art follows `Art/<Class>/<id>_<layer>.png`, lowercase snake_case, prompts and seeds logged in `assets_log.csv`.
