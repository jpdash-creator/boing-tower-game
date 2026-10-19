using System;
using BoingTower.Economy;
using BoingTower.Simulation;
using UnityEngine;
using UnityEngine.UI;

namespace BoingTower.Gameplay
{
    /// <summary>Code-built HUD and overlays (intro, pause, failed, results) for the Run scene.</summary>
    public sealed class RunHud : MonoBehaviour
    {
        public Action OnPause, OnResume, OnRetry, OnNext, OnMenu, OnRevive;
        /// <summary>The close mark of the current overlay (pause: resume, end screens: back to the level select).</summary>
        Action OnClose;

        ArtCatalog art;
        Canvas canvas;
        Text floorText, goalText, coinText, gemText, comboText, timeText, introText;
        RectTransform overlay;
        float comboPulse;
        int lastCombo;
        float introTimer;

        public bool OverlayOpen { get { return overlay != null; } }

        bool mirror;

        /// <summary>Places an element, mirrored horizontally for the left-handed layout.</summary>
        void P(RectTransform rt, Vector2 anchor, Vector2 pos, Vector2 size)
        {
            if (mirror && !Mathf.Approximately(anchor.x, 0.5f)) { anchor.x = 1f - anchor.x; pos.x = -pos.x; }
            UiKit.Place(rt, anchor, pos, size);
        }

        TextAnchor Align(TextAnchor a)
        {
            if (!mirror) return a;
            switch (a)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.UpperRight: return TextAnchor.UpperLeft;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.MiddleRight: return TextAnchor.MiddleLeft;
                default: return a;
            }
        }

        public void Build(RunSession session, ArtCatalog catalog)
        {
            mirror = GameSettings.LeftHanded;
            art = catalog;
            canvas = UiKit.CreateCanvas("RunHud", 10);
            canvas.transform.SetParent(transform, false);

            // Dark backing strip: text and icons stay readable over any background art (CUX-5).
            Image strip = UiKit.AddImage(canvas.transform, "TopBacking", null, new Color(0.05f, 0.07f, 0.14f, 0.72f));
            P(strip.rectTransform, new Vector2(0.5f, 1f), Vector2.zero, new Vector2(0f, 300f));
            strip.rectTransform.anchorMin = new Vector2(0f, 1f);
            strip.rectTransform.anchorMax = new Vector2(1f, 1f);
            strip.rectTransform.sizeDelta = new Vector2(0f, 300f);

            RectTransform root = UiKit.SafeArea(canvas.transform);

            floorText = UiKit.AddText(root, "Floor", "", 46, Color.white, Align(TextAnchor.UpperLeft));
            P((RectTransform)floorText.transform, new Vector2(0f, 1f), new Vector2(30f, -30f), new Vector2(420f, 60f));

            goalText = UiKit.AddText(root, "Goal", RunSession.DescribeGoal(session.Spec), 34, new Color(1f, 0.95f, 0.6f), Align(TextAnchor.UpperLeft));
            P((RectTransform)goalText.transform, new Vector2(0f, 1f), new Vector2(30f, -92f), new Vector2(700f, 90f));

            Image coinIcon = UiKit.AddImage(root, "CoinIcon", art != null ? art.CoinIcon : null, Color.white);
            P(coinIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-190f, -30f), new Vector2(56f, 56f));
            coinText = UiKit.AddText(root, "Coins", "0", 46, Color.white, Align(TextAnchor.MiddleLeft));
            P((RectTransform)coinText.transform, new Vector2(1f, 1f), new Vector2(-30f, -30f), new Vector2(150f, 56f));
            Image gemIcon = UiKit.AddImage(root, "GemIcon", art != null ? art.GemIcon : null, Color.white);
            P(gemIcon.rectTransform, new Vector2(1f, 1f), new Vector2(-190f, -96f), new Vector2(56f, 56f));
            gemText = UiKit.AddText(root, "Gems", "0", 46, new Color(0.7f, 0.9f, 1f), Align(TextAnchor.MiddleLeft));
            P((RectTransform)gemText.transform, new Vector2(1f, 1f), new Vector2(-30f, -96f), new Vector2(150f, 56f));

            Button pause = UiKit.AddButton(root, "Pause", "II", art != null ? ArtCatalog.Pick(art.RoundButtons, 0) : null, new Color(1f, 1f, 1f, 0.95f), new Vector2(130f, 130f), () => { if (OnPause != null) OnPause(); }, 48);
            P((RectTransform)pause.transform, new Vector2(1f, 1f), new Vector2(-30f, -165f), new Vector2(130f, 130f));

            comboText = UiKit.AddText(root, "Combo", "", 72, new Color(1f, 0.85f, 0.2f));
            P((RectTransform)comboText.transform, new Vector2(0.5f, 1f), new Vector2(0f, -260f), new Vector2(700f, 100f));

            timeText = UiKit.AddText(root, "Time", "", 40, Color.white, Align(TextAnchor.UpperLeft));
            P((RectTransform)timeText.transform, new Vector2(0f, 1f), new Vector2(30f, -200f), new Vector2(500f, 60f));

            introText = UiKit.AddText(root, "Intro", session.Region.Name + "\nLevel " + session.Level.Index + "\n" + RunSession.DescribeGoal(session.Spec), 64, Color.white);
            P((RectTransform)introText.transform, new Vector2(0.5f, 0.5f), new Vector2(0f, 120f), new Vector2(900f, 360f));
            introTimer = 2.2f;
        }

        int lastRow = -1, lastCoins = -1, lastGems = -1, lastTimeKey = -1, lastCombo2 = -1;

        public void Refresh(RunSession s, float deltaTime)
        {
            SimState st = s.State;
            // Strings are rebuilt only when a value changes: no per-frame allocations (CUX-1).
            if (st.Row != lastRow) { lastRow = st.Row; UiKit.SetText(floorText, "Floor " + st.Row + " / " + s.Spec.TargetRow()); }
            if (st.Coins != lastCoins) { lastCoins = st.Coins; UiKit.SetText(coinText, st.Coins.ToString()); }
            int gems = st.Gems + st.HiddenGems;
            if (gems != lastGems) { lastGems = gems; UiKit.SetText(gemText, gems.ToString()); }
            if (s.Spec.Goal.Type == GoalType.Speedrun)
            {
                int sec = st.Tick / 60;
                if (sec != lastTimeKey) { lastTimeKey = sec; UiKit.SetText(timeText, sec + " / " + s.Spec.Goal.TimeSec + " s"); }
            }
            else if (s.Spec.Goal.Type == GoalType.CollectCoins)
            {
                if (st.Coins != lastTimeKey) { lastTimeKey = st.Coins; UiKit.SetText(timeText, st.Coins + " / " + s.Spec.Goal.Count + " coins"); }
            }

            if (st.Combo != lastCombo)
            {
                if (st.Combo > 1) comboPulse = 1f;
                lastCombo = st.Combo;
            }
            comboPulse = Mathf.Max(0f, comboPulse - deltaTime * 3f);
            if (st.Combo != lastCombo2) { lastCombo2 = st.Combo; UiKit.SetText(comboText, st.Combo > 1 ? "COMBO x" + st.Combo : ""); }
            if (!GameSettings.ReducedMotion) comboText.transform.localScale = Vector3.one * (1f + comboPulse * 0.35f);

            if (introTimer > 0f)
            {
                introTimer -= deltaTime;
                if (introTimer <= 0f) introText.gameObject.SetActive(false);
            }
        }

        public float IntroRemaining { get { return introTimer; } }

        /// <summary>Ends the level intro immediately (used by tests and could back a tap-to-skip).</summary>
        public void SkipIntro()
        {
            introTimer = 0f;
            if (introText != null) introText.gameObject.SetActive(false);
        }

        // ---------------- overlays ----------------

        void OpenOverlay(string title)
        {
            CloseOverlay();
            overlay = UiKit.Rect(canvas.transform, "Overlay");
            UiKit.Stretch(overlay);
            var dim = overlay.gameObject.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.62f);
            Sprite panel = art != null ? ArtCatalog.Pick(art.Popup, 0) ?? ArtCatalog.Pick(art.Panels, 0) : null;
            Image frame = UiKit.AddImage(overlay, "Panel", panel, panel != null ? Color.white : new Color(0.1f, 0.12f, 0.2f, 0.95f));
            frame.preserveAspect = false;
            UiKit.Place(frame.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0f, 0f), new Vector2(860f, 1180f));
            frame.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            Text t = UiKit.AddText(frame.transform, "Title", title, 80, Color.white);
            UiKit.Place((RectTransform)t.transform, new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(560f, 120f));
            // The panel art has a close mark baked into its top-right corner: make it a real control.
            UiKit.AddHitArea(frame.transform, "CloseMark", "Close", new Vector2(1f, 1f), new Vector2(-10f, 10f), new Vector2(140f, 140f), () => { if (OnClose != null) OnClose(); });
        }

        public void CloseOverlay()
        {
            if (overlay != null) Destroy(overlay.gameObject);
            overlay = null;
        }

        Transform PanelOf() { return overlay.Find("Panel"); }

        Button OverlayButton(string label, Sprite sprite, Color tint, float y, Action action)
        {
            Button b = UiKit.AddButton(PanelOf(), label, label, sprite, tint, new Vector2(560f, 130f), action, 50);
            UiKit.Place((RectTransform)b.transform, new Vector2(0.5f, 0f), new Vector2(0f, y), new Vector2(560f, 130f));
            return b;
        }

        public void ShowPause()
        {
            OpenOverlay("Paused");
            OnClose = () => { if (OnResume != null) OnResume(); };
            Sprite b = art != null ? ArtCatalog.Pick(art.Buttons, 0) : null;
            OverlayButton("Resume", b, Color.white, 640f, () => { if (OnResume != null) OnResume(); });
            OverlayButton("Restart", b, new Color(1f, 0.85f, 0.9f), 480f, () => { if (OnRetry != null) OnRetry(); });
            OverlayButton("Level select", b, new Color(0.9f, 0.95f, 1f), 320f, () => { if (OnMenu != null) OnMenu(); });
        }

        public void ShowFailed(RunSession s, bool canAffordRevive)
        {
            OpenOverlay("You fell!");
            OnClose = () => { if (OnMenu != null) OnMenu(); };
            Text sub = UiKit.AddText(PanelOf(), "Sub", "Floor " + s.State.Row + " of " + s.Spec.TargetRow(), 52, Color.white);
            UiKit.OnPanel(sub);
            UiKit.Place((RectTransform)sub.transform, new Vector2(0.5f, 1f), new Vector2(0f, -220f), new Vector2(760f, 80f));
            Sprite b = art != null ? ArtCatalog.Pick(art.Buttons, 1) : null;
            Button revive = OverlayButton("Revive  (" + RunSession.ReviveGemCost + " gems)", b, canAffordRevive ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f), 640f, () => { if (OnRevive != null) OnRevive(); });
            revive.interactable = canAffordRevive;
            Sprite b2 = art != null ? ArtCatalog.Pick(art.Buttons, 0) : null;
            OverlayButton("Try again", b2, Color.white, 480f, () => { if (OnRetry != null) OnRetry(); });
            OverlayButton("Level select", b2, new Color(0.9f, 0.95f, 1f), 320f, () => { if (OnMenu != null) OnMenu(); });
        }

        public void ShowResults(RunSession s, RewardResult reward, bool hasNext)
        {
            OpenOverlay("Level clear!");
            OnClose = () => { if (OnMenu != null) OnMenu(); };
            Transform panel = PanelOf();
            int mask = s.BuildOutcome().StarMask;
            for (int i = 0; i < 3; i++)
            {
                bool got = (mask & (1 << i)) != 0;
                Image star = UiKit.AddImage(panel, "Star" + i, art != null ? (got ? art.StarFilled : art.StarEmpty) : null, got ? Color.white : new Color(1f, 1f, 1f, 0.6f));
                UiKit.Place(star.rectTransform, new Vector2(0.5f, 1f), new Vector2((i - 1) * 200f, -190f), new Vector2(180f, 180f));
                star.rectTransform.pivot = new Vector2(0.5f, 1f);
            }
            string line = "Coins +" + reward.TotalCoins + "     Gems +" + reward.TotalGems + "\nScore " + s.State.Score + "     Best combo x" + s.State.BestCombo;
            if (reward.FirstClear) line += "\nFirst clear!";
            if (!string.IsNullOrEmpty(reward.UnlockedRegionId)) line += "\nNew region unlocked!";
            Text info = UiKit.AddText(panel, "Info", line, 40, Color.white);
            UiKit.OnPanel(info);
            UiKit.Place((RectTransform)info.transform, new Vector2(0.5f, 1f), new Vector2(0f, -400f), new Vector2(780f, 210f));
            Sprite b = art != null ? ArtCatalog.Pick(art.Buttons, 1) : null;
            Sprite b2 = art != null ? ArtCatalog.Pick(art.Buttons, 0) : null;
            Button next = OverlayButton("Next level", b, hasNext ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f), 400f, () => { if (OnNext != null) OnNext(); });
            next.interactable = hasNext;
            OverlayButton("Play again", b2, Color.white, 240f, () => { if (OnRetry != null) OnRetry(); });
            OverlayButton("Level select", b2, new Color(0.9f, 0.95f, 1f), 80f, () => { if (OnMenu != null) OnMenu(); });
        }
    }
}
