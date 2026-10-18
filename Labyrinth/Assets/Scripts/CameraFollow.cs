using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothTime = 0.1f;   // kleiner = straffer, 0 = starr

    private Transform player;
    private float currentSmoothTime;
    private Vector3 velocity;

    void Start()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;

        player = target;
        currentSmoothTime = smoothTime;
    }

    // Für Cutscenes: Kamera schaut kurz auf ein anderes Objekt (weicher Schwenk)
    public void FocusOn(Transform focus, float smooth)
    {
        target = focus;
        currentSmoothTime = smooth;
    }

    public void FollowPlayer()
    {
        target = player;
        currentSmoothTime = smoothTime;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);

        // SmoothDamp statt Lerp: gleichmäßig bei jeder Geschwindigkeit, und ein Ruckler (langer Frame)
        // lässt die Kamera nicht springen (deltaTime wird begrenzt)
        transform.position = Vector3.SmoothDamp(transform.position, goal, ref velocity, currentSmoothTime,
            Mathf.Infinity, Mathf.Min(Time.deltaTime, 0.05f));
    }
}