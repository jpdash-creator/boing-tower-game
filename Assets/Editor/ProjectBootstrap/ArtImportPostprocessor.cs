using UnityEditor;

namespace ProjectBootstrap
{
    // Applies mobile-friendly sprite import settings to everything under Assets/Art (doc 03 section 8).
    // Generated art is 2K; ASTC 6x6 on mobile keeps the install small (doc 01: base install under 150 MB).
    public class ArtImportPostprocessor : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith("Assets/Art/")) return;
            var ti = (TextureImporter)assetImporter;
            ti.textureType = TextureImporterType.Sprite;
            ti.spriteImportMode = SpriteImportMode.Single;
            ti.spritePixelsPerUnit = 256f;
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
