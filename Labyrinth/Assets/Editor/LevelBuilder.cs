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
    // P  Respawn hinter der Tür (Objekt "DungeonSpawn", wird automatisch angelegt und verknüpft)
    // V  Hebel (Prefab "Lever")   G  Tor (Prefab "Gate")   M  Slime (Prefab "Slime")   W  Wärter (Prefab "Guard")
    // L  Wand mit Fackel (Wandtile + Prefab "Torch")
    // >  Dornen-Welle nach Osten, <  Dornen-Welle nach Westen (Prefab "Thorns", Phase nach Spalte, siehe WaveStep)
    // Fehlende Prefabs werden einmal gewarnt und übersprungen.

    // Dornen-Wellen. Geschwindigkeit der Welle = 1 / WaveStep Tiles pro Sekunde (0.4 = 2.5 Tiles/s = Spielertempo).
    // Kürzere sichere Zeit (WaveInactive) und längere aktive Zeit (WaveActive) machen die Bahnen schwerer.
    const float WaveStep = 0.4f;
    const float WaveActive = 1.9f;
    const float WaveInactive = 1.5f;

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

    // Level 2: Raetselraum (links), Hub in der Mitte, Nord- und Sued-Raum (Wellen-Schleifen),
    // Nordost- und Suedost-Fluegel (je 3 Kammern), Tor, Endraum (rechts)
    static readonly string[] Level2Layout =
    {
        "###################################################",
        "######################################L######L#####",
        "########################V#############V##.........#",
        "################.<<<<<<<.####.<<<<<<<<.##.........#",
        "################.#L####L.####.##L##L##......L.#...#",
        "################.>>>>>>>.####.>>>>>>>>.##.........L",
        "################.############.########L##...M.....#",
        "################..F...F..####..F....F..##F.F...F.F#",
        "####################.###########.###########L######",
        "####################.########L##.##################",
        "################L##L.L##L####...M..################",
        "########L######...........###.#..L.########L###L###",
        "L....#H#....###.F...F...F.###..M...######.........#",
        "#....LD#...F###...............#..#.######.F.....F.#",
        "#.1.......2.###...#...L...###.F..F.######...L.....#",
        "#...#...L...L#L...........#L#####L######L.........L",
        "#XS...K...F.TP.............G......F...........W..Z#",
        "#...L...#...L#L...........#L############L.........#",
        "L.3.......4.###...L...L...###......######...L.....#",
        "#...........###...............#..#.######.F.....F.#",
        "#..F..F.....###.F...F...F.L##..D..D######.........#",
        "#######L#######...........###.#..#.########L###L###",
        "###############L###L.L#######F....F################",
        "####################.###########.##################",
        "####################.###########.###########L##L###",
        "################..F...F..####..F....F..##.........#",
        "#######################L.#######L#####.##.D.....D.#",
        "################.<<<<<<<.###L.<<<<<<<<.##...L.#...#",
        "################.#L#####.####.#######L........D...L",
        "################.>>>>>>>.####.>>>>>>>>.##.F.....F.#",
        "################V############V###########...F.F...#",
        "###########################################L##L####",
        "###################################################",
    };

    // Ordnung in der Hierarchy: erster Eintrag = Gruppe, danach ihre Kinder (nach Namen gesucht).
    // Fehlende Objekte werden uebersprungen, leere Gruppen nicht angelegt.
    static readonly string[][] HierarchyPlan =
    {
        new[] { "Environment", "Grid" },
        new[] { "Gameplay", "Obstacles", "Pickups", "Puzzle", "Mechanics", "Enemies", "NPCs", "Torches" },
        new[] { "Actors", "Player", "Goal", "Exit", "PlayerSpawn", "DungeonSpawn" },
        new[] { "Systems", "Main Camera", "WorldItems", "LevelLighting", "Global Light 2D" },
        new[] { "UI", "Canvas", "EventSystem" },
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
        DestroyGroup("Mechanics");
        DestroyGroup("Enemies");
        DestroyGroup("NPCs");
        DestroyGroup("Torches");

        Dictionary<string, Transform> groups = new Dictionary<string, Transform>();
        Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();
        prefabs["Thorns"] = thornPrefab;
        int hintCount = 0;
        int leverCount = 0;
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

                if (c == '#' || c == 'L')
                {
                    bool first = Random.value < 0.8f || wallTiles.Length == 1;
                    walls.SetTile(pos, first ? wallTiles[0] : wallTiles[1]);
                    if (c == 'L') SpawnWallTorch(prefabs, groups, center);
                    continue;
                }

                floor.SetTile(pos, PickFloor(floorTiles));

                if (c == 'S') { MoveObject("Player", center); continue; }
                if (c == 'Z') { MoveObject("Goal", center); continue; }
                if (c == 'X') { MoveObject("Exit", center); continue; }
                if (c == 'P') { PlaceDungeonSpawn(center); continue; }

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
                    SetupPlate(obj, c);
                }
                else if (c == '>' || c == '<')
                {
                    SetWaveOffset(obj, col, c == '>');
                }
                else if (c == 'V')
                {
                    SetLeverId(obj, leverCount++);
                }
            }
        }

        OrganizeHierarchy();

        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        Debug.Log(sceneName + " aufgebaut: " + def.Layout[0].Length + " x " + height + " Tiles");
    }

    [MenuItem("Labyrinth/Hierarchie ordnen (aktive Szene)")]
    static void OrganizeHierarchyMenu()
    {
        OrganizeHierarchy();
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void OrganizeHierarchy()
    {
        int rootIndex = 0;
        foreach (string[] entry in HierarchyPlan)
        {
            Transform group = null;
            int order = 0;

            for (int i = 1; i < entry.Length; i++)
            {
                GameObject go = GameObject.Find(entry[i]);
                if (go == null) continue;

                if (group == null) group = GetOrCreateRoot(entry[0]);
                go.transform.SetParent(group, true);   // true = Weltposition bleibt gleich
                go.transform.SetSiblingIndex(order++);
            }

            if (group != null) group.SetSiblingIndex(rootIndex++);
        }
    }

    static Transform GetOrCreateRoot(string groupName)
    {
        GameObject go = GameObject.Find(groupName);
        if (go == null) go = new GameObject(groupName);
        return go.transform;
    }

    static void SetupPlate(GameObject obj, char c)
    {
        SymbolPlate plate = obj.GetComponent<SymbolPlate>();
        if (plate == null) { Debug.LogWarning("Prefab 'SymbolPlate' hat kein SymbolPlate-Script."); return; }

        plate.Setup((Symbol)(c - '1'));   // '1' Sonne, '2' Mond, '3' Stern, '4' Auge
        PrefabUtility.RecordPrefabInstancePropertyModifications(plate);

        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>();
        if (sr != null) PrefabUtility.RecordPrefabInstancePropertyModifications(sr);
    }

    static void PlaceDungeonSpawn(Vector3 position)
    {
        GameObject spawn = GameObject.Find("DungeonSpawn");
        if (spawn == null) spawn = new GameObject("DungeonSpawn");
        spawn.transform.position = position;

        PlayerSpawn playerSpawn = Object.FindFirstObjectByType<PlayerSpawn>();
        if (playerSpawn == null)
        {
            Debug.LogWarning("Kein PlayerSpawn in der Szene, DungeonSpawn ist nicht verknuepft.");
            return;
        }

        SerializedObject so = new SerializedObject(playerSpawn);
        SerializedProperty prop = so.FindProperty("dungeonSpawn");
        if (prop == null) { Debug.LogWarning("PlayerSpawn hat kein Feld 'dungeonSpawn' (neues PlayerSpawn.cs eingespielt?)."); return; }
        prop.objectReferenceValue = spawn.transform;
        so.ApplyModifiedProperties();
    }

    static void SetLeverId(GameObject lever, int id)
    {
        Lever script = lever.GetComponent<Lever>();
        if (script == null) { Debug.LogWarning("Prefab 'Lever' hat kein Lever-Script."); return; }

        SerializedObject so = new SerializedObject(script);
        so.FindProperty("leverId").intValue = id;
        so.ApplyModifiedProperties();
    }

    // Dornen einer Bahn bekommen je nach Spalte eine Phase, dadurch läuft eine Welle durch die Bahn
    static void SetWaveOffset(GameObject thorn, int column, bool eastward)
    {
        TimedThorns thorns = thorn.GetComponent<TimedThorns>();
        if (thorns == null) { Debug.LogWarning("Prefab 'Thorns' hat kein TimedThorns-Script."); return; }

        SerializedObject so = new SerializedObject(thorns);
        so.FindProperty("activeTime").floatValue = WaveActive;
        so.FindProperty("inactiveTime").floatValue = WaveInactive;
        float period = WaveActive + WaveInactive;
        float offset = (eastward ? -column : column) * WaveStep;
        offset = ((offset % period) + period) % period;
        so.FindProperty("timeOffset").floatValue = offset;
        so.ApplyModifiedProperties();
    }

    static void SpawnWallTorch(Dictionary<string, GameObject> prefabs, Dictionary<string, Transform> groups, Vector3 center)
    {
        GameObject prefab = GetPrefab(prefabs, "Torch");
        if (prefab == null) return;
        Spawn(prefab, GetGroup(groups, "Torches"), center);
    }

    static string PrefabNameFor(char c, LevelDef def)
    {
        switch (c)
        {
            case 'D': case '>': case '<': return "Thorns";
            case 'F': return "Meat";
            case 'H': return def.HintPrefab;
            case '1': case '2': case '3': case '4': return "SymbolPlate";
            case 'K': return "KeyAltar";
            case 'T': return "Door";
            case 'V': return "Lever";
            case 'G': return "Gate";
            case 'M': return "Slime";
            case 'W': return "Guard";
            default: return null;
        }
    }

    static string GroupFor(char c)
    {
        if (c == 'D' || c == '>' || c == '<') return "Obstacles";
        if (c == 'F' || c == 'H') return "Pickups";
        if (c == 'V' || c == 'G') return "Mechanics";
        if (c == 'M') return "Enemies";
        if (c == 'W') return "NPCs";
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