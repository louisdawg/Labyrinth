using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;
    [SerializeField] private float smoothSpeed = 8f;

    void Start()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;
    }
    
    void LateUpdate()
    {
        if (target is null) return;
        
        Vector3 goal = new Vector3(target.position.x, target.position.y, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goal, smoothSpeed * Time.deltaTime);
    }
}