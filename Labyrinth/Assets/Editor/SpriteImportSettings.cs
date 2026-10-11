using UnityEditor;
using UnityEngine;

public class SpriteImportSettings : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        bool isUi = name.StartsWith("ui_");
        bool isWorld = name == "meat_mouth" || name == "hint_scroll";
        if (!isUi && !isWorld) return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.filterMode = FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
        importer.spritePixelsPerUnit = isUi ? 25 : 16;
        
        if (name == "ui_panel")
        {
            importer.spriteBorder = new Vector4(8, 8, 8, 8);
        }
    }
}