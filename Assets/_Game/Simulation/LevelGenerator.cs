using System.Collections.Generic;

namespace BoingTower.Simulation
{
    public sealed class GeneratorParams
    {
        public int Seed;
        /// <summary>Effective difficulty 0..1 (region offset already applied). Ignored per row when Endless.</summary>
        public Fx Difficulty;
        /// <summary>Number of platform rows above the start platform.</summary>
        public int Rows = 60;
        /// <summary>Endless mode: difficulty rises with the row, d = min(1, row / EndlessDifficultyRows).</summary>
        public bool Endless;
        public int EndlessDifficultyRows = 400;
        /// <summary>Special platform kinds this region may use (empty = none). The signature mechanic's platform goes here.</summary>
        public PlatformKind[] Specials = new PlatformKind[0];
        /// <summary>Special share at d = 1, percent (doc 08: up to 40).</summary>
        public int SpecialShareMaxPercent = 40;
    }

    /// <summary>
    /// Seeded, deterministic random platform generator (doc 08 section 4). Same params give the same
    /// platforms on every device. Reachability is guaranteed by construction (gap below 0.9 of the max
    /// jump height, horizontal offset bounded by the analytic reach) and checked by LevelBot.
    /// </summary>
    public static class LevelGenerator
    {
        private static readonly Fx Eight = Fx.FromInt(8);

        public static Fx DifficultyAt(GeneratorParams p, int row)
        {
            if (!p.Endless) return p.Difficulty;
            if (row >= p.EndlessDifficultyRows) return Fx.One;
            return Fx.FromRatio(row, p.EndlessDifficultyRows);
        }

        /// <summary>Combines a level's base d with the region offset: d + offset * (1 - d) (doc 08).</summary>
        public static Fx EffectiveDifficulty(Fx baseD, Fx regionOffset)
        {
            return baseD + regionOffset * (Fx.One - baseD);
        }

        public static Platform[] Generate(SimConfig cfg, GeneratorParams p)
        {
            Rng rng = new Rng(p.Seed);
            List<Platform> list = new List<Platform>(p.Rows * 2 + 1);
            Fx screen = cfg.ScreenWidth;
            Fx maxGap = cfg.MaxJumpHeight() * 9 / 10;

            Platform start = new Platform();
            start.Id = 0;
            start.Row = 0;
            start.Kind = PlatformKind.Normal;
            start.X = screen / 2;
            start.Y = Fx.Zero;
            start.Width = screen;
            list.Add(start);

            Fx prevX = start.X;
            Fx prevW = start.Width;
            Fx y = Fx.Zero;

            Fx widthEasyLo = Fx.FromInt(6), widthEasyHi = Fx.FromInt(8);
            Fx widthHardLo = Fx.FromRatio(16, 10), widthHardHi = Fx.FromRatio(24, 10);
            Fx gapEasyLo = Fx.FromInt(2), gapEasyHi = Fx.FromInt(3);
            Fx gapHardLo = Fx.FromRatio(38, 10), gapHardHi = Fx.FromRatio(52, 10);
            Fx offEasyLo = Fx.Zero, offEasyHi = Fx.FromRatio(2, 5);
            Fx offHardLo = Fx.FromRatio(1, 5), offHardHi = Fx.FromRatio(9, 10);
            Fx jitterLo = Fx.FromRatio(85, 100), jitterHi = Fx.FromRatio(115, 100);
            Fx minWidth = Fx.FromRatio(14, 10);
            Fx maxWidth = Fx.FromRatio(95, 10);

            for (int row = 1; row <= p.Rows; row++)
            {
                Fx d = DifficultyAt(p, row);

                Fx gap = rng.Range(Fx.Lerp(gapEasyLo, gapHardLo, d), Fx.Lerp(gapEasyHi, gapHardHi, d));
                gap = Fx.Min(gap, maxGap);
                y = y + gap;

                Fx w = rng.Range(Fx.Lerp(widthEasyLo, widthHardLo, d), Fx.Lerp(widthEasyHi, widthHardHi, d));
                w = w * rng.Range(jitterLo, jitterHi);
                w = Fx.Clamp(w, minWidth, maxWidth);

                Fx offFrac = rng.Range(Fx.Lerp(offEasyLo, offHardLo, d), Fx.Lerp(offEasyHi, offHardHi, d));
                Fx dx = offFrac * screen;
                // Edge-to-edge travel must stay within 60 percent of the analytic full-speed reach.
                Fx dxLimit = cfg.MaxReachDx(gap) * 6 / 10 + (prevW + w) / 2 + cfg.PlayerHalfWidth;
                dx = Fx.Min(dx, dxLimit);

                int dir = rng.Next(2) == 0 ? -1 : 1;
                Fx lo = w / 2, hi = screen - w / 2;
                Fx x = prevX + dx * dir;
                if (x < lo || x > hi) x = prevX - dx * dir;
                x = Fx.Clamp(x, lo, hi);

                PlatformKind kind = PlatformKind.Normal;
                if (p.Specials.Length > 0)
                {
                    int share = (Fx.FromInt(p.SpecialShareMaxPercent) * d).FloorToInt();
                    if (rng.Next(100) < share) kind = p.Specials[rng.Next(p.Specials.Length)];
                }

                Platform plat = new Platform();
                plat.Id = list.Count;
                plat.Row = row;
                plat.Kind = kind;
                plat.X = x;
                plat.Y = y;
                plat.Width = w;
                if (kind == PlatformKind.Moving)
                {
                    Fx room = Fx.Min(x - lo, hi - x);
                    plat.Amplitude = Fx.Min(rng.Range(Fx.Half, Fx.One), room);
                    plat.Period = 2 * rng.Range(90, 150);
                    plat.Phase = rng.Next(plat.Period);
                }
                else if (kind == PlatformKind.Vanishing)
                {
                    plat.Phase = rng.Next(cfg.VanishVisibleTicks + cfg.VanishHiddenTicks);
                }
                list.Add(plat);

                // Optional second platform on the same row (easy levels give more choices).
                int secondChance = (Fx.FromInt(50) * (Fx.One - d)).FloorToInt();
                if (rng.Next(100) < secondChance)
                {
                    Fx x2 = screen - x;
                    if (Fx.Abs(x2 - x) >= (w + w) / 2 + Fx.One)
                    {
                        Platform extra = new Platform();
                        extra.Id = list.Count;
                        extra.Row = row;
                        extra.Kind = PlatformKind.Normal;
                        extra.X = Fx.Clamp(x2, lo, hi);
                        extra.Y = y;
                        extra.Width = w;
                        list.Add(extra);
                    }
                }

                prevX = x;
                prevW = w;
            }
            return list.ToArray();
        }
    }
}
