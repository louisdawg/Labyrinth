using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { Menu, Playing, Paused, GameOver, Cutscene }

public class ItemRecord
{
    public Item item;
    public Vector2 position;
}

public class SceneItemState
{
    public bool initialized;
    public List<ItemRecord> items = new List<ItemRecord>();
}

public class GameManager : MonoBehaviour
{
    public const int TotalHints = 4;
    public const int TotalLevers = 4;

    private static GameManager instance;
    public static bool HasInstance => instance != null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDrainPerSecond = 1f;
    [SerializeField] private float meatRestore = 20f;
    [SerializeField] private float starvedSpeedFactor = 1f;

    private static readonly string[] hintTexts =
    {
        "Der Mond erwacht vor dem Stern.",
        "Der Stern verblasst, bevor die Sonne aufgeht.",
        "Das Auge öffnet sich zuletzt.",
        "Die Sonne kommt direkt vor dem Auge."
    };

    private readonly HashSet<int> foundHints = new HashSet<int>();
    private readonly HashSet<int> pulledLevers = new HashSet<int>();
    private readonly Dictionary<string, SceneItemState> sceneStates = new Dictionary<string, SceneItemState>();
    private float hunger;

    public GameState State { get; private set; } = GameState.Playing;
    public Item HeldItem { get; set; } = Item.None;

    // Tasten-Legende im HUD: Eintrag verschwindet, sobald die Taste einmal benutzt wurde
    public bool UsedInteract { get; set; }
    public bool UsedEat { get; set; }
    public bool UsedNotes { get; set; }

    // Level 2: Zustand bleibt auch nach einem Tod erhalten
    public bool PuzzleSolved { get; set; }
    public bool DoorOpened { get; set; }

    public float Hunger => hunger;
    public float MaxHunger => maxHunger;
    public float SpeedMultiplier => hunger <= 0f ? starvedSpeedFactor : 1f;
    public int FoundHintCount => foundHints.Count;
    public int LastHintId { get; private set; } = -1;

    // Hebel (Level 2): Zustand bleibt nach einem Tod erhalten
    public int PulledLeverCount => pulledLevers.Count;
    public bool AllLeversPulled => pulledLevers.Count >= TotalLevers;

    // Level 2: Die Tor-Cutscene wurde gesehen, das Tor bleibt offen (auch nach einem Tod)
    public bool GateOpened { get; set; }
    public bool IsLeverPulled(int id) { return pulledLevers.Contains(id); }
    public void PullLever(int id) { pulledLevers.Add(id); }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
        hunger = maxHunger;
    }

    void Update()
    {
        if (State == GameState.Playing)
        {
            hunger = Mathf.Max(0f, hunger - hungerDrainPerSecond * Time.deltaTime);
        }
    }

    public void SetState(GameState newState)
    {
        State = newState;
        Time.timeScale = newState == GameState.Paused ? 0f : 1f;
    }

    public bool HasHint(int id)
    {
        return foundHints.Contains(id);
    }

    public void AddHint(int id)
    {
        if (foundHints.Add(id))
        {
            LastHintId = id;
        }
    }

    public string GetHintText(int id)
    {
        return id >= 0 && id < hintTexts.Length ? hintTexts[id] : "?";
    }

    public string GetNotesText()
    {
        if (foundHints.Count == 0) return "Noch keine Hinweise gefunden.";
        return string.Join("\n", foundHints.OrderBy(i => i).Select(i => "- " + GetHintText(i)));
    }

    public void Eat()
    {
        hunger = Mathf.Min(maxHunger, hunger + meatRestore);
    }

    public void LoseHunger(float amount)
    {
        hunger = Mathf.Max(0f, hunger - amount);
    }

    public SceneItemState GetSceneState(string sceneName)
    {
        if (!sceneStates.TryGetValue(sceneName, out SceneItemState state))
        {
            state = new SceneItemState();
            sceneStates[sceneName] = state;
        }
        return state;
    }

    // ---------- Tod ----------
    public void OnPlayerDied(Vector2 position)
    {
        if (HeldItem != Item.None)
        {
            GetSceneState(SceneManager.GetActiveScene().name).items.Add(new ItemRecord { item = HeldItem, position = position });
            HeldItem = Item.None;
        }
    }

    public void ResetGame()
    {
        foundHints.Clear();
        pulledLevers.Clear();
        sceneStates.Clear();
        LastHintId = -1;
        HeldItem = Item.None;
        UsedInteract = false;
        UsedEat = false;
        UsedNotes = false;
        PuzzleSolved = false;
        DoorOpened = false;
        GateOpened = false;
        hunger = maxHunger;
        SetState(GameState.Playing);
    }
}