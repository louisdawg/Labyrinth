using System.Collections;
using UnityEngine;

// Tor zum Endraum: öffnet sich, wenn alle Hebel umgelegt sind.
// Die vier Lichter am Tor leuchten pro Hebel nach und nach auf.
// Collider darf KEIN Trigger sein.
public class Gate : MonoBehaviour
{
    [SerializeField] private Sprite[] lightSprites;   // gate_0 .. gate_4 (Größe 5)
    [SerializeField] private Sprite openSprite;       // gate_open
    [SerializeField] private float openDelay = 0.7f;  // kurz alle Lichter zeigen, dann öffnen

    private SpriteRenderer sr;
    private Collider2D col;
    private int shownLevers = -1;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
    }

    void Update()
    {
        int count = GameManager.Instance.PulledLeverCount;
        if (count == shownLevers) return;

        bool firstFrame = shownLevers < 0;
        shownLevers = count;

        int index = Mathf.Clamp(count, 0, lightSprites.Length - 1);
        sr.sprite = lightSprites[index];

        if (!GameManager.Instance.GateOpen) return;

        if (firstFrame) Open();                      // Szene mit schon offenem Tor geladen
        else StartCoroutine(OpenAfterDelay());
    }

    private IEnumerator OpenAfterDelay()
    {
        yield return new WaitForSeconds(openDelay);
        Open();
    }

    private void Open()
    {
        col.enabled = false;
        sr.sprite = openSprite;
    }
}