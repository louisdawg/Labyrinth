using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelGoal : MonoBehaviour
{
    public static int ArrivalDirection = 1;

    [SerializeField] int direction = 1;

    bool triggered;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void ResetStatics()
    {
        ArrivalDirection = 1;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered || !other.CompareTag("Player")) return;

        int target = SceneManager.GetActiveScene().buildIndex + direction;
        if (target < 0 || target >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("LevelGoal: Keine Szene mit Build-Index " + target + " in den Build Settings!");
            return;
        }

        triggered = true;
        ArrivalDirection = direction;

        Rigidbody2D rb = other.attachedRigidbody;
        if (rb != null) rb.linearVelocity = Vector2.zero;
        PlayerController pc = other.GetComponent<PlayerController>();
        if (pc != null) pc.enabled = false;

        ScreenFader.Instance.FadeAndLoad(target);
    }
}