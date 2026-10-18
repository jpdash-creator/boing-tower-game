namespace BoingTower.Simulation
{
    /// <summary>
    /// Physics and rules constants (doc 09 D4: prototype baselines, to be tuned then frozen).
    /// Units: world units, 60 ticks per second. Velocities are units per tick, accelerations per tick squared.
    /// Player width is 1 unit, screen width 10 units (doc 09 D5). Production values will come from
    /// Remote Config; these defaults are the offline fallback.
    /// </summary>
    public sealed class SimConfig
    {
        public const int TicksPerSecond = 60;

        public Fx ScreenWidth = Fx.FromInt(10);
        public Fx PlayerHalfWidth = Fx.Half;
        /// <summary>Extra horizontal tolerance when landing on a platform edge.</summary>
        public Fx LandingMargin = Fx.FromRatio(1, 4);

        public Fx Gravity = Fx.FromRatio(40, 3600);          // 40 u/s^2
        public Fx Accel = Fx.FromRatio(30, 3600);            // 30 u/s^2
        public Fx AirControl = Fx.FromRatio(3, 5);           // air acceleration share
        public Fx Friction = Fx.FromRatio(45, 3600);         // ground deceleration with no input
        public Fx MaxSpeed = Fx.FromRatio(9, 60);            // 9 u/s
        public Fx JumpBase = Fx.FromRatio(14, 60);           // 14 u/s
        public Fx JumpSpeedFactor = Fx.FromRatio(85, 100);   // extra jump speed per unit of horizontal speed
        public Fx WallBoost = Fx.FromRatio(11, 10);          // speed multiplier on wall bounce
        public int CoyoteTicks = 5;                          // 80 ms
        public int JumpBufferTicks = 6;                      // 100 ms

        // Platform behaviours
        public Fx BounceMultiplier = Fx.FromRatio(14, 10);
        public Fx StickyJumpMultiplier = Fx.FromRatio(7, 10);
        public Fx SlipperyFriction = Fx.FromRatio(15, 100);
        public Fx SlipperyAccel = Fx.FromRatio(6, 10);
        public int CrumbleTicks = 30;
        public int VanishVisibleTicks = 150;
        public int VanishHiddenTicks = 60;

        // Scroll pressure (kill zone). Units per second at d = 0 and d = 1.
        public Fx ScrollSpeedEasy = Fx.FromRatio(1, 2);
        public Fx ScrollSpeedHard = Fx.FromRatio(2, 1);
        public int ScrollStartRow = 3;
        /// <summary>Kill zone never trails further than this below the highest point reached.</summary>
        public Fx KillDistance = Fx.FromInt(14);

        // Endless speed ramp (doc 08 section 2)
        public int RampFloors = 25;
        public int RampPercent = 4;
        public int RampCap = 150;
        /// <summary>Percent of the ramp applied to the player's max run speed (doc 08: half rate).</summary>
        public int PlayerSpeedSharePercent = 50;

        // Scoring (doc 09 D5)
        public int FloorPoints = 10;
        public int ComboPointsMultiplier = 5;
        public int ComboMinFloors = 2;

        /// <summary>Jump launch speed (per tick) for a given horizontal speed magnitude.</summary>
        public Fx JumpSpeed(Fx absVx)
        {
            return JumpBase + JumpSpeedFactor * absVx;
        }

        /// <summary>Analytic maximum jump height at full speed (used by the generator, 0.9 safety factor applied there).</summary>
        public Fx MaxJumpHeight()
        {
            Fx v0 = JumpSpeed(MaxSpeed);
            return v0 * v0 / (Gravity * 2);
        }

        /// <summary>
        /// Analytic maximum horizontal distance covered while a full-speed jump rises above and
        /// then comes back down to a platform <paramref name="gap"/> units higher.
        /// Returns zero when the gap is unreachable.
        /// </summary>
        public Fx MaxReachDx(Fx gap)
        {
            Fx v0 = JumpSpeed(MaxSpeed);
            Fx disc = v0 * v0 - Gravity * 2 * gap;
            if (disc.Raw < 0) return Fx.Zero;
            Fx t = (v0 + Fx.Sqrt(disc)) / Gravity;
            return MaxSpeed * t;
        }

        /// <summary>Endless ramp percent for a floor: +RampPercent per RampFloors, capped.</summary>
        public int RampPercentAt(int row)
        {
            int p = (row / RampFloors) * RampPercent;
            return p > RampCap ? RampCap : p;
        }
    }
}
