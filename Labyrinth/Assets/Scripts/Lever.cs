using UnityEngine;

// Hebel: mit E umlegen. Der Zustand steht im GameManager (bleibt nach dem Tod erhalten).
// Collider als Trigger, die ID setzt der LevelBuilder (0-3).
public class Lever : MonoBehaviour, IInteractable
{
    [SerializeField] private int leverId;
    [SerializeField] private Sprite offSprite;
    [SerializeField] private Sprite onSprite;

    private SpriteRenderer sr;

    public Vector3 PromptPosition => transform.position;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.sprite = GameManager.Instance.IsLeverPulled(leverId) ? onSprite : offSprite;
    }

    public bool CanInteract(GameManager gm)
    {
        return !gm.IsLeverPulled(leverId);
    }

    public string GetPrompt(GameManager gm)
    {
        return gm.IsLeverPulled(leverId) ? null : "[E]";
    }

    public void Interact(GameManager gm)
    {
        gm.PullLever(leverId);
        sr.sprite = onSprite;
    }
}