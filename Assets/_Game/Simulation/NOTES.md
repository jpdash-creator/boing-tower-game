# Simulation library: API notes and design decisions (T3 + T4)

Engine-agnostic C# 9 (asmdef `BoingTower.Simulation`, no engine references). Fixed-point `Fx` (Q16.16) and integers only, seeded `Rng`. No `float`, `System.Math`, LINQ-heavy code or JSON in this folder. `Sim.Step` never allocates (checked by a test). The sources are compiled by `Tests/Simulation/*.csproj` straight from `Assets/`, so anything that builds there builds in Unity 6.

## How Unity code builds a level and runs it

```csharp
SimConfig cfg = new SimConfig();                       // later: fill from Remote Config
LevelSpec spec = new LevelSpec {                       // map the level JSON to this (see Tests/Simulation/ContentLoader.cs)
    Seed = json.seed,
    Difficulty = LevelGenerator.EffectiveDifficulty(baseD, regionOffset),   // d_eff, as Fx
    LevelIndex = json.index,                           // 1..15
    Class = 0..4,                                      // simple, medium, hard, complex, highly complex
    Goal = new GoalSpec { Type = GoalTypes.FromId(json.goal.type), Value = .., Count = .., TimeSec = .. },
    Stars = ...,                                       // StarSpec.Kind 0 clear, 1 coins, 2 noRevive, 3 time
    Signature = SignatureKinds.FromModuleId(region.rules.signatureModule),
    Ramp = overrides.ramp, FixedChunks = overrides.fixedChunks };
LevelData data = LevelBuilder.Build(cfg, spec);        // deterministic, same on every device
Sim sim = new Sim(cfg, data, spec);                    // difficulty and ramp come from the spec
SimState s = sim.NewState();
sim.Step(ref s, SimInput.Right | SimInput.Jump);       // once per 1/60 s; Jump is an edge flag
if (GoalEvaluator.IsComplete(spec, s)) {               // check after every Step
    int stars = StarEvaluator.Stars(spec, s, revivesUsed: s.RevivesUsed, ticks: s.Tick);
}
if (s.Dead) sim.Revive(ref s);                         // after the caller's revive UI/payment succeeded
```

Rendering reads `data.Platforms` (use `sim.PlatformX(id, tick)` for moving platforms), `data.Hazards` (use `sim.HazardPosition(id, tick, out x, out y)`), `data.Pickups` with `s.PickupTaken[id]` for the collected flag, and the counters on `SimState` (`Coins`, `Gems`, `HiddenGems`, `Hits`, `Invuln`, `Combo`, `BestCombo`, `WallBounces`, `RevivesUsed`).
`SimState.Clone()` deep-copies the two arrays (`CrumbleTick`, `PickupTaken`); copy states with it, never with plain assignment, when branching.

Endless mode is unchanged: `new Sim(cfg, LevelGenerator.Generate(cfg, endlessParams), d, endless: true)`; the per-landing coin rule is off there (doc 08 D3).

## Public types (Assets/_Game/Simulation)

| file | contents |
|---|---|
| `LevelSpec.cs` | `GoalType`, `SignatureKind`, `SignatureKinds.FromModuleId/PlatformFor`, `GoalTypes.FromId`, `GoalSpec`, `StarSpec` (+ `KindFromId`), `LevelSpec` (`RowsFor()`, `TargetRow()`) |
| `Level.cs` | `Pickup`/`PickupKind`, `Hazard`/`HazardKind`, `LevelData` (arrays, hints, `ComputeHash()`) |
| `LevelBuilder.cs` | `LevelBuilder.Build(cfg, spec)` |
| `Chunks.cs` | `ChunkRow`, `Chunk`, `ChunkLibrary.All/Get` (12 authored chunks) |
| `Goals.cs` | `GoalEvaluator.IsComplete/JumpsWithFloors`, `StarEvaluator.Stars/Earned` |
| `Sim.cs` | `SimState` (new run-tracking fields), `Sim` (new ctors, `Revive`, `HazardPosition`) |
| `LevelBot.cs` | dev-tooling solver: `Solve()`, `MaxCoins(beamWidth)` |
| `SimConfig.cs` | every tunable, each documented with its unit |

`LevelGenerator.Generate` (platform-only, used by endless and the old tests) is unchanged and independent from the builder.

## Rules and decisions (all values are SimConfig fields unless noted)

**Rows.** `RowsFor()`: an explicit `Rows` wins; ReachFloor, Speedrun and BossChase use `Goal.Value`; every other goal uses 60; never below 30. `TargetRow()` is the row the goal checks (`Goal.Value` for row goals, the top row for the others).

**Signature template (doc 08 section 6).** Level 1 and 2: no signature platforms, no hazards, no chunks. Level 3: share `SignatureIntroPercent` (10). Level 4 and up: `SignatureBasePercent` (15) growing linearly with d to `SignatureMaxPercent` (40). Region 1 = `Bounce`, 2 = `BounceCap`, 3 = `Moving` (amplitude `DriftAmplitudeMin..Max` 1.5 to 3.0 u, width capped at `DriftMaxWidth` 4 u so the sway has room), 4 = `Sticky`, 5 = swinging gongs (hazards, below).
Safety rules so a special never sits alone on the only path (the bot verifies the result):
- After a `Sticky` platform the next gap is at most `StickyNextGapMax` (2.2 u) and the reach uses the sticky jump multiplier.
- After a `Bounce`/`BounceCap` platform the next gap is at most `BounceNextGapMax` (4.2 u) and the edge-to-edge distance at most `BounceNextDxMax` (3 u); two bounce platforms are never adjacent (the landing speed on a pad is set by the previous flight).
- `Moving` platforms keep the base position as the reach anchor; the bot waits for a good phase.
The `share` is applied per generated row; rows from chunks, the combo ladder and the hidden-gem row are never signature platforms.

**BounceCap.** New `PlatformKind.BounceCap = 7`, launch speed = jump speed x `BounceCapMultiplier` (1.8; Bounce stays 1.4).

**Pickups (doc 08 section 7).** `Pickup {Id, Kind, X, Y, Radius}`, collected once when the player centre (X, feet Y + `PlayerCenterY` 0.5) is within `Radius` (integer squared-distance compare). Per row: path coins on the jump arc between the previous and this spine platform (probability `PathCoinPercentEasy/Hard` 100 to 60 by d, 100 = exactly one per row), then a risky coin with probability `RiskyCoinPercentEasy/Hard` 10 to 90: either an edge side-route next to the platform or a high overshoot route above it, half of them as a pair. Multiplied by `ClassCoinPercent` (100,100,100,160,120). Result: about 70 coins on a 60-row easy level, about 115 at d = 1, about 190 on complex 60-row levels, about 350 on the 150-row L15. One rare 1-gem pickup per level with `GemChancePercent` 3 (deterministic by seed), on a risky edge spot.
In-run coins on `SimState.Coins`: landing on a new highest row +`LandingCoins` (1); +`ComboLandingCoins` (2, total, not additional) when that landing continues a combo (a qualifying multi-floor landing while `Combo > 0`); +`ExtraFloorCoins` (1) per floor gained beyond 1 (floors of the jump, capped by the new rows reached). Landing coins are off in endless. `Score` is unchanged (floors x 10 + combo^2 x 5, no coins).
`HiddenGem` pickups count in `HiddenGems`, not in `Gems`.

**Hazards: swinging gongs.** `Hazard {Id, Kind=Gong, X, Y (anchor), Radius, Period, Phase, Amplitude}`. Position at tick t: triangle wave on X over `Period` ticks, `Y` rises by `GongArcRisePercent` (25 percent) of the amplitude at the extremes (pendulum). A hit (distance <= gong radius + `PlayerHitRadius` 0.45 between centres) sets `Vx` to `GongKnockbackVx` (0.12 u/tick) away from the gong, sets `Vy` to +/- `GongKnockbackVy` (0.05 u/tick) away from it when airborne, starts `GongInvulnTicks` (90) of invulnerability and counts in `Hits`. Gongs never kill by themselves. Placement: in the jump corridor between two spine rows, offset 1 to 2.5 u from the corridor centre, amplitude `GongAmplitudeMin..Max` (1.5 to 3 u), radius `GongRadiusEasy..Hard` (0.6 to 0.9 u). Region 5: from level 3 (half density) with `GongDensityEasyPercent..Hard` (6 to 30 percent of rows) rising with d. Other regions get none.
`SurviveWave`: the middle rows (`WaveStartPercent` 35 to `WaveEndPercent` 65 of the height) get `WaveDensityPercent` (85) gongs per row (a second one 42 percent of the time) with shorter periods, on every region (also regions without the gong signature); the goal completes at the top row.

**Authored chunks (doc 08 section 4, D15).** `ChunkLibrary.All`: `chunk_complex_a..f` (class 3) and `chunk_gauntlet_a..f` (class 4), 6 to 8 rows each, relative X as permille of the screen width. Complex: widths 2.6 to 3.6, gaps 2.6 to 3.8, mixes of zigzag, stairs, pillars, bounce garden, drift, crumble. Gauntlet: widths 1.8 to 2.4, gaps 3.6 to 4.2, moving/vanishing/crumble/bounce mixes. The builder places chunks from `ChunkStartRow` (10) every `ChunkInterval` (14) rows after the previous chunk, cycling through `LevelSpec.FixedChunks`, plus one chunk ending the level. A chunk is mirrored on a seeded coin flip; its first row is moved into reach of the previous platform (same reach rule as generated rows) and later rows keep their authored offsets (clamping to the walls only shrinks offsets). Unknown chunk ids are skipped silently. Tests check every chunk against the generator reach rule.

**Goals (all in `GoalEvaluator`).** ReachFloor/BossChase: `Row >= Goal.Value`. CollectCoins: `Coins >= Count` (landing coins count). ComboChain: number of landings that gained at least `Value` floors in one jump (from the platform jumped from; `SimState.JumpHist`, 8 buckets, values above 8 are treated as 8) is at least `Count`; the builder inserts a **combo ladder** of `Value*Count+2` tight rungs (rung gap = `ComboLadderSpan` 3.6 u / Value, wide aligned platforms) at about 30 percent of the height, because normal gaps (>= 2 u) make a 3-floor jump impossible. NoWallBounce: `Row >= Value` with `WallBounces == 0` (a wall touch while airborne counts). FindHiddenGem: `HiddenGems > 0`; the gem floats `HiddenGemHeight` (1 u) above a 2.4 u side platform on the same row as a main platform, around the middle row, reachable by the same reach rule. Speedrun: `Row >= Value` and `Tick <= TimeSec * 60`; Speedrun levels are built with `SpeedrunGapPercent` (50) of the normal gap, because 90 floors at the normal 3.7 u gap need about 78 s of pure jumping with the baseline physics. SurviveWave: top row (see above). BossChase: `Row >= Value`; with `LevelSpec.Ramp` the scroll speed uses the endless step formula (+4 percent per 25 floors) clamped to `CampaignRampCapPercent` (60 percent; reached at row 375, so never within a 150-row level: at row 150 the ramp is +24 percent). The player speed ramp stays endless-only.

**Stars.** `StarEvaluator.Stars` = number of satisfied entries of `spec.Stars` when the goal is complete (0 otherwise): clear, coins >= Value, noRevive (`revivesUsed == 0`, the caller passes the count), time (`ticks <= Value*60`). `Earned` returns the bit mask in `spec.Stars` order.

**Revive.** `Sim.Revive(ref s)` puts a dead player back on the highest landed platform, `ReviveInvulnTicks` (120) of invulnerability, `KillY` set `ReviveKillDistance` (10 u) below, `RevivesUsed++`. No UI, no payment.

**Determinism.** Three independent `Rng` streams from `spec.Seed` (platforms, pickups = seed ^ 0x2F1A7C35, hazards = seed ^ 0x5A3C9E17); sorting uses total orders. `LevelData.ComputeHash()` is an integer hash for golden-level tests and server checks.

## Bot (T4)

`LevelBot(sim, spec, data, maxNodes).Solve()` plays the 1-star goal. Depth-first search over landing states with real-Sim rollouts. Jump options: homing jumps to every platform of the next 3 rows (and to the combo row), with waiting, a back-off run to build speed on narrow platforms and closed-loop air steering; for bounce pads it also picks the next target so the pad is entered with speed toward it; plus scripted run/air variants. Goal-aware cuts: no wall bounce, stay near the hidden gem row and the combo ladder, time limit. Speedrun uses best-first search on elapsed ticks. `MaxCoins(beamWidth)` / `MaxCoinsFrom(state, beamWidth)` is a row-layered beam search (homing jumps only) that maximises coins on a run that completes the goal and reaches the top row; it returns a lower bound of the true maximum and is used for the information-only coin-star report (the test takes the best of the solving line, a beam from the start and a beam continued from the end of the solving line). The legacy constructor `LevelBot(sim, targetRow, maxNodes)` keeps the original scripted search for platform-only levels (endless, old shipped-level test). `Tests/Simulation/SOLVABILITY_REPORT.md` is rewritten by the T4 test on every run.

Bot limits worth knowing: it never revives, never plays for style, and a level counts as solved when `GoalEvaluator.IsComplete` holds on a line that stays alive. Levels the bot cannot solve are generator bugs (fix the builder, not the budget); budget 20000 nodes, the slowest shipped level needs about 4800.

## Assumptions to confirm (open questions)

See the final report of T3/T4 and `docs/09`-style list in the session notes: coin-star targets versus landing coins, Speedrun targets versus the baseline physics, gem counting of the hidden gem, level end at goal completion, SurviveWave goal wording, signature share semantics.
