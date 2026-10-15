using UnityEngine;

// Alles, was man mit E benutzen kann (Tür, später Hebel).
// Das Objekt braucht einen Collider, damit PlayerInteraction es findet.
public interface IInteractable
{
    Vector3 PromptPosition { get; }
    bool CanInteract(GameManager gm);
    string GetPrompt(GameManager gm);   // null = kein Symbol anzeigen
    void Interact(GameManager gm);
}