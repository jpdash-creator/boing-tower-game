using UnityEngine;

namespace BoingTower.Gameplay
{
    /// <summary>Player preferences (doc 01 FR-X3). Stored in PlayerPrefs; audio and haptics read these when they exist.</summary>
    public static class GameSettings
    {
        const string SoundKey = "settings.sound";
        const string MusicKey = "settings.music";
        const string HapticsKey = "settings.haptics";

        public static bool Sound { get { return PlayerPrefs.GetInt(SoundKey, 1) == 1; } set { PlayerPrefs.SetInt(SoundKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        public static bool Music { get { return PlayerPrefs.GetInt(MusicKey, 1) == 1; } set { PlayerPrefs.SetInt(MusicKey, value ? 1 : 0); PlayerPrefs.Save(); } }
        public static bool Haptics { get { return PlayerPrefs.GetInt(HapticsKey, 1) == 1; } set { PlayerPrefs.SetInt(HapticsKey, value ? 1 : 0); PlayerPrefs.Save(); } }
    }
}
