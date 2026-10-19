using UnityEngine;

namespace BoingTower.Gameplay
{
    /// <summary>Player preferences (doc 01 FR-X3). Stored in PlayerPrefs; audio and haptics read these when they exist.</summary>
    public static class GameSettings
    {
        const string SoundKey = "settings.sound";
        const string MusicKey = "settings.music";
        const string HapticsKey = "settings.haptics";
        const string ReducedKey = "settings.reducedmotion";
        const string LeftKey = "settings.lefthanded";
        const string TextKey = "settings.textpercent";

        public static readonly int[] TextPercents = { 100, 115, 130 };

        public static bool Sound { get { return PlayerPrefs.GetInt(SoundKey, 1) == 1; } set { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        public static bool Music { get { return PlayerPrefs.GetInt(MusicKey, 1) == 1; } set { PlayerPrefs.SetInt(MusicKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        public static bool Haptics { get { return PlayerPrefs.GetInt(HapticsKey, 1) == 1; } set { PlayerPrefs.SetInt(HapticsKey, value ? 1 : 0); PlayerPrefs.Save(); } }

        /// <summary>No screen shake, squash, pulses or parallax scrolling (CUX-6).</summary>
        public static bool ReducedMotion { get { return PlayerPrefs.GetInt(ReducedKey, 0) == 1; } set { PlayerPrefs.SetInt(ReducedKey, value ? 1 : 0); PlayerPrefs.Save(); } }

        /// <summary>Mirrors the run HUD so the pause button and wallet sit under the left thumb (CUX-4).</summary>
        public static bool LeftHanded { get { return PlayerPrefs.GetInt(LeftKey, 0) == 1; } set { PlayerPrefs.SetInt(LeftKey, value ? 1 : 0); PlayerPrefs.Save(); } }

        /// <summary>Accessibility text size: 100, 115 or 130 percent. Applied to every text built afterwards.</summary>
        public static int TextPercent
        {
            get { return PlayerPrefs.GetInt(TextKey, 100); }
            set { PlayerPrefs.SetInt(TextKey, value); PlayerPrefs.Save(); ApplyTextScale(); }
        }

        public static void ApplyTextScale() { UiKit.TextScale = Mathf.Clamp(TextPercent, 100, 130) / 100f; }

        public static void NextTextPercent()
        {
            int i = System.Array.IndexOf(TextPercents, TextPercent);
            TextPercent = TextPercents[(i + 1) % TextPercents.Length];
        }
    }
}
