using UnityEngine;

public class HintPickup : MonoBehaviour
{
    [SerializeField] private int hintId;

    void Start()
    {
        if (GameManager.Instance.HasHint(hintId))
        {
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameManager.Instance.AddHint(hintId);
        Destroy(gameObject);
    }
}