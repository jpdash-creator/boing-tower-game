# Economy module (T8): save, wallet, rewards, unlocks

Assembly `BoingTower.Economy` (references Simulation, Content, Newtonsoft). Plain C# except nothing engine-specific; tested in Unity EditMode (`Assets/_Game/Tests/EditMode/EconomyTests.cs`, 28 tests).

## API
```
var repo = ContentRepository.LoadFromResources();
var svc  = new ProgressService(repo, new FileSaveStore(Path.Combine(Application.persistentDataPath, "save.json")), playableRegionLimit: 5);

svc.CanEnterRegion(region), svc.IsLevelUnlocked(level), svc.GemSkipPrice(region), svc.TrySkipToRegion(region) -> SkipResult
svc.ApplyRun(levelId, new RunOutcome { Completed, StarMask, Score, CoinsCollected, GemsCollected }) -> RewardResult
svc.Coins / Gems, AddCoins/AddGems, TrySpendCoins/TrySpendGems, TotalStars(), StarsInRegion(region), Changed event
```
Build `RunOutcome` from the finished `SimState`: `StarMask = StarEvaluator.Earned(spec, s, revivesUsed, ticks)`, `CoinsCollected = s.Coins`, `GemsCollected = s.Gems`, `Completed = GoalEvaluator.IsComplete(spec, s)`.

## Save
`SaveData` (version 1): coins, gems, per-level `LevelProgress {Stars, StarMask, BestScore, Clears}`, unlocked region ids (region 1 implicit). `FileSaveStore` writes `path.tmp`, keeps the last good file as `path.bak`, swaps, and loads in the order main, tmp, backup, fresh. Corrupt files never throw; a save from a newer app version (version greater than 1) is ignored, not guessed at. `MemorySaveStore` is for tests.

## Rules implemented (doc 01 FR-P5/P6, doc 08 section 7)
- Region 1 is always unlocked; clearing level 15 unlocks the next region if it is playable.
- Gem skip: pay `unlock.gemSkip` (region 2 = 100 ... region 20 = 400) to unlock the NEXT locked region without clearing the previous one.
- Teaser regions can never be entered. `playableRegionLimit` (5 for this milestone) restricts live regions in the build; content still marks regions 6-20 live.
- Coins: end-of-level payout = base * region coinFactor, plus a star bonus, reduced to the level's `replayCoinPercent` (40) on replays. In-run pickups always pay in full.
- Gems: normal levels only from in-run gem pickups. Milestones: L5 and L10 roll the content chance (25 percent for 1-3 gems) on first clear, L10 guarantees 1; L15 first clear gives 5 plus a star bonus (+2 for two stars, +5 for three), replays 50 percent for 2. Rolls use the Simulation `Rng` seeded from the level seed and the clear count, so a server can recompute them.
- Level N unlocks when level N-1 of the same region is cleared. Best stars use the union of earned star bits; score keeps the maximum.

## Assumptions to confirm (also in `.claude/pendingdecisions/open-items.md`)
1. Star bonus is +10 percent of the payout per star beyond the first (doc 08 says "base + star bonus" without a number). Constant: `EconomyRules.StarBonusPercentPerExtraStar`.
2. A failed run pays nothing and records nothing.
3. Pickups pay in full on replays (only the end payout is reduced to 40 percent).
4. Gem skip only unlocks the next locked region, not any region.
5. Levels inside a region unlock in order.
6. Hidden gems pay no wallet gems yet (Simulation counts them separately).
7. Playable region limit is code, not content: flip regions 6-20 to `teaser` in `gen_content.py` (and the validator rule) when the launch scope is final.
8. Local only: the server-authoritative wallet (PlayFab) replaces this later; the save format is the conflict-resolution unit (server wins for currencies, max wins for progress).
