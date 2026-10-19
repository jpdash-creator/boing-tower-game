using System;
using BoingTower.Content;
using BoingTower.Simulation;

namespace BoingTower.Economy
{
    /// <summary>Tunables that doc 08 leaves open. Defaults are assumptions; they will move to Remote Config.</summary>
    public static class EconomyRules
    {
        /// <summary>Extra coin payout percent per star beyond the first (doc 08: "base + star bonus", amount unspecified).</summary>
        public const int StarBonusPercentPerExtraStar = 10;
    }

    /// <summary>What a finished run reports to the economy. Build it from SimState and StarEvaluator.</summary>
    public struct RunOutcome
    {
        public bool Completed;
        /// <summary>Bits earned: bit 0 = star 1 (clear), bit 1 = star 2, bit 2 = star 3 (StarEvaluator.Earned).</summary>
        public int StarMask;
        public int Score;
        /// <summary>Coins picked up and earned by landing rules during the run (SimState.Coins).</summary>
        public int CoinsCollected;
        /// <summary>Normal gem pickups collected in the run (SimState.Gems).</summary>
        public int GemsCollected;

        public int Stars
        {
            get
            {
                int n = 0;
                for (int m = StarMask & 7; m != 0; m >>= 1) n += m & 1;
                return n;
            }
        }
    }

    public struct RewardResult
    {
        public bool FirstClear;
        public int Stars;
        public int PayoutCoins;      // end-of-level payout after region factor, star bonus and replay percent
        public int StarBonusCoins;   // part of PayoutCoins that came from the star bonus
        public int PickupCoins;      // coins collected during the run
        public int RewardGems;       // milestone gem rewards (L5/L10/L15)
        public int PickupGems;       // gems picked up during the run
        public string UnlockedRegionId;

        public int TotalCoins { get { return PayoutCoins + PickupCoins; } }
        public int TotalGems { get { return RewardGems + PickupGems; } }
    }

    /// <summary>
    /// Doc 08 section 7 reward rules, deterministic (rolls use the Simulation Rng seeded from the level seed and clear count,
    /// so a server can recompute them).
    /// Assumptions (open for the owner): a failed run pays nothing; in-run pickups always pay in full (only the end-of-level payout
    /// is reduced on replays); normal levels get gems only from in-run pickups (the 3 percent chance in the content is the pickup);
    /// milestone gem rewards (L5, L10, L15) follow the doc 08 table; hidden gems pay nothing yet.
    /// </summary>
    public static class RewardCalculator
    {
        public static bool IsMilestone(LevelDef level)
        {
            return level.LevelClass == LevelDef.ClassComplex || level.LevelClass == LevelDef.ClassHighlyComplex;
        }

        static int Percent(double chance)
        {
            return (int)Math.Round(chance * 100.0);
        }

        public static RewardResult Compute(LevelDef level, RegionDef region, RunOutcome run, LevelProgress before)
        {
            var result = new RewardResult();
            if (!run.Completed) return result;

            int clearsBefore = before == null ? 0 : before.Clears;
            bool firstClear = clearsBefore == 0;
            int stars = run.Stars;
            result.FirstClear = firstClear;
            result.Stars = stars;

            // End-of-level coin payout: base * region factor, plus star bonus, reduced on replays.
            long payout = ContentRepository.CoinPayoutFor(level, region);
            long bonus = stars > 1 ? payout * EconomyRules.StarBonusPercentPerExtraStar * (stars - 1) / 100 : 0;
            long gross = payout + bonus;
            if (!firstClear)
            {
                gross = gross * level.Rewards.ReplayCoinPercent / 100;
                bonus = bonus * level.Rewards.ReplayCoinPercent / 100;
            }
            result.PayoutCoins = (int)gross;
            result.StarBonusCoins = (int)bonus;
            result.PickupCoins = run.CoinsCollected < 0 ? 0 : run.CoinsCollected;
            result.PickupGems = run.GemsCollected < 0 ? 0 : run.GemsCollected;

            GemRewardDef g = level.Rewards.Gems;
            if (g != null && IsMilestone(level))
            {
                var rng = new Rng(unchecked(level.Seed ^ (clearsBefore * 7919 + 13)));
                if (firstClear)
                {
                    int amount = 0;
                    if (rng.Next(100) < Percent(g.Chance)) amount = rng.Range(g.Min, g.Max);
                    if (g.FirstClearGuaranteed.HasValue && g.FirstClearGuaranteed.Value > amount) amount = g.FirstClearGuaranteed.Value;
                    if (g.StarBonusMax.HasValue && stars > 1) amount += g.StarBonusMax.Value * (stars - 1) / 2;
                    result.RewardGems = amount;
                }
                else if (g.ReplayChance.HasValue && g.ReplayAmount.HasValue)
                {
                    if (rng.Next(100) < Percent(g.ReplayChance.Value)) result.RewardGems = g.ReplayAmount.Value;
                }
            }
            return result;
        }
    }
}
