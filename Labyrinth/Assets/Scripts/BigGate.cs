using System.Collections;
using UnityEngine;

// Großes Tor in Level 3. Es öffnet sich nur, wenn der Wächter es nach dem Gespräch befiehlt (Guard.cs).
// Der Zustand steht im GameManager (BigGateOpened), das Tor bleibt auch nach einem Neustart offen.
// Collider darf KEIN Trigger sein. Im Prefab Scale ca. 1.5 einstellen, damit es groß wirkt.
public class BigGate : MonoBehaviour
{
    [SerializeField] private Sprite closedSprite;   // z.B. gate_4
    [SerializeField] private Sprite openSprite;     // z.B. gate_open
    [SerializeField] private float shakeSeconds = 1.5f;
    [SerializeField] private float shakeAmount = 0.05f;

    private SpriteRenderer sr;
    private Collider2D col;
    private Vector3 basePosition;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        basePosition = transform.position;

        if (GameManager.Instance.BigGateOpened) SetOpen();
        else if (closedSprite != null) sr.sprite = closedSprite;
    }

    // Wird vom Wächter mit StartCoroutine(gate.Open()) aufgerufen
    public IEnumerator Open()
    {
        float t = 0f;
        while (t < shakeSeconds)
        {
            t += Time.deltaTime;
            transform.position = basePosition + new Vector3(Random.Range(-1f, 1f) * shakeAmount, 0f, 0f);
            yield return null;
        }
        transform.position = basePosition;
        SetOpen();
    }

    private void SetOpen()
    {
        col.enabled = false;
        if (openSprite != null) sr.sprite = openSprite;
    }
}