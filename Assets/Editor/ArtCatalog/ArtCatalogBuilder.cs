using System;
using System.IO;
using System.Linq;
using BoingTower.Gameplay;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;

namespace BoingTower.Editor
{
    /// <summary>
    /// Fills Assets/Resources/ArtCatalog.asset from the sliced art in Assets/Art (see Assets/Art/NOTES.md).
    /// Menu: Tools/Boing Tower/Build Art Catalog. Headless: -executeMethod BoingTower.Editor.ArtCatalogBuilder.BuildFromCommandLine
    /// </summary>
    public static class ArtCatalogBuilder
    {
        const string Art = "Assets/Art";
        const string AssetPath = "Assets/Resources/ArtCatalog.asset";

        [MenuItem("Tools/Boing Tower/Build Art Catalog")]
        public static void Build()
        {
            AssetDatabase.Refresh();
            var catalog = AssetDatabase.LoadAssetAtPath<ArtCatalog>(AssetPath);
            if (catalog == null)
            {
                catalog = ScriptableObject.CreateInstance<ArtCatalog>();
                Directory.CreateDirectory("Assets/Resources");
                AssetDatabase.CreateAsset(catalog, AssetPath);
            }

            var regions = new RegionArt[5];
            for (int r = 1; r <= 5; r++) regions[r - 1] = BuildRegion(r);
            catalog.Regions = regions;

            catalog.Player = One(Art + "/Characters/char_momo/char_momo/char_momo.png");
            catalog.CoinFrames = Folder(Art + "/Pickups/pck_coin_sheet");
            catalog.GemSizes = Folder(Art + "/Pickups/pck_gem_sheet");
            catalog.Orbs = Folder(Art + "/Pickups/pck_orbs");
            catalog.CoinIcon = One(Art + "/Currency/cur_coin_single/cur_coin_single.png");
            catalog.GemIcon = One(Art + "/Currency/cur_gem_single/cur_gem_single.png");
            catalog.StarFilled = One(Art + "/Currency/cur_star_filled/cur_star_filled.png");
            catalog.StarEmpty = One(Art + "/Currency/cur_star_empty/cur_star_empty.png");

            catalog.Buttons = Folder(Art + "/UI/ui_buttons_primary");
            catalog.RoundButtons = Folder(Art + "/UI/ui_buttons_icon_round");
            catalog.Panels = Folder(Art + "/UI/ui_panel_glass");
            catalog.Popup = Folder(Art + "/UI/ui_panel_popup");
            catalog.HudIcons = Folder(Art + "/UI/ui_hud_icons");
            catalog.LevelNodes = Folder(Art + "/UI/ui_level_nodes");
            catalog.MapNodes = Folder(Art + "/UI/ui_map_nodes");
            catalog.Ribbons = Folder(Art + "/UI/ui_banners_ribbons");
            catalog.RarityFrames = Folder(Art + "/UI/ui_frames_rarity");
            catalog.Toggles = Folder(Art + "/UI/ui_toggles_sliders");
            catalog.ProgressBars = Folder(Art + "/UI/ui_progress_bars");
            catalog.SettingsIcons = Folder(Art + "/UI/ui_settings_icons");
            catalog.Confetti = Folder(Art + "/VFX/vfx_confetti");
            catalog.Sparkles = Folder(Art + "/VFX/vfx_sparkles");
            catalog.DustPuff = Folder(Art + "/VFX/vfx_dust_puff");
            catalog.Trail = Folder(Art + "/Items/trail/trl_rainbow_ribbon");
            catalog.MapBackground = One(Art + "/Map/map_world_v1.png");
            catalog.LoadingArt = One(Art + "/UI/ui_loading_screen_art.png");
            catalog.AppIcon = One(Art + "/Marketing/icon/app_icon_concept_v1.png");

            EditorUtility.SetDirty(catalog);
            AssetDatabase.SaveAssets();
            Debug.Log("[ArtCatalog] Built: " + Summary(catalog));
        }

        public static void BuildFromCommandLine()
        {
            try
            {
                Build();
                EditorApplication.Exit(0);
            }
            catch (Exception e)
            {
                Debug.LogError("[ArtCatalog] Failed: " + e);
                EditorApplication.Exit(1);
            }
        }

        static RegionArt BuildRegion(int r)
        {
            string regionDir = Directory.GetDirectories(Art + "/Regions", "region_00" + r + "_*").FirstOrDefault();
            if (regionDir == null) throw new InvalidOperationException("No art folder for region " + r);
            regionDir = regionDir.Replace('\\', '/');
            var ra = new RegionArt { Index = r, Id = Path.GetFileName(regionDir) };
            ra.BgFar = First(regionDir + "/bg_far");
            ra.BgMid = First(regionDir + "/bg_mid");
            ra.BgNear = First(regionDir + "/bg_near");

            string tiles = regionDir + "/tiles/reg00" + r + "_tileset";
            Sprite[] t = Folder(tiles);
            JObject map = JObject.Parse(File.ReadAllText(tiles + "/tileset_map.json"));
            ra.Normal = Idx(t, (int)map["normal"]);
            ra.Narrow = Idx(t, (int)map["narrow"]);
            ra.Moving = Idx(t, (int)map["moving"]);
            ra.Crumble = Idx(t, (int)map["crumble"]);
            ra.Bounce = Idx(t, (int)map["bounce"]);
            ra.WallL = Idx(t, (int)map["wallL"]);
            ra.WallR = Idx(t, (int)map["wallR"]);
            ra.Props = Folder(regionDir + "/props/reg00" + r + "_props");
            ra.Signature = Folder(regionDir + "/hazards/reg00" + r + "_signature");
            return ra;
        }

        static Sprite Idx(Sprite[] list, int oneBased)
        {
            if (oneBased < 1 || oneBased > list.Length) throw new InvalidOperationException("tileset map index " + oneBased + " out of range " + list.Length);
            return list[oneBased - 1];
        }

        static Sprite[] Folder(string folder)
        {
            if (!Directory.Exists(folder)) throw new InvalidOperationException("Missing art folder " + folder);
            return Directory.GetFiles(folder, "*.png").Select(f => f.Replace('\\', '/')).OrderBy(f => f, StringComparer.Ordinal)
                .Select(f => AssetDatabase.LoadAssetAtPath<Sprite>(f)).Where(s => s != null).ToArray();
        }

        static Sprite One(string path)
        {
            var s = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (s == null) throw new InvalidOperationException("Missing sprite " + path);
            return s;
        }

        static Sprite First(string folder)
        {
            string f = Directory.GetFiles(folder, "*.png").Select(x => x.Replace('\\', '/')).OrderBy(x => x, StringComparer.Ordinal).FirstOrDefault();
            if (f == null) throw new InvalidOperationException("No png in " + folder);
            return One(f);
        }

        static string Summary(ArtCatalog c)
        {
            return c.Regions.Length + " regions, " + c.CoinFrames.Length + " coin frames, " + c.Buttons.Length + " buttons, " + c.LevelNodes.Length + " level nodes, " + c.MapNodes.Length + " map nodes";
        }
    }
}
