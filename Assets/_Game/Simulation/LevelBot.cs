using System.Collections.Generic;

namespace BoingTower.Simulation
{
    public struct BotResult
    {
        public bool Solved;
        public int Nodes;
        public int Ticks;
        public int BestRow;
        public int Score;
    }

    /// <summary>
    /// Headless solvability bot (doc 03 section 4 / doc 08 section 4). Depth-first search over
    /// landing states: from each landing it tries scripted actions (wait, run up, jump, steer in the
    /// air), simulates them with the real Sim, and recurses on every safe landing on a higher row.
    /// A level is solvable if the bot reaches the target row alive.
    /// </summary>
    public sealed class LevelBot
    {
        private static readonly int[] RunTicks = { 0, 5, 10, 16, 24, 36 };
        /// <summary>Air steering: hold the air direction this many ticks, then release (0 = hold the whole flight).</summary>
        private static readonly int[] AirHolds = { 0, 8, 16, 28, 45 };
        private static readonly int[] WaitTicks = { 0, 20, 45, 80 };
        private const int MaxAirTicks = 300;

        private readonly Sim sim;
        private readonly int targetRow;
        private readonly int maxNodes;
        private readonly HashSet<long> visited = new HashSet<long>();
        private int nodes;
        private int bestRow;
        private SimState best;

        public LevelBot(Sim sim, int targetRow, int maxNodes)
        {
            this.sim = sim;
            this.targetRow = targetRow;
            this.maxNodes = maxNodes;
        }

        public BotResult Solve()
        {
            SimState start = sim.NewState();
            best = start;
            bool solved = Search(start);
            BotResult r = new BotResult();
            r.Solved = solved;
            r.Nodes = nodes;
            r.Ticks = best.Tick;
            r.BestRow = bestRow;
            r.Score = best.Score;
            return r;
        }

        private bool Search(SimState s)
        {
            if (s.Row > bestRow) { bestRow = s.Row; best = s; }
            if (s.Row >= targetRow) { best = s; return true; }
            if (++nodes > maxNodes) return false;

            Platform here = sim.Platforms[s.GroundId];
            int preferred = PreferredDirection(s);
            int[] dirs = { preferred, 0, -preferred };
            int[] airDirs = { preferred, 0, -preferred };
            int[] waits = here.Kind == PlatformKind.Crumble ? new int[] { 0 } : WaitTicks;

            for (int wi = 0; wi < waits.Length; wi++)
            {
                for (int di = 0; di < dirs.Length; di++)
                {
                    for (int ri = 0; ri < RunTicks.Length; ri++)
                    {
                        if (dirs[di] == 0 && ri > 0) continue;
                        for (int ai = 0; ai < airDirs.Length; ai++)
                        {
                            for (int hi = 0; hi < AirHolds.Length; hi++)
                            {
                                if (airDirs[ai] == 0 && hi > 0) continue;
                                SimState c;
                                if (!TryJump(s, waits[wi], dirs[di], RunTicks[ri], airDirs[ai], AirHolds[hi], out c)) continue;
                                if (c.Row <= s.Row && c.GroundId == s.GroundId) continue;
                                if (c.Row < s.Row) continue;
                                long key = Key(c);
                                if (!visited.Add(key)) continue;
                                if (Search(c)) return true;
                                if (nodes > maxNodes) return false;
                            }
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>Steers toward the average X of the next row so the first tries are the likely ones.</summary>
        private int PreferredDirection(SimState s)
        {
            int next = s.Row + 1;
            long sum = 0;
            int count = 0;
            for (int i = 0; i < sim.Platforms.Length; i++)
            {
                if (sim.Platforms[i].Row == next) { sum += sim.PlatformX(i, s.Tick).Raw; count++; }
                else if (sim.Platforms[i].Row > next) break;
            }
            if (count == 0) return 1;
            Fx target = Fx.FromRaw((int)(sum / count));
            return target >= s.X ? 1 : -1;
        }

        private long Key(SimState c)
        {
            Platform p = sim.Platforms[c.GroundId];
            long vb = (c.Vx.Raw * 20L / Fx.OneRaw) + 16;
            long tb = 0;
            if (p.Kind == PlatformKind.Moving || p.Kind == PlatformKind.Vanishing) tb = (c.Tick / 20) % 24;
            long xb = c.X.Raw * 4L / Fx.OneRaw;
            return c.GroundId + (vb << 20) + (tb << 28) + (xb << 36);
        }

        private bool TryJump(SimState from, int wait, int runDir, int runTicks, int airDir, int airHold, out SimState result)
        {
            SimState c = from.Clone();
            result = c;
            int runInput = runDir < 0 ? SimInput.Left : (runDir > 0 ? SimInput.Right : 0);
            int airInput = airDir < 0 ? SimInput.Left : (airDir > 0 ? SimInput.Right : 0);

            for (int i = 0; i < wait; i++)
            {
                sim.Step(ref c, 0);
                if (c.Dead || !c.Grounded) return false;
            }
            for (int i = 0; i < runTicks; i++)
            {
                sim.Step(ref c, runInput);
                if (c.Dead || !c.Grounded) return false;
            }
            sim.Step(ref c, runInput | SimInput.Jump);
            if (c.Dead || c.Grounded) return false;

            int air = 0;
            while (!c.Grounded && !c.Dead && air < MaxAirTicks)
            {
                sim.Step(ref c, airHold > 0 && air >= airHold ? 0 : airInput);
                air++;
            }
            if (c.Dead || !c.Grounded) return false;
            result = c;
            return true;
        }
    }
}
