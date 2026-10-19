using BoingTower.Content;
using BoingTower.Economy;
using BoingTower.Simulation;

namespace BoingTower.Gameplay
{
    /// <summary>
    /// One attempt at one level: owns the deterministic Sim and its state, advances it one tick at a time and decides when
    /// the run is over (owner decision: a run ends at goal completion, or when the player falls). Plain C#, no Unity types,
    /// so it is unit-testable and the same code a server could replay.
    /// </summary>
    public sealed class RunSession
    {
        public const int ReviveGemCost = 20;   // doc 08 section 8

        public readonly SimConfig Cfg;
        public readonly LevelDef Level;
        public readonly RegionDef Region;
        public readonly LevelSpec Spec;
        public readonly LevelData Data;
        public readonly Sim Sim;
        public SimState State;

        public bool GoalReached { get; private set; }
        public bool Failed { get { return State.Dead && !GoalReached; } }
        public bool Over { get { return GoalReached || State.Dead; } }

        public RunSession(ContentRepository repo, SimConfig cfg, string levelId)
        {
            Cfg = cfg;
            Level = repo.GetLevel(levelId);
            Region = repo.GetRegion(Level.Region);
            Spec = LevelSpecFactory.Build(Level, Region);
            Data = LevelBuilder.Build(cfg, Spec);
            Sim = new Sim(cfg, Data, Spec);
            State = Sim.NewState();
        }

        /// <summary>Advances one 1/60 s tick. Does nothing once the run is over.</summary>
        public void Step(int input)
        {
            if (Over) return;
            Sim.Step(ref State, input);
            if (GoalEvaluator.IsComplete(Spec, State)) GoalReached = true;
        }

        /// <summary>Pays the revive price from the wallet and puts the player back on the highest landed platform.</summary>
        public bool TryRevive(ProgressService progress)
        {
            if (!State.Dead || GoalReached) return false;
            if (!progress.TrySpendGems(ReviveGemCost)) return false;
            Sim.Revive(ref State);
            return true;
        }

        public RunOutcome BuildOutcome()
        {
            return new RunOutcome
            {
                Completed = GoalReached,
                StarMask = GoalReached ? StarEvaluator.Earned(Spec, State, State.RevivesUsed, State.Tick) : 0,
                Score = State.Score,
                CoinsCollected = State.Coins,
                GemsCollected = State.Gems,
                HiddenGems = State.HiddenGems
            };
        }

        /// <summary>Short goal text for the HUD and the level info popup.</summary>
        public static string DescribeGoal(LevelSpec spec)
        {
            GoalSpec g = spec.Goal;
            switch (g.Type)
            {
                case GoalType.ReachFloor: return "Reach floor " + g.Value;
                case GoalType.CollectCoins: return "Collect " + g.Count + " coins";
                case GoalType.ComboChain: return "Chain " + g.Count + " jump" + (g.Count == 1 ? "" : "s") + " of " + g.Value + "+ floors";
                case GoalType.NoWallBounce: return "Reach floor " + g.Value + " without a wall bounce";
                case GoalType.FindHiddenGem: return "Find the hidden gem";
                case GoalType.Speedrun: return "Reach floor " + g.Value + " in " + g.TimeSec + " s";
                case GoalType.SurviveWave: return "Survive the wave and reach the top";
                case GoalType.BossChase: return "Outclimb the chase: floor " + g.Value;
                default: return "Reach the top";
            }
        }

        public static string DescribeStar(StarSpec star)
        {
            switch (star.Kind)
            {
                case StarSpec.KindClear: return "Clear the level";
                case StarSpec.KindCoins: return "Collect " + star.Value + " coins";
                case StarSpec.KindNoRevive: return "Do not revive";
                case StarSpec.KindTime: return "Finish in " + star.Value + " s";
                default: return "";
            }
        }
    }
}
