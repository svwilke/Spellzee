using UnityEditor;
using UnityEngine;

/// <summary>
/// Custom default settings for texture imports
/// </summary>
public class RetroBlitTexturePreProcessor : AssetPostprocessor
{
    private void OnPreprocessTexture()
    {
        if (assetPath.Contains("RetroBlit-ignore"))
        {
            return;
        }

        TextureImporter textureImporter = (TextureImporter)assetImporter;
        textureImporter.convertToNormalmap = false;
        textureImporter.anisoLevel = 0;
        textureImporter.filterMode = FilterMode.Point;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
        textureImporter.spritePixelsPerUnit = 1;
        textureImporter.maxTextureSize = 8192;
        textureImporter.isReadable = false;
    }
}
