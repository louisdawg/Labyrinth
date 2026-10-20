using UnityEditor;

// Stellt die neuen Dungeon-Sprites beim Import automatisch pixelgenau ein
// (Sprite, Point, keine Kompression, 16 Pixels Per Unit).
// Ins Verzeichnis Assets/Editor legen, BEVOR die PNGs ins Projekt kommen
// (sonst Rechtsklick auf die PNGs -> Reimport).
public class DungeonSpriteImport : AssetPostprocessor
{
    private static readonly string[] prefixes =
    {
        "door_open_", "key_mouth", "key_altar", "lever_", "gate_",
        "slime_", "guard_", "dungeon_exit",
        "floor_grass_", "floor_path", "wall_tree_", "goal_signpost"   // Level 4
    };

    void OnPreprocessTexture()
    {
        string name = System.IO.Path.GetFileNameWithoutExtension(assetPath);
        foreach (string prefix in prefixes)
        {
            if (!name.StartsWith(prefix)) continue;

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = 16;
            importer.filterMode = UnityEngine.FilterMode.Point;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            return;
        }
    }
}