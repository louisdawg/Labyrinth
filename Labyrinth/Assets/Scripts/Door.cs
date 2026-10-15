using System.Collections;
using UnityEngine;

// Tür: mit Schlüssel im Mund und E öffnen, der Schlüssel wird verbraucht.
// Collider darf KEIN Trigger sein, sonst läuft der Spieler durch.
public class Door : MonoBehaviour, IInteractable
{
    [Header("Öffnen-Animation (optional)")]
    [SerializeField] private Sprite[] openFrames;       // z.B. door_open_0..2, der letzte Frame bleibt stehen
    [SerializeField] private float framesPerSecond = 8f;
    [SerializeField] private float fadeSeconds = 0.4f;  // ohne Frames: Tür blendet aus

    private SpriteRenderer sr;
    private Collider2D col;
    private bool opened;

    public Vector3 PromptPosition => transform.position;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();

        if (GameManager.Instance.DoorOpened)
        {
            opened = true;
            col.enabled = false;
            ShowOpenInstantly();
        }
    }

    public bool CanInteract(GameManager gm)
    {
        return !opened && gm.HeldItem == Item.Schluessel;
    }

    public string GetPrompt(GameManager gm)
    {
        if (opened) return null;
        return gm.HeldItem == Item.Schluessel ? "[E]" : "Verschlossen";
    }

    public void Interact(GameManager gm)
    {
        gm.HeldItem = Item.None;
        gm.DoorOpened = true;

        opened = true;
        col.enabled = false;   // sofort begehbar
        StartCoroutine(OpenRoutine());
    }

    private void ShowOpenInstantly()
    {
        if (openFrames != null && openFrames.Length > 0) sr.sprite = openFrames[openFrames.Length - 1];
        else sr.enabled = false;
    }

    private IEnumerator OpenRoutine()
    {
        if (openFrames != null && openFrames.Length > 0)
        {
            foreach (Sprite frame in openFrames)
            {
                sr.sprite = frame;
                yield return new WaitForSeconds(1f / framesPerSecond);
            }
            yield break;
        }

        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.deltaTime;
            Color c = sr.color;
            c.a = Mathf.Clamp01(1f - t / fadeSeconds);
            sr.color = c;
            yield return null;
        }
        sr.enabled = false;
    }
}