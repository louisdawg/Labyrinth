using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    [Header("Körper")]
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Sprite idleSprite;
    [SerializeField] private Sprite[] walkFrames;
    [SerializeField] private float frameTime = 0.12f;

    [Header("Item im Mund")]
    [SerializeField] private SpriteRenderer mouthItem;
    [SerializeField] private Sprite meatInMouth;
    [SerializeField] private Sprite keyInMouth;
    [SerializeField] private Vector2 mouthPosition = new Vector2(0.16f, -0.25f);
    [SerializeField] private float stretchedFrameLift = 0.0625f;

    private Rigidbody2D rb;
    private float timer;
    private int frame;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        bool moving = rb.linearVelocity.sqrMagnitude > 0.05f;
        bool stretched = false;

        if (moving && walkFrames.Length > 0)
        {
            timer += Time.deltaTime * GameManager.Instance.SpeedMultiplier;
            if (timer >= frameTime)
            {
                timer -= frameTime;
                frame = (frame + 1) % walkFrames.Length;
            }
            body.sprite = walkFrames[frame];
            stretched = frame % 2 == 1;
        }
        else
        {
            frame = 0;
            timer = 0f;
            body.sprite = idleSprite;
        }

        UpdateMouthItem(stretched);
    }

    private void UpdateMouthItem(bool stretched)
    {
        Item item = GameManager.Instance.HeldItem;

        Sprite sprite = null;
        if (item == Item.Fleisch) sprite = meatInMouth;
        else if (item == Item.Schluessel) sprite = keyInMouth;

        mouthItem.enabled = sprite is not null;
        mouthItem.sprite = sprite;

        Vector2 pos = mouthPosition;
        if (stretched) pos.y += stretchedFrameLift;
        mouthItem.transform.localPosition = pos;
    }
}