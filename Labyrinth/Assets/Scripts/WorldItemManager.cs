using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class WorldItemManager : MonoBehaviour
{
    public static WorldItemManager Instance { get; private set; }

    [Header("Prefabs")]
    [SerializeField] private GameObject meatPrefab;
    [SerializeField] private GameObject keyPrefab;

    [Header("Zufaelliges Fleisch")]
    [SerializeField] private Tilemap floorMap;
    [SerializeField] private Tilemap wallMap;
    [SerializeField] private float spawnInterval = 40f;
    [SerializeField] private int maxMeat = 8;
    [SerializeField] private float minDistanceToPlayer = 5f;
    [SerializeField] private float minDistanceToObjects = 0.9f;

    private SceneItemState state;
    private readonly List<Vector2> freeCells = new List<Vector2>();
    private readonly List<Vector2> blockedPositions = new List<Vector2>();
    private Transform player;
    private float spawnTimer;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        state = GameManager.Instance.GetSceneState(SceneManager.GetActiveScene().name);

        ItemPickup[] placed = FindObjectsByType<ItemPickup>(FindObjectsSortMode.None);

        if (!state.initialized)
        {
            foreach (ItemPickup p in placed)
            {
                state.items.Add(new ItemRecord { item = p.item, position = p.transform.position });
            }
            state.initialized = true;
        }

        foreach (ItemPickup p in placed)
        {
            Destroy(p.gameObject);
        }
        foreach (ItemRecord record in state.items)
        {
            SpawnObject(record);
        }

        CollectFreeCells();
        CollectBlockedPositions();

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;

        spawnTimer = spawnInterval;
    }

    void Update()
    {
        if (GameManager.Instance.State != GameState.Playing) return;

        spawnTimer -= Time.deltaTime;
        if (spawnTimer > 0f) return;

        spawnTimer = spawnInterval;
        TrySpawnRandomMeat();
    }

    public void DropItem(Item item, Vector2 position)
    {
        ItemRecord record = new ItemRecord { item = item, position = position };
        state.items.Add(record);
        SpawnObject(record);
    }

    public void RemoveItem(ItemPickup pickup)
    {
        if (pickup.record != null) state.items.Remove(pickup.record);
        Destroy(pickup.gameObject);
    }

    private void SpawnObject(ItemRecord record)
    {
        GameObject prefab = GetPrefab(record.item);
        if (prefab == null)
        {
            Debug.LogWarning("Kein Prefab fuer " + record.item + " im WorldItemManager.");
            return;
        }

        GameObject obj = Instantiate(prefab, record.position, Quaternion.identity, transform);
        ItemPickup pickup = obj.GetComponent<ItemPickup>();
        if (pickup != null)
        {
            pickup.item = record.item;
            pickup.record = record;
        }
    }

    private void TrySpawnRandomMeat()
    {
        if (meatPrefab == null || freeCells.Count == 0) return;

        int meatCount = 0;
        foreach (ItemRecord r in state.items)
        {
            if (r.item == Item.Fleisch) meatCount++;
        }
        if (meatCount >= maxMeat) return;

        for (int attempt = 0; attempt < 30; attempt++)
        {
            Vector2 pos = freeCells[Random.Range(0, freeCells.Count)];
            if (IsGoodSpawnPosition(pos))
            {
                DropItem(Item.Fleisch, pos);
                return;
            }
        }
    }

    private bool IsGoodSpawnPosition(Vector2 pos)
    {
        if (player != null && Vector2.Distance(pos, player.position) < minDistanceToPlayer) return false;

        foreach (Vector2 blocked in blockedPositions)
        {
            if (Vector2.Distance(pos, blocked) < minDistanceToObjects) return false;
        }

        foreach (ItemRecord r in state.items)
        {
            if (Vector2.Distance(pos, r.position) < minDistanceToObjects) return false;
        }
        return true;
    }

    private void CollectFreeCells()
    {
        if (floorMap == null || wallMap == null)
        {
            Debug.LogWarning("WorldItemManager: Floor Map / Wall Map fehlen, zufaelliges Fleisch ist aus.");
            return;
        }

        foreach (Vector3Int cell in floorMap.cellBounds.allPositionsWithin)
        {
            if (floorMap.HasTile(cell) && !wallMap.HasTile(cell))
            {
                freeCells.Add(floorMap.GetCellCenterWorld(cell));
            }
        }
    }

    private void CollectBlockedPositions()
    {
        foreach (Obstacle o in FindObjectsByType<Obstacle>(FindObjectsSortMode.None))
        {
            blockedPositions.Add(o.transform.position);
        }
        foreach (HintPickup h in FindObjectsByType<HintPickup>(FindObjectsSortMode.None))
        {
            blockedPositions.Add(h.transform.position);
        }
        GameObject goal = GameObject.Find("Goal");
        if (goal != null) blockedPositions.Add(goal.transform.position);
    }

    private GameObject GetPrefab(Item item)
    {
        switch (item)
        {
            case Item.Fleisch: return meatPrefab;
            case Item.Schluessel: return keyPrefab;
            default: return null;
        }
    }
}