using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum GameState { Menu, Playing, Paused, GameOver }

public class GameManager : MonoBehaviour
{
    public const int TotalHints = 4;

    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            if (instance is null)
            {
                instance = new GameObject("GameManager").AddComponent<GameManager>();
            }
            return instance;
        }
    }

    [Header("Hunger")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float hungerDrainPerSecond = 0.5f;
    [SerializeField] private float meatRestore = 35f;
    [SerializeField] private float starvedSpeedFactor = 0.6f;
    
    private static readonly string[] hintTexts =
    {
        "Der Mond erwacht vor dem Stern.",
        "Der Stern verblasst, bevor die Sonne aufgeht.",
        "Das Auge oeffnet sich zuletzt.",
        "Die Sonne kommt direkt vor dem Auge."
    };

    private readonly HashSet<int> foundHints = new HashSet<int>();
    private float hunger;

    public GameState State { get; private set; } = GameState.Playing;
    public Item HeldItem { get; set; } = Item.None;

    public float Hunger => hunger;
    public float MaxHunger => maxHunger;
    public float SpeedMultiplier => hunger <= 0f ? starvedSpeedFactor : 1f;
    public int FoundHintCount => foundHints.Count;
    public int LastHintId { get; private set; } = -1;

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
    
    public void OnPlayerDied()
    {
        HeldItem = Item.None;
    }
}