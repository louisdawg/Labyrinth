using UnityEngine;

// Tür: mit Schlüssel im Mund und E öffnen, der Schlüssel wird verbraucht.
// Collider darf KEIN Trigger sein, sonst läuft der Spieler durch.
public class Door : MonoBehaviour, IInteractable
{
    private SpriteRenderer sr;
    private Collider2D col;
    private bool opened;

    public Vector3 PromptPosition => transform.position;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        if (GameManager.Instance.DoorOpened) Open();
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
        Open();
    }

    private void Open()
    {
        opened = true;
        col.enabled = false;
        sr.enabled = false;
    }
}