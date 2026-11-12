using UnityEditor;
using UnityEngine;

namespace SolarSystem.EditorTools
{
    public sealed class SolarTextureImportSettings : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            TextureImporter importer = (TextureImporter)assetImporter;
            if (!assetPath.Contains("/SolarSystem/Textures/")) return;

            importer.textureType = TextureImporterType.Default;
            importer.mipmapEnabled = true;
            importer.filterMode = FilterMode.Trilinear;
            importer.anisoLevel = 16;
            importer.maxTextureSize = 8192;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.compressionQuality = 100;
            importer.crunchedCompression = false;
            importer.wrapMode = TextureWrapMode.Repeat;

            if (assetPath.EndsWith("8k_earth_normal_map.tif"))
            {
                importer.textureType = TextureImporterType.NormalMap;
                importer.convertToNormalmap = false;
                importer.textureCompression = TextureImporterCompression.Compressed;
            }
            else if (assetPath.EndsWith("8k_earth_specular_map.tif"))
            {
                importer.sRGBTexture = false;
            }

            TextureImporterPlatformSettings desktop = importer.GetPlatformTextureSettings("Standalone");
            desktop.name = "Standalone";
            desktop.overridden = true;
            desktop.maxTextureSize = 8192;
            desktop.textureCompression = importer.textureCompression;
            desktop.compressionQuality = 100;
            desktop.crunchedCompression = false;
            importer.SetPlatformTextureSettings(desktop);
        }
    }
}