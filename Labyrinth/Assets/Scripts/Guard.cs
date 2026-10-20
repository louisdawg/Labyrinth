using System.Collections;
using UnityEngine;

// Wächter vor dem großen Tor (Level 3). E startet das Gespräch.
//
// Mindestens ein Slime gefressen: kein Auswahlmenü, der Wächter sagt, dass du kein Freund des Dungeons bist -> LoseScreen.
// Kein Slime gefressen: Monolog, dann die Frage "Bist du ein Freund des Dungeons?"
//   Ja   -> das große Tor öffnet sich (Cutscene), weiter zu Level 4
//   Nein -> LoseScreen
//
// Prefab: SpriteRenderer (guard_0), Box Collider 2D (KEIN Trigger, ca. 0.8 x 0.8), dieses Script.
// Der Wächter steht neben dem Weg zum Tor, er blockiert den Gang nicht.
public class Guard : MonoBehaviour, IInteractable
{
    // ---------- Texte (Platzhalter, hier ändern) ----------
    private static readonly string[] slimeLines =
    {
        "Halt.",
        "Du riechst nach Schleim.",
        "Du hast die Kinder des Dungeons gefressen.",
        "Du bist kein Freund des Dungeons."
    };

    private static readonly string[] monologueLines =
    {
        "Halt, Kreatur.",
        "Lange hat niemand diesen Weg bis zu mir geschafft.",
        "Der Dungeon ist grausam. Aber er ist auch ein Zuhause, für mich und für die Schleime.",
        "Du hast keinen von ihnen gefressen. Das habe ich bemerkt."
    };

    private const string Question = "Bist du ein Freund des Dungeons?";

    private static readonly string[] yesLines =
    {
        "Dann geh in Frieden.",
        "Das Tor ist offen."
    };

    private static readonly string[] noLines =
    {
        "Dann bist du ein Feind.",
        "Aus diesem Dungeon kommst du nicht hinaus."
    };

    // ---------- Einstellungen ----------
    [Header("Aussehen (optional)")]
    [SerializeField] private Sprite[] idleFrames;      // guard_0, guard_1
    [SerializeField] private float frameTime = 0.6f;

    [Header("Cutscene")]
    [SerializeField] private float cameraSmoothTime = 0.4f;
    [SerializeField] private float cameraWait = 1f;
    [SerializeField] private float openedSeconds = 1f;

    private SpriteRenderer sr;
    private BigGate gate;
    private CameraFollow cam;
    private Transform player;
    private bool busy;

    public Vector3 PromptPosition => transform.position;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        gate = FindFirstObjectByType<BigGate>();
        cam = FindFirstObjectByType<CameraFollow>();
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
    }

    void Update()
    {
        if (idleFrames == null || idleFrames.Length == 0) return;
        sr.sprite = idleFrames[(int)(Time.time / frameTime) % idleFrames.Length];
    }

    // ---------- IInteractable ----------
    public bool CanInteract(GameManager gm)
    {
        return !busy && !gm.BigGateOpened;
    }

    public string GetPrompt(GameManager gm)
    {
        return CanInteract(gm) ? "[E]" : null;
    }

    public void Interact(GameManager gm)
    {
        StartCoroutine(Talk());
    }

    // ---------- Gespräch ----------
    private IEnumerator Talk()
    {
        busy = true;
        GameManager gm = GameManager.Instance;
        gm.SetState(GameState.Cutscene);
        InteractPrompt.Hide();
        yield return null;   // der E-Druck, der das Gespräch gestartet hat, zählt nicht als "weiter"

        // Szenario 1: Slime gefressen, keine Auswahl
        if (gm.SlimesEaten > 0)
        {
            foreach (string line in slimeLines) yield return StartCoroutine(DialogueBox.Say(line));
            yield return StartCoroutine(Lose());
            yield break;
        }

        // Szenario 2: Monolog, dann Frage mit Auswahl
        foreach (string line in monologueLines) yield return StartCoroutine(DialogueBox.Say(line));

        int choice = 0;
        yield return StartCoroutine(DialogueBox.Ask(Question, new[] { "Ja", "Nein" }, i => choice = i));

        if (choice == 0)
        {
            foreach (string line in yesLines) yield return StartCoroutine(DialogueBox.Say(line));
            DialogueBox.Close();
            yield return StartCoroutine(OpenGate());
        }
        else
        {
            foreach (string line in noLines) yield return StartCoroutine(DialogueBox.Say(line));
            yield return StartCoroutine(Lose());
        }
    }

    private IEnumerator OpenGate()
    {
        GameManager gm = GameManager.Instance;

        if (gate != null)
        {
            if (cam != null) cam.FocusOn(gate.transform, cameraSmoothTime);
            yield return new WaitForSeconds(cameraWait);
            yield return StartCoroutine(gate.Open());
            gm.BigGateOpened = true;
            yield return new WaitForSeconds(openedSeconds);
        }
        else
        {
            Debug.LogWarning("Guard: Kein BigGate in der Szene.");
            gm.BigGateOpened = true;
        }

        if (cam != null) cam.FollowPlayer();
        yield return new WaitForSeconds(cameraWait * 0.6f);

        gm.SetState(GameState.Playing);
        busy = false;
    }

    private IEnumerator Lose()
    {
        DialogueBox.Close();

        // Der Spieler blinkt kurz rot, dann wird ausgeblendet
        if (player != null)
        {
            SpriteRenderer body = player.GetComponent<SpriteRenderer>();
            if (body != null) body.color = new Color(1f, 0.3f, 0.3f, 1f);
        }
        yield return new WaitForSeconds(0.7f);

        // Der Zustand bleibt Cutscene, der LoseScreen (MenuController) setzt ihn auf Menu
        GameSceneManager.Instance.LoadLoseScreen();
    }
}