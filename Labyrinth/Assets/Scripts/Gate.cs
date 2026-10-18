using System.Collections;
using UnityEngine;

// Tor zum Endraum. Die vier Lichter leuchten pro Hebel nach und nach auf.
// Sind alle Hebel umgelegt, bleibt das Tor zu. Kommt der Spieler danach in die Nähe,
// läuft eine kurze Cutscene: Kamera schwenkt zum Tor, das Tor wackelt und öffnet sich.
// Danach bleibt es offen (GameManager.GateOpened, auch nach einem Tod).
// Collider darf KEIN Trigger sein.
public class Gate : MonoBehaviour
{
    [SerializeField] private Sprite[] lightSprites;   // gate_0 .. gate_4 (Größe 5)
    [SerializeField] private Sprite openSprite;       // gate_open

    [Header("Cutscene")]
    [SerializeField] private float triggerDistance = 2.5f;  // so nah muss der Spieler sein
    [SerializeField] private float cameraSmoothTime = 0.4f; // Dauer des Kameraschwenks
    [SerializeField] private float cameraWait = 1f;         // Warten, bis die Kamera angekommen ist
    [SerializeField] private float shakeSeconds = 1.2f;
    [SerializeField] private float shakeAmount = 0.04f;
    [SerializeField] private float openedSeconds = 1f;      // Tor offen zeigen, bevor die Kamera zurück schwenkt

    private SpriteRenderer sr;
    private Collider2D col;
    private Transform player;
    private CameraFollow cam;
    private Vector3 basePosition;
    private int shownLevers = -1;
    private bool cutsceneRunning;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        basePosition = transform.position;

        // Cutscene schon gesehen (z.B. nach einem Tod): Tor ist sofort offen
        if (GameManager.Instance.GateOpened) SetOpen();
    }

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;
        cam = FindFirstObjectByType<CameraFollow>();
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;
        if (gm.GateOpened || cutsceneRunning) return;

        int count = gm.PulledLeverCount;
        if (count != shownLevers)
        {
            shownLevers = count;
            sr.sprite = lightSprites[Mathf.Clamp(count, 0, lightSprites.Length - 1)];
        }

        if (!gm.AllLeversPulled || player == null) return;
        if (gm.State != GameState.Playing) return;

        if (Vector2.Distance(player.position, transform.position) <= triggerDistance)
        {
            StartCoroutine(OpenCutscene());
        }
    }

    void OnDestroy()
    {
        // Szene wird mitten in der Cutscene verlassen: Spiel nicht im Cutscene-Zustand hängen lassen
        if (cutsceneRunning && GameManager.HasInstance && GameManager.Instance.State == GameState.Cutscene)
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
    }

    private IEnumerator OpenCutscene()
    {
        cutsceneRunning = true;
        GameManager gm = GameManager.Instance;
        gm.SetState(GameState.Cutscene);
        InteractPrompt.Hide();

        // 1. Kamera zum Tor
        if (cam != null) cam.FocusOn(transform, cameraSmoothTime);
        yield return new WaitForSeconds(cameraWait);

        // 2. Tor wackelt
        float t = 0f;
        while (t < shakeSeconds)
        {
            t += Time.deltaTime;
            transform.position = basePosition + new Vector3(Random.Range(-1f, 1f) * shakeAmount, 0f, 0f);
            yield return null;
        }
        transform.position = basePosition;

        // 3. Tor öffnet sich
        SetOpen();
        gm.GateOpened = true;
        yield return new WaitForSeconds(openedSeconds);

        // 4. Kamera zurück zum Spieler, dann wieder steuerbar
        if (cam != null) cam.FollowPlayer();
        yield return new WaitForSeconds(cameraWait * 0.6f);

        gm.SetState(GameState.Playing);
        cutsceneRunning = false;
    }

    private void SetOpen()
    {
        col.enabled = false;
        sr.sprite = openSprite;
    }
}