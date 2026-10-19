using System.Collections.Generic;
using BoingTower.Simulation;
using UnityEngine;

namespace BoingTower.Gameplay
{
    /// <summary>
    /// Draws a RunSession with SpriteRenderers (no gameplay logic here: it only reads the Sim state).
    /// World units equal Simulation units; the camera shows exactly SimConfig.ScreenWidth units across.
    /// </summary>
    public sealed class RunView : MonoBehaviour
    {
        sealed class Layer
        {
            public SpriteRenderer[] Tiles;
            public float Factor;
            public float Height;
            public float Scale;
        }

        sealed class Pop
        {
            public Transform Tf;
            public SpriteRenderer Sr;
            public float Age = 99f;
            public float Life = 0.35f;
            public float Size = 1f;
            public float Rise = 1f;
        }

        RunSession session;
        ArtCatalog art;
        RegionArt region;
        Camera cam;

        Transform worldRoot;
        Transform player;
        SpriteRenderer playerSr;
        float playerHeight = 1.5f;
        SpriteRenderer[] platformSr;
        Transform[] platformTf;
        float[] platformBaseScaleX;
        float[] platformOffX, platformOffY;   // sprite offset that puts the island's walking surface on the platform line
        SpriteRenderer[] pickupSr;
        Transform[] hazardTf;
        SpriteRenderer[] hazardSr;
        readonly List<Layer> layers = new List<Layer>();
        SpriteRenderer[] wallL, wallR;
        float wallSegHeight = 3.2f;
        SpriteRenderer killZone;
        SpriteRenderer killLine;
        readonly Pop[] pops = new Pop[24];
        int nextPop;
        Sprite white;

        float camHalfH, camHalfW, camY, designHalfH;
        float prevX, prevY, curX, curY;
        bool wasGrounded = true;
        float squash;   // 0..1 landing squash timer
        bool reduced;   // reduced-motion setting, read once per run
        Sprite[] patterns;
        SpriteRenderer[] cueSr;
        SpriteRenderer dangerBand;

        /// <summary>Tallest supported aspect (9:21 and a margin). Backgrounds and walls are sized for it so any phone is covered.</summary>
        public const float TallestAspect = 0.4f;

        /// <summary>Test hook: forces the view aspect (width / height). 0 = use the camera.</summary>
        public static float AspectOverride;

        public float CameraY { get { return camY; } }
        public Camera Camera { get { return cam; } }

        /// <summary>Debug/test hook: hides the parallax layers so the gameplay objects can be inspected on their own.</summary>
        public void SetBackgroundVisible(bool visible)
        {
            for (int i = 0; i < layers.Count; i++)
                for (int k = 0; k < layers[i].Tiles.Length; k++) layers[i].Tiles[k].enabled = visible;
        }

        /// <summary>Debug/test hook: shows only the first n parallax layers (0 = none).</summary>
        public void SetBackgroundLayerCount(int n)
        {
            for (int i = 0; i < layers.Count; i++)
                for (int k = 0; k < layers[i].Tiles.Length; k++) layers[i].Tiles[k].enabled = i < n;
        }

        public string DebugPlayer()
        {
            return playerSr == null ? "no player" : "player enabled=" + playerSr.enabled + " order=" + playerSr.sortingOrder + " sprite=" + (playerSr.sprite != null ? playerSr.sprite.name : "null") + " pos=" + playerSr.transform.position + " scale=" + playerSr.transform.localScale + " color=" + playerSr.color + " bounds=" + playerSr.bounds.size;
        }

        // ---------------- setup ----------------

        public void Build(RunSession s, ArtCatalog catalog, Camera camera)
        {
            session = s;
            reduced = GameSettings.ReducedMotion;
            art = catalog;
            cam = camera;
            region = art != null ? art.ForRegion(s.Region.Index) : null;
            white = MakeWhite();

            worldRoot = new GameObject("RunWorld").transform;
            worldRoot.SetParent(transform, false);

            float screenW = s.Cfg.ScreenWidth.F();
            cam.orthographic = true;
            camHalfW = screenW * 0.5f;
            FitCamera();
            designHalfH = screenW / (2f * TallestAspect);
            cam.transform.position = new Vector3(screenW * 0.5f, 0f, -10f);
            cam.backgroundColor = new Color32(0x14, 0x16, 0x24, 0xff);
            cam.clearFlags = CameraClearFlags.SolidColor;

            BuildBackground(screenW);
            BuildWalls();
            BuildPlatforms();
            BuildPickups();
            BuildHazards();
            BuildPlayer();
            BuildKillZone();
            for (int i = 0; i < pops.Length; i++) pops[i] = MakePop();

            curX = prevX = s.State.X.F();
            curY = prevY = s.State.Y.F();
            camY = curY + camHalfH * 0.55f;
            ApplyCamera();
        }

        static Sprite MakeWhite()
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            tex.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            tex.Apply();
            tex.filterMode = FilterMode.Bilinear;
            return Sprite.Create(tex, new Rect(0, 0, 2, 2), new Vector2(0.5f, 0.5f), 2f);   // 1x1 world unit
        }

        SpriteRenderer NewSprite(string name, Sprite sprite, int order, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : worldRoot, false);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = sprite != null ? sprite : white;
            sr.sortingOrder = order;
            return sr;
        }

        void BuildBackground(float screenW)
        {
            Sprite[] sprites = region != null ? new[] { region.BgFar, region.BgMid, region.BgNear } : new Sprite[3];
            float[] factor = { 0.12f, 0.25f, 0.45f };
            float[] alpha = { 1f, 0.55f, 0.35f };   // owner decision: mid/near stack with fade (they came out opaque)
            for (int i = 0; i < 3; i++)
            {
                if (sprites[i] == null) continue;
                Vector2 size = sprites[i].bounds.size;
                float scale = Mathf.Max(screenW * 1.1f / size.x, designHalfH * 2f * 1.05f / size.y);
                var layer = new Layer { Factor = factor[i], Height = size.y * scale, Scale = scale, Tiles = new SpriteRenderer[3] };
                for (int k = 0; k < 3; k++)
                {
                    var sr = NewSprite("bg" + i + "_" + k, sprites[i], -100 + i * 10);
                    sr.transform.localScale = new Vector3(scale, scale, 1f);
                    sr.color = new Color(1f, 1f, 1f, alpha[i]);
                    layer.Tiles[k] = sr;
                }
                layers.Add(layer);
            }
        }

        void BuildWalls()
        {
            if (region == null || (region.WallL == null && region.WallR == null)) return;
            int count = Mathf.CeilToInt(designHalfH * 2f / wallSegHeight) + 2;
            wallL = new SpriteRenderer[count];
            wallR = new SpriteRenderer[count];
            for (int i = 0; i < count; i++)
            {
                wallL[i] = MakeWall(region.WallL, true);
                wallR[i] = MakeWall(region.WallR, false);
            }
        }

        SpriteRenderer MakeWall(Sprite sprite, bool left)
        {
            if (sprite == null) return null;
            var sr = NewSprite(left ? "wallL" : "wallR", sprite, -20);
            Vector2 size = sprite.bounds.size;
            float h = wallSegHeight;
            float w = Mathf.Min(1.1f, size.x / size.y * h);
            sr.transform.localScale = new Vector3(w / size.x, h / size.y, 1f);
            return sr;
        }

        static readonly System.Collections.Generic.Dictionary<int, Sprite> slabs = new System.Collections.Generic.Dictionary<int, Sprite>();

        /// <summary>A clean rounded slab: dark outline, light top band, darker body. 9-sliced, so it is never stretched out of shape.</summary>
        static Sprite MakeSlab(Color top, Color side)
        {
            int key = ((Color32)top).GetHashCode() * 31 + ((Color32)side).GetHashCode();
            Sprite cached;
            if (slabs.TryGetValue(key, out cached) && cached != null) return cached;
            const int w = 64, h = 32, r = 10;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            tex.wrapMode = TextureWrapMode.Clamp;
            var px = new Color32[w * h];
            Color outline = new Color(0.08f, 0.07f, 0.14f, 1f);
            Color hi = Color.Lerp(top, Color.white, 0.45f);
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    // distance to the rounded rectangle edge (0 at the border, growing inwards)
                    float dx = Mathf.Min(x + 0.5f, w - x - 0.5f), dy = Mathf.Min(y + 0.5f, h - y - 0.5f);
                    float inset;
                    if (dx < r && dy < r) inset = r - Mathf.Sqrt((r - dx) * (r - dx) + (r - dy) * (r - dy));
                    else inset = Mathf.Min(dx, dy);
                    if (inset < 0f) { px[y * w + x] = new Color32(0, 0, 0, 0); continue; }
                    Color c;
                    float fromTop = h - 1 - y;     // y grows upwards in texture space
                    if (inset < 2.2f) c = outline;
                    else if (fromTop < 3.5f) c = outline;
                    else if (fromTop < 6f) c = hi;
                    else if (fromTop < 13f) c = top;
                    else c = Color.Lerp(side, side * 0.78f, Mathf.Clamp01((fromTop - 13f) / 16f));
                    c.a = 1f;
                    px[y * w + x] = c;
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            Sprite sp = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 64f, 0, SpriteMeshType.FullRect, new Vector4(14, 10, 14, 10));
            slabs[key] = sp;
            return sp;
        }

        PlatformMetric PlatformMetricFor(Platform p)
        {
            if (region == null || region.Metrics == null) return default(PlatformMetric);
            int k;
            switch (p.Kind)
            {
                case PlatformKind.Moving: k = region.Moving != null ? 2 : 0; break;
                case PlatformKind.Crumble: k = region.Crumble != null ? 3 : 0; break;
                case PlatformKind.Bounce:
                case PlatformKind.BounceCap: k = region.Bounce != null ? 4 : 0; break;
                default: k = (p.Width.F() < 3.2f && region.Narrow != null) ? 1 : 0; break;
            }
            return k < region.Metrics.Length ? region.Metrics[k] : default(PlatformMetric);
        }

        Sprite PlatformSprite(Platform p)
        {
            if (region == null) return null;
            switch (p.Kind)
            {
                case PlatformKind.Moving: return region.Moving ?? region.Normal;
                case PlatformKind.Crumble: return region.Crumble ?? region.Normal;
                case PlatformKind.Bounce:
                case PlatformKind.BounceCap: return region.Bounce ?? region.Normal;
                default: return (p.Width.F() < 3.2f && region.Narrow != null) ? region.Narrow : region.Normal;
            }
        }

        /// <summary>Procedural 32 x 8 tile used as a non-colour cue: dots (sticky), stripes (slippery), dashes (vanishing), hazard stripes (danger).</summary>
        static Sprite MakePattern(int kind)
        {
            const int w = 32, h = 8;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            tex.wrapMode = TextureWrapMode.Repeat;
            tex.filterMode = FilterMode.Point;
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    bool on;
                    switch (kind)
                    {
                        case 0: on = ((x % 16) - 8) * ((x % 16) - 8) + (y - 4) * (y - 4) <= 6; break;           // dots
                        case 1: on = ((x + y) % 8) < 3; break;                                                    // diagonal stripes
                        case 2: on = (x % 16) < 9 && y >= 2 && y <= 5; break;                                     // dashes
                        case 4: on = Mathf.Abs((x % 16) - 8) == 7 - y || Mathf.Abs((x % 16) - 8) == 6 - y; break;   // up chevrons (bounce)
                        default: on = ((x + y) % 16) < 8; break;                                                  // hazard stripes
                    }
                    px[y * w + x] = on ? new Color32(20, 16, 40, 230) : new Color32(0, 0, 0, 0);
                }
            }
            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 32f, 0, SpriteMeshType.FullRect);
        }

        static Color PlatformTint(PlatformKind kind)
        {
            switch (kind)
            {
                case PlatformKind.Sticky: return new Color(1f, 0.7f, 0.9f, 1f);
                case PlatformKind.Slippery: return new Color(0.7f, 0.9f, 1f, 1f);
                case PlatformKind.Vanishing: return new Color(0.8f, 0.7f, 1f, 1f);
                default: return Color.white;
            }
        }

        void BuildPlatforms()
        {
            Platform[] plats = session.Data.Platforms;
            platformSr = new SpriteRenderer[plats.Length];
            platformTf = new Transform[plats.Length];
            platformBaseScaleX = new float[plats.Length];
            platformOffX = new float[plats.Length];
            platformOffY = new float[plats.Length];
            cueSr = new SpriteRenderer[plats.Length];
            for (int i = 0; i < plats.Length; i++)
            {
                Platform p = plats[i];
                Sprite sprite = PlatformSprite(p);
                var sr = NewSprite("plat" + p.Id + "_" + p.Kind, sprite, 0);
                float width = p.Width.F();
                platformOffX[i] = 0f;
                if (region != null && region.PlatSide.a > 0f)
                {
                    // Clean code-drawn slab in the region's colours, 0.5 units thick; the top edge is exactly the walking surface.
                    Color top = region.PlatTop, side = region.PlatSide;
                    if (p.Kind == PlatformKind.Bounce || p.Kind == PlatformKind.BounceCap) { top = new Color(1f, 0.55f, 0.75f); side = new Color(0.8f, 0.2f, 0.45f); }
                    else if (p.Kind == PlatformKind.Crumble) { side = Color.Lerp(side, Color.gray, 0.45f); top = Color.Lerp(top, Color.gray, 0.35f); }
                    sr.sprite = MakeSlab(top, side);
                    sr.drawMode = SpriteDrawMode.Sliced;
                    sr.size = new Vector2(width + 0.3f, 0.6f);
                    sr.transform.localScale = Vector3.one;
                    platformBaseScaleX[i] = 1f;
                    platformOffY[i] = -0.3f;
                }
                else
                {
                    sr.transform.localScale = new Vector3(width, 0.4f, 1f);   // fallback bar
                    platformBaseScaleX[i] = width;
                    platformOffY[i] = -0.2f;
                }
                sr.color = PlatformTint(p.Kind);
                platformSr[i] = sr;
                platformTf[i] = sr.transform;

                // Non-colour cue (CUX-6): a pattern strip on the walking surface.
                int cue = p.Kind == PlatformKind.Sticky ? 0 : p.Kind == PlatformKind.Slippery ? 1 : p.Kind == PlatformKind.Vanishing ? 2 : (p.Kind == PlatformKind.Bounce || p.Kind == PlatformKind.BounceCap) ? 4 : -1;
                if (cue >= 0)
                {
                    if (patterns == null) patterns = new Sprite[5];
                    if (patterns[cue] == null) patterns[cue] = MakePattern(cue);
                    var c = NewSprite("cue" + p.Id, patterns[cue], 1);
                    c.drawMode = SpriteDrawMode.Tiled;
                    c.size = new Vector2(width * 0.8f, 0.25f);
                    cueSr[i] = c;
                }
            }
        }

        /// <summary>Drawn size in world units (the Simulation radius is only the collection zone).</summary>
        static float VisualDiameter(PickupKind kind)
        {
            switch (kind)
            {
                case PickupKind.Gem: return 0.8f;
                case PickupKind.HiddenGem: return 1.1f;
                default: return 0.7f;
            }
        }

        void BuildPickups()
        {
            Pickup[] picks = session.Data.Pickups;
            pickupSr = new SpriteRenderer[picks.Length];
            for (int i = 0; i < picks.Length; i++)
            {
                Pickup pk = picks[i];
                Sprite sprite = null;
                if (art != null)
                {
                    if (pk.Kind == PickupKind.Coin) sprite = ArtCatalog.Pick(art.CoinFrames, 0);
                    else sprite = ArtCatalog.Pick(art.GemSizes, pk.Kind == PickupKind.HiddenGem ? 2 : 1);
                }
                var sr = NewSprite("pick" + pk.Id, sprite, 5);
                float diameter = VisualDiameter(pk.Kind);
                if (sprite != null)
                {
                    Vector2 size = sprite.bounds.size;
                    float s = diameter / Mathf.Max(size.x, size.y);
                    sr.transform.localScale = new Vector3(s, s, 1f);
                }
                else sr.transform.localScale = new Vector3(diameter, diameter, 1f);
                if (pk.Kind == PickupKind.HiddenGem) sr.color = new Color(1f, 0.6f, 1f, 1f);
                sr.transform.position = new Vector3(pk.X.F(), pk.Y.F(), 0f);
                pickupSr[i] = sr;
            }
        }

        void BuildHazards()
        {
            Hazard[] hz = session.Data.Hazards;
            hazardTf = new Transform[hz.Length];
            hazardSr = new SpriteRenderer[hz.Length];
            Sprite gong = region != null && region.Signature.Length > 0 ? region.Signature[0] : null;
            for (int i = 0; i < hz.Length; i++)
            {
                var sr = NewSprite("gong" + hz[i].Id, gong, 6);
                float diameter = hz[i].Radius.F() * 2.4f;
                if (gong != null)
                {
                    Vector2 size = gong.bounds.size;
                    float s = diameter / Mathf.Max(size.x, size.y);
                    sr.transform.localScale = new Vector3(s, s, 1f);
                }
                else
                {
                    sr.transform.localScale = new Vector3(diameter, diameter, 1f);
                    sr.color = new Color(1f, 0.75f, 0.2f, 1f);
                }
                hazardTf[i] = sr.transform;
                hazardSr[i] = sr;
            }
        }

        void BuildPlayer()
        {
            playerSr = NewSprite("Momo", art != null ? art.Player : null, 10);
            player = playerSr.transform;
            if (art != null && art.Player != null)
            {
                Vector2 size = art.Player.bounds.size;
                float s = playerHeight / size.y;
                player.localScale = new Vector3(s, s, 1f);
            }
            else player.localScale = new Vector3(0.9f, playerHeight, 1f);
        }

        void BuildKillZone()
        {
            killZone = NewSprite("DangerZone", white, 30);
            killZone.color = new Color(1f, 0.1f, 0.2f, 0.28f);
            killLine = NewSprite("DangerLine", white, 31);
            killLine.color = new Color(1f, 0.3f, 0.35f, 0.9f);
            // Hazard stripes along the line: the danger zone is not signalled by red alone (CUX-6).
            if (patterns == null) patterns = new Sprite[5];
            patterns[3] = MakePattern(3);
            dangerBand = NewSprite("DangerBand", patterns[3], 32);
            dangerBand.drawMode = SpriteDrawMode.Tiled;
            dangerBand.size = new Vector2(camHalfW * 2f + 2f, 0.3f);
            dangerBand.color = new Color(1f, 1f, 1f, 0.8f);
        }

        Pop MakePop()
        {
            var sr = NewSprite("pop", null, 20);
            sr.enabled = false;
            return new Pop { Tf = sr.transform, Sr = sr };
        }

        // ---------------- per tick / per frame ----------------

        /// <summary>Call once after every Sim tick: remembers the previous and current player position for interpolation and spawns effects.</summary>
        public void OnTick(bool coinTaken, float coinX, float coinY)
        {
            prevX = curX;
            prevY = curY;
            curX = session.State.X.F();
            curY = session.State.Y.F();
            bool grounded = session.State.Grounded;
            if (grounded && !wasGrounded) { squash = 1f; Spawn(curX, curY + 0.1f, 0.9f, new Color(1f, 1f, 1f, 0.6f), 0.25f, 0.2f, ArtCatalog.Pick(art != null ? art.DustPuff : null, 0)); }
            wasGrounded = grounded;
            if (coinTaken) Spawn(coinX, coinY, 0.7f, new Color(1f, 0.9f, 0.3f, 1f), 0.3f, 0.8f, ArtCatalog.Pick(art != null ? art.Sparkles : null, 0));
        }

        void Spawn(float x, float y, float size, Color color, float life, float rise, Sprite sprite)
        {
            if (reduced) return;
            Pop p = pops[nextPop];
            nextPop = (nextPop + 1) % pops.Length;
            p.Age = 0f;
            p.Life = life;
            p.Size = size;
            p.Rise = rise;
            p.Sr.sprite = sprite != null ? sprite : white;
            p.Sr.color = color;
            p.Sr.enabled = true;
            p.Tf.position = new Vector3(x, y, 0f);
            if (sprite != null)
            {
                Vector2 sz = sprite.bounds.size;
                float s = size / Mathf.Max(sz.x, sz.y);
                p.Tf.localScale = new Vector3(s, s, 1f);
            }
            else p.Tf.localScale = new Vector3(size * 0.4f, size * 0.4f, 1f);
        }

        const float camSmoothing = 11f;
        float frameDt = 1f / 60f;

        public void Render(float alpha, float deltaTime)
        {
            if (session == null) return;
            frameDt = deltaTime;
            SimState s = session.State;
            int tick = s.Tick;

            // Player (interpolated, squash and stretch from vertical speed and landing).
            float px = Mathf.Lerp(prevX, curX, alpha);
            float py = Mathf.Lerp(prevY, curY, alpha);
            float vy = s.Vy.F();
            squash = Mathf.Max(0f, squash - deltaTime * 6f);
            float stretch = reduced ? 1f : 1f + Mathf.Clamp(Mathf.Abs(vy) * 1.6f, 0f, 0.25f) - squash * 0.22f;
            float h = playerHeight * stretch;
            float widthFactor = 1f / Mathf.Max(0.7f, stretch);
            if (art != null && art.Player != null)
            {
                Vector2 size = art.Player.bounds.size;
                float sy = h / size.y;
                float sx = (playerHeight / size.y) * widthFactor;
                float facing = s.Vx.Raw < -1000 ? -1f : (s.Vx.Raw > 1000 ? 1f : Mathf.Sign(player.localScale.x));
                player.localScale = new Vector3(sx * (facing < 0 ? -1f : 1f), sy, 1f);
            }
            player.position = new Vector3(px, py + h * 0.5f, 0f);
            player.rotation = reduced ? Quaternion.identity : Quaternion.Euler(0f, 0f, Mathf.Clamp(-s.Vx.F() * 40f, -12f, 12f));
            playerSr.color = s.Invuln > 0 && (reduced || (tick / 10) % 2 == 0) ? new Color(1f, 1f, 1f, 0.55f) : Color.white;

            // Platforms (moving ones follow the sim, vanishing ones blink, crumbling ones fade).
            Platform[] plats = session.Data.Platforms;
            for (int i = 0; i < plats.Length; i++)
            {
                Platform p = plats[i];
                float x = p.Kind == PlatformKind.Moving ? session.Sim.PlatformX(p.Id, tick).F() : p.X.F();
                float y = p.Y.F();
                var sr = platformSr[i];
                platformTf[i].position = new Vector3(x + platformOffX[i], y + platformOffY[i], 0f);
                if (p.Kind == PlatformKind.Vanishing || p.Kind == PlatformKind.Crumble)
                {
                    bool solid = session.Sim.IsSolid(p.Id, tick, s.CrumbleTick);
                    Color c = PlatformTint(p.Kind);
                    c.a = solid ? 1f : 0.2f;
                    if (p.Kind == PlatformKind.Crumble && s.CrumbleTick[i] >= 0 && solid) c.a = reduced ? 0.7f : 0.6f + 0.4f * Mathf.Abs(Mathf.Sin(tick * 0.3f));
                    sr.color = c;
                    if (cueSr[i] != null) cueSr[i].color = new Color(1f, 1f, 1f, c.a);
                }
                if (cueSr[i] != null) cueSr[i].transform.position = new Vector3(x, y - 0.14f, 0f);
            }

            // Pickups (taken ones hide; coins spin through their frames).
            Pickup[] picks = session.Data.Pickups;
            Sprite[] frames = art != null ? art.CoinFrames : null;
            int frame = (tick / 6) % (frames != null && frames.Length > 0 ? frames.Length : 1);
            for (int i = 0; i < picks.Length; i++)
            {
                var sr = pickupSr[i];
                if (s.PickupTaken != null && s.PickupTaken[i]) { if (sr.enabled) sr.enabled = false; continue; }
                if (picks[i].Kind == PickupKind.Coin && frames != null && frames.Length > 0 && sr.sprite != frames[frame])
                {
                    sr.sprite = frames[frame];
                    Vector2 sz = sr.sprite.bounds.size;
                    float d = VisualDiameter(picks[i].Kind);
                    float k = d / Mathf.Max(sz.x, sz.y);
                    sr.transform.localScale = new Vector3(k, k, 1f);
                }
            }

            // Hazards.
            for (int i = 0; i < hazardTf.Length; i++)
            {
                Fx hx, hy;
                session.Sim.HazardPosition(session.Data.Hazards[i].Id, tick, out hx, out hy);
                hazardTf[i].position = new Vector3(hx.F(), hy.F(), 0f);
                hazardTf[i].rotation = Quaternion.Euler(0f, 0f, Mathf.Sin(tick * 0.05f + i) * 12f);
            }

            // Effects.
            for (int i = 0; i < pops.Length; i++)
            {
                Pop p = pops[i];
                if (p.Age > p.Life) { if (p.Sr.enabled) p.Sr.enabled = false; continue; }
                p.Age += deltaTime;
                float t = Mathf.Clamp01(p.Age / p.Life);
                Color c = p.Sr.color;
                c.a = 1f - t;
                p.Sr.color = c;
                p.Tf.position += new Vector3(0f, p.Rise * deltaTime, 0f);
                p.Tf.localScale *= 1f + deltaTime * 2.5f;
            }

            UpdateCamera(py, s.KillY.F());
            UpdateBackground();
            UpdateWalls();
            UpdateKillZone(s.KillY.F());
        }

        /// <summary>Fits the view width to the screen width for the current aspect (called every frame: rotation, split screen, tests).</summary>
        void FitCamera()
        {
            float aspect = AspectOverride > 0f ? AspectOverride : cam.aspect;
            float h = camHalfW / Mathf.Max(0.3f, aspect);
            if (Mathf.Abs(h - camHalfH) > 0.0001f)
            {
                camHalfH = h;
                cam.orthographicSize = h;
            }
        }

        void UpdateCamera(float playerY, float killY)
        {
            FitCamera();
            float target = playerY + camHalfH * 0.2f;   // the player sits in the lower-middle, above the control pad
            // Time-based smoothing so the follow feels the same at 30, 60 and 120 fps (CUX-1).
            if (target > camY) camY += (target - camY) * (1f - Mathf.Exp(-camSmoothing * frameDt));
            float minCam = killY - 1f + camHalfH;      // the danger line never leaves the bottom of the screen
            if (camY < minCam) camY = minCam;
            ApplyCamera();
        }

        void ApplyCamera()
        {
            cam.transform.position = new Vector3(camHalfW, camY, -10f);
        }

        void UpdateBackground()
        {
            for (int i = 0; i < layers.Count; i++)
            {
                Layer l = layers[i];
                float scrolled = reduced ? 0f : camY * l.Factor;
                float offset = Mathf.Repeat(scrolled, l.Height);
                int baseIdx = Mathf.FloorToInt(scrolled / l.Height);
                for (int k = 0; k < l.Tiles.Length; k++)
                {
                    // Every other tile is mirrored vertically so neighbouring tiles meet on identical pixels: no visible seam.
                    bool mirror = (((baseIdx + k - 1) % 2) + 2) % 2 == 1;
                    l.Tiles[k].transform.localScale = new Vector3(l.Scale, mirror ? -l.Scale : l.Scale, 1f);
                    float cy = camY - offset + (k - 1) * l.Height;
                    l.Tiles[k].transform.position = new Vector3(camHalfW, cy + l.Height * 0.5f - camHalfH * 0.0f, 0f);
                }
            }
        }

        void UpdateWalls()
        {
            if (wallL == null) return;
            float bottom = camY - camHalfH - wallSegHeight;
            int first = Mathf.FloorToInt(bottom / wallSegHeight);
            for (int i = 0; i < wallL.Length; i++)
            {
                float y = (first + i) * wallSegHeight + wallSegHeight * 0.5f;
                if (wallL[i] != null) wallL[i].transform.position = new Vector3(0.3f, y, 0f);
                if (wallR[i] != null) wallR[i].transform.position = new Vector3(camHalfW * 2f - 0.3f, y, 0f);
            }
        }

        void UpdateKillZone(float killY)
        {
            killZone.transform.position = new Vector3(camHalfW, killY - 4f, 0f);
            killZone.transform.localScale = new Vector3(camHalfW * 2f + 2f, 8f, 1f);
            killLine.transform.position = new Vector3(camHalfW, killY, 0f);
            killLine.transform.localScale = new Vector3(camHalfW * 2f + 2f, 0.08f, 1f);
            dangerBand.transform.position = new Vector3(camHalfW, killY + 0.19f, 0f);
        }
    }
}
