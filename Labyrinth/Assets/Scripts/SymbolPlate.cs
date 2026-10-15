using System.Collections;
using UnityEngine;

public enum Symbol { Sonne, Mond, Stern, Auge }

// Druckplatte mit Symbol. Die Logik (Reihenfolge) steckt in SymbolPuzzle.
public class SymbolPlate : MonoBehaviour
{
    public Symbol symbol;

    // Reihenfolge: Sonne, Mond, Stern, Auge
    [SerializeField] private Sprite[] symbolSprites = new Sprite[4];

    private static readonly Color inactiveColor = new Color(0.55f, 0.55f, 0.6f, 1f);
    private static readonly Color activeColor = Color.white;
    private static readonly Color wrongColor = new Color(1f, 0.3f, 0.3f, 1f);

    private SpriteRenderer sr;

    public bool IsActive { get; private set; }

    // Wird vom LevelBuilder aufgerufen (setzt Symbol und Sprite)
    public void Setup(Symbol s)
    {
        symbol = s;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        int index = (int)s;
        if (renderer != null && index < symbolSprites.Length && symbolSprites[index] != null)
        {
            renderer.sprite = symbolSprites[index];
        }
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        SetActive(false);
        SymbolPuzzle.Instance.Register(this);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        SymbolPuzzle.Instance.Press(this);
    }

    public void SetActive(bool active)
    {
        IsActive = active;
        sr.color = active ? activeColor : inactiveColor;
    }

    // Falsche Reihenfolge: kurz rot, dann wieder inaktiv
    public void FlashWrong()
    {
        IsActive = false;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        sr.color = wrongColor;
        yield return new WaitForSeconds(0.5f);
        sr.color = inactiveColor;
    }
}