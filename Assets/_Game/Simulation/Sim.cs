namespace BoingTower.Simulation
{
    /// <summary>Input bit flags for one tick. Jump is an edge: set it only on the tick of the tap.</summary>
    public static class SimInput
    {
        public const int Left = 1;
        public const int Right = 2;
        public const int Jump = 4;
    }

    /// <summary>All mutable simulation state. Copy with Clone() (the crumble array is the only reference member).</summary>
    public struct SimState
    {
        public int Tick;
        public Fx X, Y, Vx, Vy;
        public bool Grounded;
        /// <summary>Platform last stood on (kept while in coyote time and while airborne).</summary>
        public int GroundId;
        public int Coyote;
        public int JumpBuffer;
        public int JumpStartRow;
        /// <summary>Highest platform row landed on.</summary>
        public int Row;
        public int Combo, BestCombo, Score, WallBounces;
        public Fx KillY, MaxY;
        public bool Dead;
        /// <summary>Per platform: tick of first landing on a crumble platform, or -1.</summary>
        public int[] CrumbleTick;

        public SimState Clone()
        {
            SimState c = this;
            c.CrumbleTick = (int[])CrumbleTick.Clone();
            return c;
        }
    }

    /// <summary>
    /// Deterministic fixed-step simulation of one run. Stateless apart from the level: all mutable
    /// state lives in SimState so replays, bots and the server can branch and re-simulate cheaply.
    /// Platforms must be sorted by Y ascending (the generator guarantees this).
    /// </summary>
    public sealed class Sim
    {
        public readonly SimConfig Cfg;
        public readonly Platform[] Platforms;
        public readonly bool Endless;
        private readonly Fx scrollPerTick;

        /// <param name="difficulty">0..1 level difficulty; scales the scroll pressure.</param>
        public Sim(SimConfig cfg, Platform[] platforms, Fx difficulty, bool endless)
        {
            Cfg = cfg;
            Platforms = platforms;
            Endless = endless;
            scrollPerTick = Fx.Lerp(cfg.ScrollSpeedEasy, cfg.ScrollSpeedHard, difficulty) / SimConfig.TicksPerSecond;
        }

        public SimState NewState()
        {
            Platform start = Platforms[0];
            SimState s = new SimState();
            s.X = start.X;
            s.Y = start.Y;
            s.Grounded = true;
            s.GroundId = 0;
            s.Coyote = Cfg.CoyoteTicks;
            s.MaxY = start.Y;
            s.KillY = start.Y - Cfg.KillDistance;
            s.CrumbleTick = new int[Platforms.Length];
            for (int i = 0; i < s.CrumbleTick.Length; i++) s.CrumbleTick[i] = -1;
            return s;
        }

        public Fx PlatformX(int id, int tick)
        {
            Platform p = Platforms[id];
            if (p.Kind != PlatformKind.Moving) return p.X;
            int half = p.Period / 2;
            int phase = (tick + p.Phase) % p.Period;
            int tri = phase < half ? phase : p.Period - phase;
            Fx t = Fx.FromRatio(tri, half);
            return p.X + (t * 2 - Fx.One) * p.Amplitude;
        }

        public bool IsSolid(int id, int tick, int[] crumbleTick)
        {
            Platform p = Platforms[id];
            if (p.Kind == PlatformKind.Vanishing)
            {
                return (tick + p.Phase) % (Cfg.VanishVisibleTicks + Cfg.VanishHiddenTicks) < Cfg.VanishVisibleTicks;
            }
            if (p.Kind == PlatformKind.Crumble)
            {
                int landed = crumbleTick[id];
                return landed < 0 || tick - landed < Cfg.CrumbleTicks;
            }
            return true;
        }

        /// <summary>Index of the first platform whose Y is at or above y.</summary>
        private int LowerBound(Fx y)
        {
            int lo = 0, hi = Platforms.Length;
            while (lo < hi)
            {
                int mid = (lo + hi) >> 1;
                if (Platforms[mid].Y < y) lo = mid + 1; else hi = mid;
            }
            return lo;
        }

        private Fx CurrentMaxSpeed(int row)
        {
            Fx max = Cfg.MaxSpeed;
            if (Endless)
            {
                int pct = Cfg.RampPercentAt(row) * Cfg.PlayerSpeedSharePercent / 100;
                max = max * (100 + pct) / 100;
            }
            return max;
        }

        public void Step(ref SimState s, int input)
        {
            if (s.Dead) return;
            int tick = s.Tick + 1;
            int dir = ((input & SimInput.Right) != 0 ? 1 : 0) - ((input & SimInput.Left) != 0 ? 1 : 0);
            Fx maxSpeed = CurrentMaxSpeed(s.Row);

            // --- horizontal acceleration ---
            PlatformKind surface = Platforms[s.GroundId].Kind;
            Fx accel = Cfg.Accel;
            Fx friction = Cfg.Friction;
            if (!s.Grounded)
            {
                accel = accel * Cfg.AirControl;
            }
            else if (surface == PlatformKind.Slippery)
            {
                accel = accel * Cfg.SlipperyAccel;
                friction = friction * Cfg.SlipperyFriction;
            }
            if (dir != 0)
            {
                Fx a = accel;
                if (s.Vx.Sign() != 0 && s.Vx.Sign() != dir) a = a * 2;
                s.Vx = s.Vx + a * dir;
            }
            else if (s.Grounded)
            {
                if (s.Vx > friction) s.Vx = s.Vx - friction;
                else if (s.Vx < -friction) s.Vx = s.Vx + friction;
                else s.Vx = Fx.Zero;
            }
            s.Vx = Fx.Clamp(s.Vx, -maxSpeed, maxSpeed);

            // --- jump ---
            if ((input & SimInput.Jump) != 0) s.JumpBuffer = Cfg.JumpBufferTicks;
            if (s.JumpBuffer > 0 && (s.Grounded || s.Coyote > 0))
            {
                Fx v0 = Cfg.JumpSpeed(Fx.Abs(s.Vx));
                if (surface == PlatformKind.Sticky) v0 = v0 * Cfg.StickyJumpMultiplier;
                s.Vy = v0;
                s.Grounded = false;
                s.Coyote = 0;
                s.JumpBuffer = 0;
                s.JumpStartRow = Platforms[s.GroundId].Row;
            }

            // --- movement ---
            Fx prevY = s.Y;
            if (s.Grounded)
            {
                if (Platforms[s.GroundId].Kind == PlatformKind.Moving)
                {
                    s.X = s.X + PlatformX(s.GroundId, tick) - PlatformX(s.GroundId, s.Tick);
                }
                s.X = s.X + s.Vx;
            }
            else
            {
                s.Vy = s.Vy - Cfg.Gravity;
                s.Y = s.Y + s.Vy;
                s.X = s.X + s.Vx;
            }

            // --- walls ---
            Fx minX = Cfg.PlayerHalfWidth;
            Fx maxX = Cfg.ScreenWidth - Cfg.PlayerHalfWidth;
            if (s.X < minX)
            {
                s.X = minX;
                if (s.Vx < Fx.Zero) { s.Vx = Fx.Clamp(-s.Vx * Cfg.WallBoost, -maxSpeed, maxSpeed); if (!s.Grounded) s.WallBounces++; }
            }
            else if (s.X > maxX)
            {
                s.X = maxX;
                if (s.Vx > Fx.Zero) { s.Vx = Fx.Clamp(-s.Vx * Cfg.WallBoost, -maxSpeed, maxSpeed); if (!s.Grounded) s.WallBounces++; }
            }

            // --- ground / landing ---
            if (s.Grounded)
            {
                Platform g = Platforms[s.GroundId];
                bool solid = IsSolid(s.GroundId, tick, s.CrumbleTick);
                if (!solid || Fx.Abs(s.X - PlatformX(s.GroundId, tick)) > g.Width / 2 + Cfg.LandingMargin)
                {
                    s.Grounded = false;
                    s.Vy = Fx.Zero;
                    s.Coyote = Cfg.CoyoteTicks;
                    s.JumpStartRow = g.Row;
                }
            }
            else
            {
                if (s.Coyote > 0) s.Coyote--;
                if (s.Vy < Fx.Zero)
                {
                    int best = -1;
                    for (int i = LowerBound(s.Y); i < Platforms.Length && Platforms[i].Y <= prevY; i++)
                    {
                        Platform p = Platforms[i];
                        if (!IsSolid(i, tick, s.CrumbleTick)) continue;
                        if (Fx.Abs(s.X - PlatformX(i, tick)) > p.Width / 2 + Cfg.LandingMargin) continue;
                        if (best < 0 || p.Y > Platforms[best].Y) best = i;
                    }
                    if (best >= 0) Land(ref s, best, tick);
                }
            }

            // --- scroll pressure and death ---
            if (s.Y > s.MaxY) s.MaxY = s.Y;
            if (s.Row >= Cfg.ScrollStartRow)
            {
                Fx scroll = scrollPerTick;
                if (Endless) scroll = scroll * (100 + Cfg.RampPercentAt(s.Row)) / 100;
                s.KillY = s.KillY + scroll;
            }
            Fx follow = s.MaxY - Cfg.KillDistance;
            if (follow > s.KillY) s.KillY = follow;
            if (s.Y < s.KillY) s.Dead = true;

            if (s.JumpBuffer > 0) s.JumpBuffer--;
            s.Tick = tick;
        }

        private void Land(ref SimState s, int id, int tick)
        {
            Platform p = Platforms[id];
            s.Y = p.Y;
            s.Vy = Fx.Zero;
            s.Grounded = true;
            s.GroundId = id;
            s.Coyote = Cfg.CoyoteTicks;

            if (p.Row > s.Row)
            {
                s.Score += (p.Row - s.Row) * Cfg.FloorPoints;
                s.Row = p.Row;
            }
            if (p.Row - s.JumpStartRow >= Cfg.ComboMinFloors)
            {
                s.Combo++;
                if (s.Combo > s.BestCombo) s.BestCombo = s.Combo;
                s.Score += s.Combo * s.Combo * Cfg.ComboPointsMultiplier;
            }
            else
            {
                s.Combo = 0;
            }

            if (p.Kind == PlatformKind.Crumble && s.CrumbleTick[id] < 0) s.CrumbleTick[id] = tick;
            if (p.Kind == PlatformKind.Bounce)
            {
                s.Vy = Cfg.JumpSpeed(Fx.Abs(s.Vx)) * Cfg.BounceMultiplier;
                s.Grounded = false;
                s.Coyote = 0;
                s.JumpStartRow = p.Row;
            }
        }
    }
}
