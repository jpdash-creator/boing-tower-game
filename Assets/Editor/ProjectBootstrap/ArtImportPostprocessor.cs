using System.IO;
using UnityEditor;
using UnityEngine;

namespace ProjectBootstrap
{
    // Applies mobile-friendly sprite import settings to everything under Assets/Art (doc 03 section 8).
    // Generated art is 2K; ASTC 6x6 on mobile keeps the install small (doc 01: base install under 150 MB).
    public class ArtImportPostprocessor : AssetPostprocessor
    {
        // Thin platform sheets (<id>_platforms): pixels per unit and the 9-slice side borders come from platform_set.json, written by
        // Tools/art_pipeline/platform_metrics.py, so every platform piece has the same world thickness and caps that never stretch.
        void ApplyPlatformSet(TextureImporter ti)
        {
            if (!assetPath.Contains("_platforms/")) return;
            string json = Path.Combine(Path.GetDirectoryName(assetPath), "platform_set.json");
            if (!File.Exists(json)) return;
            var root = Newtonsoft.Json.Linq.JObject.Parse(File.ReadAllText(json));
            string name = Path.GetFileName(assetPath);
            foreach (var item in (Newtonsoft.Json.Linq.JArray)root["pieces"])
            {
                if ((string)item["file"] != name) continue;
                ti.spritePixelsPerUnit = (float)item["ppu"];
                ti.spriteBorder = new Vector4((float)item["borderL"], 0f, (float)item["borderR"], 0f);
                var settings = new TextureImporterSettings();
                ti.ReadTextureSettings(settings);
                settings.spriteMeshType = SpriteMeshType.FullRect;
                ti.SetTextureSettings(settings);
            }
        }

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 256f;
            ApplyPlatformSet(ti);
            ti.mipmapEnabled = false;
            ti.filterMode = UnityEngine.FilterMode.Bilinear;
            ti.alphaIsTransparency = true;
            ti.maxTextureSize = 2048;
            ti.textureCompression = TextureImporterCompression.Compressed;
            foreach (var platform in new[] { "Android", "iPhone" })
            {
                var ps = ti.GetPlatformTextureSettings(platform);
                ps.overridden = true;
                ps.maxTextureSize = 2048;
                ps.format = TextureImporterFormat.ASTC_6x6;
                ti.SetPlatformTextureSettings(ps);
            }
        }
    }
}
