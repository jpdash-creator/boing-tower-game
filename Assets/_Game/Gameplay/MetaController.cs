using System;
using System.Collections.Generic;
using BoingTower.Content;
using BoingTower.Economy;
using BoingTower.Simulation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BoingTower.Gameplay
{
    /// <summary>
    /// Meta scene: main menu, world map, level select (15 levels per region, stars, locks), level info popup,
    /// region gem-skip popup and settings. Built in code from the sliced art (no prefabs yet).
    /// </summary>
    public sealed class MetaController : MonoBehaviour
    {
        // Positions of the five live regions on the map (fractions of the screen), bottom to top, then "coming soon".
        static readonly Vector2[] MapSpots =
        {
            new Vector2(0.28f, 0.19f), new Vector2(0.70f, 0.302f), new Vector2(0.30f, 0.414f), new Vector2(0.70f, 0.526f), new Vector2(0.32f, 0.638f),
            new Vector2(0.70f, 0.75f)
        };

        ContentRepository repo;
        ProgressService progress;
        ArtCatalog art;
        Canvas canvas;
        RectTransform screenRoot; // full-bleed root (backgrounds)
        RectTransform screen;     // safe-area content of the current screen: all text and buttons live under here
        RectTransform popup;
        RectTransform momo;
        Vector2 momoBase;
        Text coinText, gemText;
        string currentRegionId;

        public string CurrentScreen { get; private set; }

        void Start()
        {
            GameSession.EnsureInit();
            GameSettings.ApplyTextScale();
            repo = GameSession.Repo;
            progress = GameSession.Progress;
            art = GameSession.Art;
            canvas = UiKit.CreateCanvas("MetaUi", 0);
            canvas.transform.SetParent(transform, false);
            progress.Changed += RefreshWallet;

            if (!string.IsNullOrEmpty(GameSession.PendingRegionId))
            {
                string id = GameSession.PendingRegionId;
                GameSession.PendingRegionId = null;
                ShowLevels(repo.GetRegion(id));
            }
            else ShowMenu();
        }

        void OnDestroy()
        {
            if (progress != null) progress.Changed -= RefreshWallet;
        }

        void Update()
        {
            if (momo != null && !GameSettings.ReducedMotion) momo.anchoredPosition = momoBase + new Vector2(0f, Mathf.Sin(Time.unscaledTime * 2.2f) * 22f);
        }

        // ---------------- helpers ----------------

        RectTransform NewScreen(string name)
        {
            if (screenRoot != null) Destroy(screenRoot.gameObject);
            ClosePopup();
            momo = null;
            coinText = null;
            gemText = null;
            starText = null;
            screenRoot = UiKit.Rect(canvas.transform, name);
            UiKit.Stretch(screenRoot);
            screen = UiKit.SafeArea(screenRoot);
            CurrentScreen = name;
            return screen;
        }

        static void DropOutline(Text t)
        {
            var o = t.GetComponent<Outline>();
            if (o != null) o.enabled = false;
        }

        Sprite Btn(int i) { return art != null ? ArtCatalog.Pick(art.Buttons, i) : null; }

        void Background(Sprite sprite, Color fallback)
        {
            Image bg = UiKit.AddImage(screenRoot, "Bg", sprite, sprite != null ? Color.white : fallback);
            bg.preserveAspect = false;
            UiKit.Stretch(bg.rectTransform);
            bg.transform.SetAsFirstSibling();
            // A cover-fit background: scales to fill any aspect without stretching the art.
            if (sprite != null)
            {
                var fit = bg.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = sprite.rect.width / sprite.rect.height;
            }
        }

        Text starText;

        /// <summary>Stretches a header between the icon button (left) and the wallet panel (right) so the two can never overlap.</summary>
        static void BesideWallet(RectTransform rt, float yFromTop, float height)
        {
            rt.anchorMin = new Vector2(0f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(0f, 1f);
            rt.offsetMin = new Vector2(190f, yFromTop - height);
            rt.offsetMax = new Vector2(-310f, yFromTop);
        }

        /// <summary>Wallet: one dark rounded panel at the top-right with three aligned rows (coins, gems, stars): [icon][number].</summary>
        void AddWallet()
        {
            const float rowH = 84f, panelW = 270f;
            Image panel = UiKit.AddPanel(screen, "WalletPanel", new Color(0.05f, 0.07f, 0.16f, 0.92f));
            UiKit.Place(panel.rectTransform, new Vector2(1f, 1f), new Vector2(-20f, -20f), new Vector2(panelW, rowH * 3f + 30f));
            coinText = WalletRow(panel.transform, 0, art != null ? art.CoinIcon : null, Color.white);
            gemText = WalletRow(panel.transform, 1, art != null ? art.GemIcon : null, new Color(0.75f, 0.92f, 1f));
            starText = WalletRow(panel.transform, 2, art != null ? art.StarFilled : null, new Color(1f, 0.92f, 0.5f));
            RefreshWallet();
        }

        Text WalletRow(Transform parent, int row, Sprite icon, Color numberColor)
        {
            float y = -15f - row * 84f;
            Image ic = UiKit.AddImage(parent, "Icon" + row, icon, Color.white);
            UiKit.Place(ic.rectTransform, new Vector2(0f, 1f), new Vector2(22f, y - 8f), new Vector2(68f, 68f));
            Text t = UiKit.AddText(parent, "Value" + row, "", 48, numberColor, TextAnchor.MiddleRight, true);
            UiKit.Place((RectTransform)t.transform, new Vector2(1f, 1f), new Vector2(-22f, y), new Vector2(140f, 84f));
            return t;
        }

        void RefreshWallet()
        {
            if (coinText != null) UiKit.SetText(coinText, progress.Coins.ToString());
            if (gemText != null) UiKit.SetText(gemText, progress.Gems.ToString());
            if (starText != null)
            {
                int stars = 0;
                for (int i = 1; i <= GameSession.PlayableRegions; i++)
                {
                    RegionDef r = repo.TryGetRegionByIndex(i);
                    if (r != null) stars += progress.StarsInRegion(r);
                }
                UiKit.SetText(starText, stars.ToString());
            }
        }

        /// <summary>Round icon button at the top-left (home or back arrow): no text, with an accessibility label.</summary>
        void AddBack(string label, Action action)
        {
            int icon = label == "Menu" ? 2 : 3;   // round buttons: 2 = home, 3 = back arrow
            Sprite sprite = art != null ? ArtCatalog.Pick(art.RoundButtons, icon) : null;
            Button b = UiKit.AddButton(screen, "Back", "", sprite, Color.white, new Vector2(150f, 150f), action, 40);
            b.GetComponent<A11yLabel>().Text = label;
            UiKit.Place((RectTransform)b.transform, new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(150f, 150f));
            if (sprite == null)
            {
                Text t = UiKit.AddText(b.transform, "Fallback", label, 40, UiKit.ButtonInk);
                UiKit.Stretch((RectTransform)t.transform);
            }
        }

        // ---------------- main menu ----------------

        public void ShowMenu()
        {
            NewScreen("Menu");
            RegionArt meadow = art != null ? art.ForRegion(1) : null;
            Background(meadow != null ? meadow.BgFar : null, new Color(0.08f, 0.09f, 0.16f, 1f));
            Text title = UiKit.AddText(screen, "Title", "BOING\nTOWER", 150, Color.white);
            UiKit.Place((RectTransform)title.transform, new Vector2(0.5f, 1f), new Vector2(0f, -110f), new Vector2(900f, 340f));
            if (art != null && art.Player != null)
            {
                Image m = UiKit.AddImage(screen, "Momo", art.Player, Color.white);
                UiKit.Place(m.rectTransform, new Vector2(0.5f, 0.47f), Vector2.zero, new Vector2(440f, 540f));
                m.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                momo = m.rectTransform;
                momoBase = momo.anchoredPosition;
            }
            Button play = UiKit.AddButton(screen, "Play", "PLAY", Btn(1), Color.white, new Vector2(520f, 150f), ShowMap, 72);
            UiKit.Place((RectTransform)play.transform, new Vector2(0.5f, 0f), new Vector2(0f, 290f), new Vector2(520f, 150f));
            Button settings = UiKit.AddButton(screen, "Settings", "Settings", Btn(0), new Color(0.9f, 0.95f, 1f), new Vector2(380f, 130f), ShowSettings, 46);
            UiKit.Place((RectTransform)settings.transform, new Vector2(0.5f, 0f), new Vector2(0f, 135f), new Vector2(380f, 130f));
            Text v = UiKit.AddText(screen, "Version", "v" + Application.version + "  prototype", 30, new Color(1f, 1f, 1f, 0.6f));
            UiKit.Place((RectTransform)v.transform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(700f, 60f));
        }

        // ---------------- world map ----------------

        public void ShowMap()
        {
            NewScreen("Map");
            currentRegionId = null;
            Background(art != null ? art.MapBackground : null, new Color(0.1f, 0.14f, 0.25f, 1f));
            AddWallet();
            AddBack("Menu", ShowMenu);
            Text hdr = UiKit.AddText(screen, "Header", "World map", 64, Color.white, TextAnchor.MiddleLeft);
            BesideWallet((RectTransform)hdr.transform, -30f, 130f);

            for (int i = 0; i < MapSpots.Length; i++)
            {
                RegionDef region = repo.TryGetRegionByIndex(i + 1);
                if (region == null) continue;
                AddRegionNode(region, MapSpots[i]);
            }
        }

        void AddRegionNode(RegionDef region, Vector2 anchor)
        {
            bool playable = progress.IsRegionPlayable(region);
            bool unlocked = progress.IsRegionUnlocked(region);
            int stars = playable ? progress.StarsInRegion(region) : 0;
            int cleared = 0;
            if (playable) foreach (string id in region.Levels) if (progress.IsLevelCleared(id)) cleared++;

            // MapNodes order from the art sheet: unlocked, current, completed, locked, teaser, ring.
            int sprite = !playable ? 4 : (!unlocked ? 3 : (cleared >= region.Levels.Count ? 2 : (cleared > 0 ? 1 : 0)));
            Sprite nodeSprite = art != null ? ArtCatalog.Pick(art.MapNodes, sprite) : null;

            Button b = UiKit.AddButton(screen, "Region" + region.Index, "", nodeSprite, Color.white, new Vector2(180f, 180f), () => OnRegionTapped(region), 40);
            RectTransform rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            Image img = b.GetComponent<Image>();
            img.preserveAspect = true;

            // No number on the node: the node art and the name label identify the region (owner, 2026-10-19).

            // Ribbon banner under the node (the game's own ribbon art): cream for open regions, tan for locked, no black boxes.
            int ribbonIdx = !playable ? 3 : (!unlocked ? 3 : (cleared >= region.Levels.Count ? 2 : 0));
            Sprite ribbon = art != null ? ArtCatalog.Pick(art.Ribbons, ribbonIdx) : null;
            float rw = 500f, rh = 140f;   // the ribbon overlaps the lower rim of the node, so the labels need little vertical room
            if (ribbon != null) { Vector2 rs = ribbon.bounds.size; rh = rw * rs.y / rs.x; }
            Image rib = UiKit.AddImage(rt, "Ribbon", ribbon, ribbon != null ? Color.white : new Color(0.95f, 0.9f, 0.8f, 1f));
            rib.rectTransform.anchorMin = rib.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            rib.rectTransform.pivot = new Vector2(0.5f, 1f);
            rib.rectTransform.anchoredPosition = new Vector2(0f, 44f);
            rib.rectTransform.sizeDelta = new Vector2(rw, rh);
            string label = playable ? region.Name : "Coming soon";
            Text name = UiKit.AddText(rt, "Name", label, 44, UiKit.ButtonInk, TextAnchor.MiddleCenter, true);
            DropOutline(name);
            RectTransform nrt = (RectTransform)name.transform;
            nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0f);
            nrt.pivot = new Vector2(0.5f, 0.5f);
            nrt.anchoredPosition = new Vector2(0f, 44f - rh * 0.5f);
            nrt.sizeDelta = new Vector2(rw * 0.80f, rh * 0.5f);
            // Star counts live on the level-select screen and in the wallet; the map stays uncluttered.
        }

        public void OnRegionTapped(RegionDef region)
        {
            if (!progress.IsRegionPlayable(region)) { ShowMessage("Coming soon", region.Name + " is not open yet."); return; }
            if (progress.IsRegionUnlocked(region)) { ShowLevels(region); return; }
            ShowUnlockPopup(region);
        }

        void ShowUnlockPopup(RegionDef region)
        {
            int price = progress.GemSkipPrice(region);
            RegionDef prev = repo.GetRegionByIndex(region.Index - 1);
            bool isNext = region.Index == progress.HighestUnlockedRegionIndex() + 1;
            string body = "Clear level 15 of " + prev.Name + " to open " + region.Name + ".";
            if (isNext) body += "\nOr skip ahead for " + price + " gems (you have " + progress.Gems + ").";
            OpenPopup(region.Name + " is locked", body);
            if (isNext)
            {
                bool can = progress.Gems >= price;
                Button b = PopupButton("Unlock for " + price + " gems", Btn(1), can ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f), 230f, () =>
                {
                    if (progress.TrySkipToRegion(region) == SkipResult.Ok) ShowMap();
                });
                b.interactable = can;
            }
            PopupButton("Close", Btn(0), Color.white, 60f, ClosePopup);
        }

        // ---------------- level select ----------------

        public void ShowLevels(RegionDef region)
        {
            NewScreen("Levels");
            currentRegionId = region.Id;
            RegionArt ra = art != null ? art.ForRegion(region.Index) : null;
            Background(ra != null ? ra.BgFar : null, new Color(0.1f, 0.14f, 0.25f, 1f));
            AddWallet();
            AddBack("Map", ShowMap);
            Text hdr = UiKit.AddText(screen, "Header", region.Name, 56, Color.white, TextAnchor.MiddleLeft);
            BesideWallet((RectTransform)hdr.transform, -20f, 180f);
            Text sub = UiKit.AddText(screen, "Sub", progress.StarsInRegion(region) + " / " + region.Levels.Count * 3 + " stars", 44, new Color(1f, 0.92f, 0.5f), TextAnchor.MiddleLeft);
            BesideWallet((RectTransform)sub.transform, -205f, 60f);

            List<LevelDef> levels = repo.LevelsOf(region.Id);
            string currentId = null;
            foreach (LevelDef l in levels) if (progress.IsLevelUnlocked(l) && !progress.IsLevelCleared(l.Id)) { currentId = l.Id; break; }

            // 3 columns x 5 rows, level 1 at the bottom (the tower climbs upward).
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDef l = levels[i];
                int row = i / 3, col = i % 3;
                AddLevelNode(l, new Vector2(0.2f + 0.3f * col, 0.115f + row * 0.143f), l.Id == currentId);
            }
        }

        // Sprite order of ui_level_nodes after slicing: 0 plain bubble, 1 gem bubble (garbled text), 2 crown, 3 padlock,
        // 4 = three small pieces (NOT a node), 5 large bubble.
        void AddLevelNode(LevelDef level, Vector2 anchor, bool isCurrent)
        {
            bool unlocked = progress.IsLevelUnlocked(level);
            bool cleared = progress.IsLevelCleared(level.Id);
            bool boss = level.LevelClass == LevelDef.ClassHighlyComplex;
            bool complex = level.LevelClass == LevelDef.ClassComplex;
            int sprite = !unlocked ? 3 : (boss ? 2 : (cleared ? 5 : 0));
            Color tint = Color.white;
            if (unlocked && !boss && cleared) tint = new Color(1f, 0.93f, 0.55f, 1f);   // completed: warm gold bubble
            else if (isCurrent) tint = new Color(1f, 1f, 0.7f, 1f);                     // current: bright
            Sprite nodeSprite = art != null ? ArtCatalog.Pick(art.LevelNodes, sprite) : null;
            Button b = UiKit.AddButton(screen, "Level" + level.Index, "", nodeSprite, tint, new Vector2(190f, 190f), () => OnLevelTapped(level), 40);
            RectTransform rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            b.GetComponent<Image>().preserveAspect = true;
            if (isCurrent) rt.localScale = Vector3.one * 1.12f;
            if (unlocked && !boss)
            {
                Text num = UiKit.AddText(rt, "Num", level.Index.ToString(), 72, UiKit.ButtonInk);
                DropOutline(num);
                UiKit.Stretch((RectTransform)num.transform);
            }
            if (unlocked && complex && art != null && art.GemIcon != null)
            {
                Image gem = UiKit.AddImage(rt, "GemBadge", art.GemIcon, Color.white);
                gem.rectTransform.anchorMin = gem.rectTransform.anchorMax = new Vector2(1f, 1f);
                gem.rectTransform.pivot = new Vector2(0.5f, 0.5f);
                gem.rectTransform.anchoredPosition = new Vector2(-14f, -14f);
                gem.rectTransform.sizeDelta = new Vector2(64f, 64f);
            }
            LevelProgress p = progress.GetProgress(level.Id);
            int stars = p != null ? p.Stars : 0;
            for (int s = 0; s < 3 && unlocked; s++)
            {
                Sprite star = art != null ? (s < stars ? art.StarFilled : art.StarEmpty) : null;
                Image si = UiKit.AddImage(rt, "S" + s, star, s < stars ? Color.white : new Color(1f, 1f, 1f, 0.45f));
                RectTransform srt = si.rectTransform;
                srt.anchorMin = srt.anchorMax = new Vector2(0.5f, 0f);
                srt.pivot = new Vector2(0.5f, 1f);
                srt.anchoredPosition = new Vector2((s - 1) * 52f, -4f);
                srt.sizeDelta = new Vector2(50f, 50f);
            }
        }

        public void OnLevelTapped(LevelDef level)
        {
            if (!progress.IsLevelUnlocked(level)) { ShowMessage("Locked", "Clear level " + (level.Index - 1) + " first."); return; }
            LevelSpec spec = LevelSpecFactory.Build(level, repo.GetRegion(level.Region));
            LevelProgress p = progress.GetProgress(level.Id);
            int starsHave = p != null ? p.Stars : 0;
            OpenPopup("Level " + level.Index, "");
            Transform panel = popup.Find("Panel");
            Destroy(panel.Find("Body").gameObject);

            // Everything is centred on the panel: goal, the three stars, what pays, then the buttons.
            Text goal = UiKit.AddText(panel, "Goal", RunSession.DescribeGoal(spec), 52, Color.white, TextAnchor.MiddleCenter, true);
            UiKit.OnPanel(goal);
            UiKit.Place((RectTransform)goal.transform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(740f, 130f));
            for (int i = 0; i < 3; i++)
            {
                bool got = i < starsHave;
                Image star = UiKit.AddImage(panel, "Star" + i, art != null ? (got ? art.StarFilled : art.StarEmpty) : null, got ? Color.white : new Color(1f, 1f, 1f, 0.55f));
                UiKit.Place(star.rectTransform, new Vector2(0.5f, 1f), new Vector2((i - 1) * 170f, -300f), new Vector2(150f, 150f));
                star.rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            string info = "Collect the 3 stars on the way up.\nPays 30 coins + 40 per star + 5 per combo.";
            if (starsHave >= 3) info = "All 3 stars collected.\nReplays pay half the coins and no gems.";
            if (p != null && p.BestScore > 0) info += "\nBest score " + p.BestScore;
            Text t = UiKit.AddText(panel, "Info", info, 42, Color.white, TextAnchor.UpperCenter);
            UiKit.OnPanel(t);
            UiKit.Place((RectTransform)t.transform, new Vector2(0.5f, 1f), new Vector2(0f, -465f), new Vector2(740f, 330f));
            PopupButton("PLAY", Btn(1), Color.white, 200f, () =>
            {
                GameSession.CurrentLevelId = level.Id;
                SceneManager.LoadScene("Run");
            });
            PopupButton("Close", Btn(0), Color.white, 40f, ClosePopup);
        }

        // ---------------- settings ----------------

        public void ShowSettings()
        {
            NewScreen("Settings");
            RegionArt meadowBg = art != null ? art.ForRegion(1) : null;
            Background(meadowBg != null ? meadowBg.BgFar : null, new Color(0.08f, 0.09f, 0.16f, 1f));
            AddBack("Back", ShowMenu);
            Text hdr = UiKit.AddText(screen, "Header", "Settings", 80, Color.white);
            UiKit.Place((RectTransform)hdr.transform, new Vector2(0.5f, 1f), new Vector2(0f, -190f), new Vector2(800f, 110f));

            // Backing panel: white labels stay readable over the bright background art (CUX-5).
            Image rows = UiKit.AddImage(screen, "RowsBacking", null, new Color(0.05f, 0.07f, 0.16f, 0.72f));
            rows.rectTransform.anchorMin = new Vector2(0f, 1f);
            rows.rectTransform.anchorMax = new Vector2(1f, 1f);
            rows.rectTransform.pivot = new Vector2(0.5f, 1f);
            rows.rectTransform.anchoredPosition = new Vector2(0f, -315f);
            rows.rectTransform.sizeDelta = new Vector2(-30f, 1070f);
            AddToggle("Sound effects", -340f, () => GameSettings.Sound, v => GameSettings.Sound = v);
            AddToggle("Music", -490f, () => GameSettings.Music, v => GameSettings.Music = v);
            AddToggle("Haptics", -640f, () => GameSettings.Haptics, v => GameSettings.Haptics = v);
            AddToggle("Reduced motion", -790f, () => GameSettings.ReducedMotion, v => GameSettings.ReducedMotion = v);
            AddToggle("Left-handed", -940f, () => GameSettings.LeftHanded, v => GameSettings.LeftHanded = v);
            AddTextSize(-1090f);
            AddDifficulty(-1240f);

            Button reset = UiKit.AddButton(screen, "Reset", "Reset progress", Btn(1), new Color(1f, 0.6f, 0.6f), new Vector2(560f, 130f), ConfirmReset, 46);
            UiKit.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0f, 40f), new Vector2(560f, 130f));
        }

        /// <summary>Difficulty dropdown: the button shows the current level; tapping it opens a list of the five levels.</summary>
        void AddDifficulty(float yFromTop)
        {
            Text t = UiKit.AddText(screen, "Difficulty", "Difficulty", 54, Color.white, TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)t.transform, new Vector2(0f, 1f), new Vector2(70f, yFromTop), new Vector2(470f, 130f));
            Button b = UiKit.AddButton(screen, "DifficultyButton", GameSettings.DifficultyNames[GameSettings.Difficulty] + " v", Btn(0), Color.white, new Vector2(300f, 130f), ShowDifficultyList, 44);
            UiKit.Place((RectTransform)b.transform, new Vector2(1f, 1f), new Vector2(-70f, yFromTop), new Vector2(300f, 130f));
        }

        public void ShowDifficultyList()
        {
            OpenPopup("Difficulty", "How fast the screen speeds up. Harder levels also drop more gems.");
            Transform panel = popup.Find("Panel");
            for (int i = 0; i < GameSettings.DifficultyNames.Length; i++)
            {
                int level = i;
                bool current = level == GameSettings.Difficulty;
                string label = GameSettings.DifficultyLabel(level) + (GameSettings.DifficultyGemPercent[level] > 5 ? "  gems " + GameSettings.DifficultyGemPercent[level] + "%" : "");
                Button opt = UiKit.AddButton(panel, "Diff" + level, label, Btn(current ? 1 : 0), Color.white, new Vector2(700f, 100f), () =>
                {
                    GameSettings.Difficulty = level;
                    GameSession.ApplyDifficulty();
                    ClosePopup();
                    ShowSettings();
                }, 40);
                UiKit.Place((RectTransform)opt.transform, new Vector2(0.5f, 0f), new Vector2(0f, 40f + (GameSettings.DifficultyNames.Length - 1 - i) * 125f), new Vector2(700f, 100f));
            }
        }

        void AddTextSize(float yFromTop)
        {
            Text t = UiKit.AddText(screen, "Text size", "Text size", 54, Color.white, TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)t.transform, new Vector2(0f, 1f), new Vector2(70f, yFromTop), new Vector2(470f, 130f));
            Button b = UiKit.AddButton(screen, "TextSizeButton", GameSettings.TextPercent + "%", Btn(0), Color.white, new Vector2(240f, 130f), () =>
            {
                GameSettings.NextTextPercent();
                ShowSettings();   // rebuild so the new size applies to every label
            }, 48);
            UiKit.Place((RectTransform)b.transform, new Vector2(1f, 1f), new Vector2(-70f, yFromTop), new Vector2(240f, 130f));
        }

        void AddToggle(string label, float yFromTop, Func<bool> get, Action<bool> set)
        {
            Text t = UiKit.AddText(screen, label, label, 54, Color.white, TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)t.transform, new Vector2(0f, 1f), new Vector2(70f, yFromTop), new Vector2(470f, 130f));
            Button b = null;
            b = UiKit.AddButton(screen, label + "Toggle", get() ? "ON" : "OFF", Btn(get() ? 1 : 2), Color.white, new Vector2(240f, 130f), () =>
            {
                set(!get());
                b.GetComponentInChildren<Text>().text = get() ? "ON" : "OFF";
                b.GetComponent<Image>().sprite = Btn(get() ? 1 : 2);
            }, 48);
            UiKit.Place((RectTransform)b.transform, new Vector2(1f, 1f), new Vector2(-70f, yFromTop), new Vector2(240f, 130f));
        }

        public void ConfirmReset()
        {
            OpenPopup("Reset progress?", "All stars, coins, gems and unlocked regions will be erased on this device.");
            PopupButton("Erase everything", Btn(1), new Color(1f, 0.6f, 0.6f), 230f, () =>
            {
                progress.ResetAll();
                ClosePopup();
                ShowMenu();
            });
            PopupButton("Cancel", Btn(0), Color.white, 60f, ClosePopup);
        }

        // ---------------- popups ----------------

        void OpenPopup(string title, string body, float height = 1180f)
        {
            ClosePopup();
            popup = UiKit.Rect(canvas.transform, "Popup");
            UiKit.Stretch(popup);
            var dim = popup.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            RectTransform panel = UiKit.BuildPopupFrame(popup, title, art, new Vector2(860f, height), ClosePopup);
            Text b = UiKit.AddText(panel, "Body", body, 44, Color.white);
            UiKit.OnPanel(b);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0f, -240f), new Vector2(740f, 520f));
        }

        Button PopupButton(string label, Sprite sprite, Color tint, float y, Action action)
        {
            Button b = UiKit.AddButton(popup.Find("Panel"), label, label, sprite, tint, new Vector2(560f, 130f), action, 46);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(560f, 130f));
            return b;
        }

        void ShowMessage(string title, string body)
        {
            OpenPopup(title, body, 860f);
            PopupButton("OK", Btn(0), Color.white, 60f, ClosePopup);
        }

        void ClosePopup()
        {
            if (popup != null) Destroy(popup.gameObject);
            popup = null;
        }
    }
}
