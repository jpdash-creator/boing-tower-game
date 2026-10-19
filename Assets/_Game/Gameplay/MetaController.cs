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
            new Vector2(0.28f, 0.16f), new Vector2(0.70f, 0.27f), new Vector2(0.30f, 0.38f), new Vector2(0.70f, 0.49f), new Vector2(0.32f, 0.60f),
            new Vector2(0.70f, 0.72f)
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
            if (momo != null) momo.anchoredPosition = momoBase + new Vector2(0f, Mathf.Sin(Time.unscaledTime * 2.2f) * 22f);
        }

        // ---------------- helpers ----------------

        RectTransform NewScreen(string name)
        {
            if (screenRoot != null) Destroy(screenRoot.gameObject);
            ClosePopup();
            momo = null;
            coinText = null;
            gemText = null;
            screenRoot = UiKit.Rect(canvas.transform, name);
            UiKit.Stretch(screenRoot);
            screen = UiKit.SafeArea(screenRoot);
            CurrentScreen = name;
            return screen;
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

        void AddWallet()
        {
            // [icon][number], right-aligned block: number box 190 px wide, icon directly left of it.
            Image coinIcon = UiKit.AddImage(screen, "CoinIcon", art != null ? art.CoinIcon : null, Color.white);
            UiKit.Place(coinIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-230f, -30f), new Vector2(64f, 64f));
            coinText = UiKit.AddText(screen, "Coins", "", 48, Color.white, TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)coinText.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(190f, 64f));
            Image gemIcon = UiKit.AddImage(screen, "GemIcon", art != null ? art.GemIcon : null, Color.white);
            UiKit.Place(gemIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-230f, -104f), new Vector2(64f, 64f));
            gemText = UiKit.AddText(screen, "Gems", "", 48, new Color(0.7f, 0.9f, 1f), TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)gemText.transform, new Vector2(1f, 1f), new Vector2(-30f, -104f), new Vector2(190f, 64f));
            RefreshWallet();
        }

        void RefreshWallet()
        {
            if (coinText != null) UiKit.SetText(coinText, progress.Coins.ToString());
            if (gemText != null) UiKit.SetText(gemText, progress.Gems.ToString());
        }

        void AddBack(string label, Action action)
        {
            Button b = UiKit.AddButton(screen, "Back", label, Btn(2), new Color(1f, 1f, 1f, 0.95f), new Vector2(260f, 130f), action, 44);
            UiKit.Place((RectTransform)b.transform, new Vector2(0f, 1f), new Vector2(20f, -20f), new Vector2(260f, 130f));
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
            Text hdr = UiKit.AddText(screen, "Header", "World map", 64, Color.white);
            UiKit.Place((RectTransform)hdr.transform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(600f, 90f));

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

            Button b = UiKit.AddButton(screen, "Region" + region.Index, "", nodeSprite, Color.white, new Vector2(230f, 230f), () => OnRegionTapped(region), 40);
            RectTransform rt = (RectTransform)b.transform;
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = Vector2.zero;
            Image img = b.GetComponent<Image>();
            img.preserveAspect = true;

            Text num = UiKit.AddText(rt, "Num", region.Index.ToString(), 90, Color.white);
            UiKit.Stretch((RectTransform)num.transform);

            string label = playable ? region.Name + "\n" + stars + " / " + region.Levels.Count * 3 + " stars" : "Coming soon";
            Image backing = UiKit.AddImage(rt, "NameBacking", null, new Color(0f, 0f, 0f, 0.5f));
            RectTransform brt = backing.rectTransform;
            brt.anchorMin = brt.anchorMax = new Vector2(0.5f, 0f);
            brt.pivot = new Vector2(0.5f, 1f);
            brt.anchoredPosition = new Vector2(0f, -2f);
            brt.sizeDelta = new Vector2(330f, 96f);
            Text name = UiKit.AddText(rt, "Name", label, 38, Color.white);
            RectTransform nrt = (RectTransform)name.transform;
            nrt.anchorMin = nrt.anchorMax = new Vector2(0.5f, 0f);
            nrt.pivot = new Vector2(0.5f, 1f);
            nrt.anchoredPosition = new Vector2(0f, -2f);
            nrt.sizeDelta = new Vector2(330f, 96f);
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
            Text hdr = UiKit.AddText(screen, "Header", region.Name, 70, Color.white);
            UiKit.Place((RectTransform)hdr.transform, new Vector2(0.5f, 1f), new Vector2(0f, -170f), new Vector2(900f, 100f));
            Text sub = UiKit.AddText(screen, "Sub", progress.StarsInRegion(region) + " / " + region.Levels.Count * 3 + " stars", 44, new Color(1f, 0.92f, 0.5f));
            UiKit.Place((RectTransform)sub.transform, new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(600f, 60f));

            List<LevelDef> levels = repo.LevelsOf(region.Id);
            string currentId = null;
            foreach (LevelDef l in levels) if (progress.IsLevelUnlocked(l) && !progress.IsLevelCleared(l.Id)) { currentId = l.Id; break; }

            // 3 columns x 5 rows, level 1 at the bottom (the tower climbs upward).
            for (int i = 0; i < levels.Count; i++)
            {
                LevelDef l = levels[i];
                int row = i / 3, col = i % 3;
                AddLevelNode(l, new Vector2(0.2f + 0.3f * col, 0.115f + row * 0.154f), l.Id == currentId);
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
                Text num = UiKit.AddText(rt, "Num", level.Index.ToString(), 72, Color.white);
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
            string body = "Goal: " + RunSession.DescribeGoal(spec) + "\n";
            for (int i = 0; i < spec.Stars.Length; i++)
            {
                bool got = p != null && (p.StarMask & (1 << i)) != 0;
                body += (got ? "[x] " : "[  ] ") + RunSession.DescribeStar(spec.Stars[i]) + "\n";
            }
            if (p != null) body += "Best score " + p.BestScore;
            OpenPopup("Level " + level.Index, body);
            PopupButton("PLAY", Btn(1), Color.white, 230f, () =>
            {
                GameSession.CurrentLevelId = level.Id;
                SceneManager.LoadScene("Run");
            });
            PopupButton("Close", Btn(0), Color.white, 60f, ClosePopup);
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

            AddToggle("Sound effects", -400f, () => GameSettings.Sound, v => GameSettings.Sound = v);
            AddToggle("Music", -590f, () => GameSettings.Music, v => GameSettings.Music = v);
            AddToggle("Haptics", -780f, () => GameSettings.Haptics, v => GameSettings.Haptics = v);

            Button reset = UiKit.AddButton(screen, "Reset", "Reset progress", Btn(1), new Color(1f, 0.6f, 0.6f), new Vector2(560f, 130f), ConfirmReset, 46);
            UiKit.Place((RectTransform)reset.transform, new Vector2(0.5f, 0f), new Vector2(0f, 60f), new Vector2(560f, 130f));
        }

        void AddToggle(string label, float yFromTop, Func<bool> get, Action<bool> set)
        {
            Text t = UiKit.AddText(screen, label, label, 54, Color.white, TextAnchor.MiddleLeft);
            UiKit.Place((RectTransform)t.transform, new Vector2(0f, 1f), new Vector2(70f, yFromTop), new Vector2(520f, 130f));
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

        void OpenPopup(string title, string body)
        {
            ClosePopup();
            popup = UiKit.Rect(canvas.transform, "Popup");
            UiKit.Stretch(popup);
            var dim = popup.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);
            Sprite panel = art != null ? ArtCatalog.Pick(art.Popup, 0) ?? ArtCatalog.Pick(art.Panels, 0) : null;
            Image frame = UiKit.AddImage(popup, "Panel", panel, panel != null ? Color.white : new Color(0.1f, 0.12f, 0.2f, 0.96f));
            frame.preserveAspect = false;
            UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860f, 1060f));
            frame.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Text t = UiKit.AddText(frame.transform, "Title", title, 72, Color.white);
            UiKit.Place((RectTransform)t.transform, new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(560f, 170f));
            Text b = UiKit.AddText(frame.transform, "Body", body, 44, Color.white);
            UiKit.OnPanel(b);
            UiKit.AddHitArea(frame.transform, "CloseMark", "Close", new Vector2(1f, 1f), new Vector2(-10f, 10f), new Vector2(140f, 140f), ClosePopup);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 1f), new Vector2(0f, -250f), new Vector2(740f, 380f));
        }

        Button PopupButton(string label, Sprite sprite, Color tint, float y, Action action)
        {
            Button b = UiKit.AddButton(popup.Find("Panel"), label, label, sprite, tint, new Vector2(560f, 130f), action, 46);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(560f, 130f));
            return b;
        }

        void ShowMessage(string title, string body)
        {
            OpenPopup(title, body);
            PopupButton("OK", Btn(0), Color.white, 60f, ClosePopup);
        }

        void ClosePopup()
        {
            if (popup != null) Destroy(popup.gameObject);
            popup = null;
        }
    }
}
