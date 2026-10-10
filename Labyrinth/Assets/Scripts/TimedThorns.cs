using UnityEngine;

public class TimedThorns : MonoBehaviour
{
    [SerializeField] private float activeTime = 1.5f;
    [SerializeField] private float inactiveTime = 2f;
    [SerializeField] private float warnTime = 0.6f;
    [SerializeField] private float timeOffset = -1f;

    private SpriteRenderer sr;
    private Collider2D col;
    private float period;
    private float offset;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        col = GetComponent<Collider2D>();
        period = activeTime + inactiveTime;
        
        offset = timeOffset >= 0f
            ? timeOffset
            : Mathf.Abs(transform.position.x * 7.3f + transform.position.y * 3.1f) % period;
    }

    void Update()
    {
        float t = (Time.timeSinceLevelLoad + offset) % period;

        bool active = t >= inactiveTime;
        bool warning = !active && t >= inactiveTime - warnTime;

        col.enabled = active;

        Color c = sr.color;
        if (active) c.a = 1f;
        else if (warning) c.a = 0.35f + 0.55f * Mathf.PingPong(Time.time * 8f, 1f);
        else c.a = 0.25f;
        sr.color = c;
    }
}