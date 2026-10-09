using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class Level1Builder
{
    // # = Wand, alles andere = Boden
    // S = Start, Z = Ziel, F = Fleisch, H = Hinweis, D = Dornen (Objekte kommen später)
    static readonly string[] Layout =
    {
        "###############################",
        "#......F.......HD............Z#",
        "#.#.###.#########.###########.#",
        "#H#...#H#.....#...#F....#...#.#",
        "#####.###.###.#.#####.#.#.#.#.#",
        "#.....#....F#.........#...#H#.#",
        "#.#####.#####D###########.###.#",
        "#.......#...#...........DF#...#",
        "#######D#.#.#.#########.###.#.#",
        "#S.......F#...#F............#F#",
        "###############################",
    };

    [MenuItem("Labyrinth/Level1 aufbauen")]
    static void Build()
    {
        Tilemap floor = FindTilemap("Floor");
        Tilemap walls = FindTilemap("Walls");
        if (floor == null || walls == null) return;

        Tile[] floorTiles = { GetTile("floor_jungle_a", false), GetTile("floor_jungle_b", false), GetTile("floor_jungle_c", false) };
        Tile[] wallTiles = { GetTile("wall_jungle_a", true), GetTile("wall_jungle_b", true) };
        foreach (Tile t in floorTiles) if (t == null) return;
        foreach (Tile t in wallTiles) if (t == null) return;

        floor.ClearAllTiles();
        walls.ClearAllTiles();
        Random.InitState(7);

        int height = Layout.Length;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < Layout[row].Length; col++)
            {
                Vector3Int pos = new Vector3Int(col, height - 1 - row, 0);

                if (Layout[row][col] == '#')
                {
                    walls.SetTile(pos, Random.value < 0.8f ? wallTiles[0] : wallTiles[1]);
                }
                else
                {
                    float r = Random.value;
                    floor.SetTile(pos, r < 0.7f ? floorTiles[0] : (r < 0.85f ? floorTiles[1] : floorTiles[2]));
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log("Level1 aufgebaut: " + Layout[0].Length + " x " + height + " Tiles");
    }

    static Tilemap FindTilemap(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        Tilemap map = go != null ? go.GetComponent<Tilemap>() : null;
        if (map == null) Debug.LogError("Tilemap '" + objectName + "' nicht gefunden (Name in der Hierarchy pruefen).");
        return map;
    }

    // Erstellt (einmalig) ein Tile-Asset in Assets/Tiles und gibt es zurück
    static Tile GetTile(string spriteName, bool isWall)
    {
        string path = "Assets/Tiles/" + spriteName + ".asset";
        Tile tile = AssetDatabase.LoadAssetAtPath<Tile>(path);
        if (tile != null) return tile;

        Sprite sprite = FindSprite(spriteName);
        if (sprite == null)
        {
            Debug.LogError("Sprite '" + spriteName + "' nicht gefunden.");
            return null;
        }

        if (!AssetDatabase.IsValidFolder("Assets/Tiles")) AssetDatabase.CreateFolder("Assets", "Tiles");

        tile = ScriptableObject.CreateInstance<Tile>();
        tile.sprite = sprite;
        tile.colliderType = isWall ? Tile.ColliderType.Grid : Tile.ColliderType.None;
        AssetDatabase.CreateAsset(tile, path);
        return tile;
    }

    static Sprite FindSprite(string spriteName)
    {
        foreach (string guid in AssetDatabase.FindAssets(spriteName + " t:Sprite"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == spriteName)
                return AssetDatabase.LoadAssetAtPath<Sprite>(p);
        }
        return null;
    }
}