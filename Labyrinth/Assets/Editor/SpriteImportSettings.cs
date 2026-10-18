using UnityEditor;

// Stellt die selbst gezeichneten UI-Sprites beim Import automatisch pixelgenau ein
// (Sprite, Point, keine Kompression). ui_* hat 25 Pixels Per Unit, meat_mouth und hint_scroll 16.
// Die Dungeon-Sprites (door_open_, lever_, gate_, slime_ ...) übernimmt DungeonSpriteImport.cs.
// Ins Verzeichnis Assets/Editor legen (ersetzt die alte Datei mit demselben Namen).
public class UiSpriteImport : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);

        int pixelsPerUnit;
        if (name.StartsWith("ui_")) pixelsPerUnit = 25;
        else if (name == "meat_mouth" || name == "hint_scroll") pixelsPerUnit = 16;
        else return;

        TextureImporter importer = (TextureImporter)assetImporter;
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = pixelsPerUnit;
        importer.filterMode = UnityEngine.FilterMode.Point;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.mipmapEnabled = false;
    }
}