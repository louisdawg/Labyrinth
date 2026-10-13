using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public static class LevelBuilder
{
    // Legende (nicht jedes Level nutzt alle Zeichen):
    // #  Wand            .  Boden
    // S  Start           Z  Ziel (Objekt "Goal")      X  Ausgang zurück (Objekt "Exit")
    // D  Dornen-Falle    F  Fleisch                   H  Hinweis
    // 1-4 Symbol-Platten K  Schlüssel-Altar          T  Tür

    class LevelDef
    {
        public string[] Layout;
        public string[] FloorTiles;
        public string[] WallTiles;
        public string HintPrefab;
        public bool NumberHints;
    }

    static readonly string[] Level1Layout =
    {
        "###############################",
        "#..D.H#F....#F..#..D..#...D..Z#",
        "#.#########.###.#.###.#.#######",
        "#.....#.#.#.#.#....F#........F#",
        "###.###.#.#.#.#D#####.###.#####",
        "#.#...#.#.#.......#H#..F......#",
        "#.#.#.#.#.#.#.#####.#.#####.#.#",
        "#...#F....D.#.D..H#.........#.#",
        "#.###.#####.#.#.#.#.###.#####D#",
        "#S..D...#...#.#.#...#F...F#H..#",
        "###############################",
    };

    static readonly string[] Level2Layout =
    {
        "##########################",
        "#....#H#....##############",
        "#....#D#....###..#.FD..#.#",
        "#.1.......2.###..D..D..#.#",
        "#...#...#...###..D..D..D.#",
        "#XS...K...F.T....D..D..DZ#",
        "#...#...#...###..D..D..D.#",
        "#.3.......4.###..D..#..D.#",
        "#...........###..#..#F.D.#",
        "#.....F.....##############",
        "##########################",
    };

    static LevelDef GetDef(string sceneName)
    {
        switch (sceneName)
        {
            case "Level1":
                return new LevelDef
                {
                    Layout = Level1Layout,
                    FloorTiles = new[] { "floor_jungle_a", "floor_jungle_b", "floor_jungle_c" },
                    WallTiles = new[] { "wall_jungle_a", "wall_jungle_b" },
                    HintPrefab = "Hint",
                    NumberHints = true
                };
            case "Level2":
                return new LevelDef
                {
                    Layout = Level2Layout,
                    FloorTiles = new[] { "floor_dungeon_a", "floor_dungeon_b" },
                    WallTiles = new[] { "wall_dungeon_a", "wall_dungeon_b" },
                    HintPrefab = "FallbackHint",
                    NumberHints = false
                };
            default:
                return null;
        }
    }

    [MenuItem("Labyrinth/Level aufbauen (aktive Szene)")]
    static void Build()
    {
        string sceneName = SceneManager.GetActiveScene().name;
        LevelDef def = GetDef(sceneName);
        if (def == null)
        {
            Debug.LogError("Keine Level-Definition fuer Szene '" + sceneName + "'. Bekannt: Level1, Level2.");
            return;
        }

        Tilemap floor = FindTilemap("Floor");
        Tilemap walls = FindTilemap("Walls");
        if (floor == null || walls == null) return;

        Tile[] floorTiles = LoadTiles(def.FloorTiles, false);
        Tile[] wallTiles = LoadTiles(def.WallTiles, true);
        if (floorTiles == null || wallTiles == null) return;

        GameObject thornPrefab = FindPrefab("Thorns", true);
        if (thornPrefab == null) return;

        foreach (string row in def.Layout)
            if (row.Length != def.Layout[0].Length)
                Debug.LogWarning("Zeile hat eine andere Laenge als die erste: " + row);

        floor.ClearAllTiles();
        walls.ClearAllTiles();
        DestroyGroup("Obstacles");
        DestroyGroup("Pickups");
        DestroyGroup("Puzzle");

        Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        prefabs["Thorns"] = thornPrefab;
        int hintCount = 0;
        Random.InitState(7);

        int height = def.Layout.Length;
        for (int row = 0; row < height; row++)
        {
            for (int col = 0; col < def.Layout[row].Length; col++)
            {
                int y = height - 1 - row;
                Vector3Int pos = new Vector3Int(col, y, 0);
                Vector3 center = new Vector3(col + 0.5f, y + 0.5f, 0f);
                char c = def.Layout[row][col];

                if (c == '#')
                {
                    bool first = Random.value < 0.8f || wallTiles.Length == 1;
                    walls.SetTile(pos, first ? wallTiles[0] : wallTiles[1]);
                    continue;
                }

                floor.SetTile(pos, PickFloor(floorTiles));

                if (c == 'S') { MoveObject("Player", center); continue; }
                if (c == 'Z') { MoveObject("Goal", center); continue; }
                if (c == 'X') { MoveObject("Exit", center); continue; }

                string prefabName = PrefabNameFor(c, def);
                if (prefabName == null) continue;

                GameObject prefab = GetPrefab(prefabs, prefabName);
                if (prefab == null) continue;

                GameObject obj = Spawn(prefab, GetGroup(groups, GroupFor(c)), center);
                if (c == 'H')
                {
                    if (def.NumberHints) SetHintId(obj, hintCount);
                    hintCount++;
                }
                else if (c >= '1' && c <= '4')
                {
                    obj.name = prefabName + "_" + c;
                }
            }
        }

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(sceneName + " aufgebaut: " + def.Layout[0].Length + " x " + height + " Tiles");
    }

    static string PrefabNameFor(char c, LevelDef def)
    {
        switch (c)
        {
            case 'D': return "Thorns";
            case 'F': return "Meat";
            case 'H': return def.HintPrefab;
            case '1': case '2': case '3': case '4': return "SymbolPlate";
            case 'K': return "KeyAltar";
            case 'T': return "Door";
            default: return null;
        }
    }

    static string GroupFor(char c)
    {
        if (c == 'D') return "Obstacles";
        if (c == 'F' || c == 'H') return "Pickups";
        return "Puzzle";
    }

    static Tile PickFloor(Tile[] tiles)
    {
        float r = Random.value;
        if (r < 0.7f || tiles.Length == 1) return tiles[0];
        int i = 1 + (int)((r - 0.7f) / 0.3f * (tiles.Length - 1));
        return tiles[Mathf.Min(i, tiles.Length - 1)];
    }

    static Tile[] LoadTiles(string[] spriteNames, bool isWall)
    {
        Tile[] tiles = new Tile[spriteNames.Length];
        for (int i = 0; i < spriteNames.Length; i++)
        {
            tiles[i] = GetTile(spriteNames[i], isWall);
            if (tiles[i] == null) return null;
        }
        return tiles;
    }

    static GameObject GetPrefab(Dictionary<string, GameObject> cache, string prefabName)
    {
        GameObject prefab;
        if (cache.TryGetValue(prefabName, out prefab)) return prefab;
        prefab = FindPrefab(prefabName, false);   // warnt nur einmal pro Name
        cache[prefabName] = prefab;
        return prefab;
    }

    static Transform GetGroup(Dictionary<string, Transform> groups, string groupName)
    {
        Transform t;
        if (!groups.TryGetValue(groupName, out t))
        {
            t = new GameObject(groupName).transform;
            groups[groupName] = t;
        }
        return t;
    }

    static void DestroyGroup(string groupName)
    {
        GameObject old = GameObject.Find(groupName);
        if (old != null) Object.DestroyImmediate(old);
    }

    static GameObject Spawn(GameObject prefab, Transform parent, Vector3 position)
    {
        GameObject obj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
        obj.transform.position = position;
        return obj;
    }

    static void SetHintId(GameObject hint, int id)
    {
        HintPickup pickup = hint.GetComponent<HintPickup>();
        if (pickup == null) { Debug.LogWarning("Prefab 'Hint' hat kein HintPickup-Script."); return; }

        SerializedObject so = new SerializedObject(pickup);
        so.FindProperty("hintId").intValue = id;
        so.ApplyModifiedProperties();
    }

    static void MoveObject(string objectName, Vector3 position)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null) { Debug.LogWarning("Objekt '" + objectName + "' nicht in der Szene."); return; }
        go.transform.position = position;
    }

    static Tilemap FindTilemap(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        Tilemap map = go != null ? go.GetComponent<Tilemap>() : null;
        if (map == null) Debug.LogError("Tilemap '" + objectName + "' nicht gefunden (Name in der Hierarchy pruefen).");
        return map;
    }

    static GameObject FindPrefab(string prefabName, bool required)
    {
        foreach (string guid in AssetDatabase.FindAssets(prefabName + " t:Prefab"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == prefabName)
                return AssetDatabase.LoadAssetAtPath<GameObject>(p);
        }
        string msg = "Prefab '" + prefabName + "' nicht gefunden (Assets/Prefabs).";
        if (required) Debug.LogError(msg); else Debug.LogWarning(msg + " Wird uebersprungen.");
        return null;
    }

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