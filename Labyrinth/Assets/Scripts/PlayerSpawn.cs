using UnityEngine;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] Transform returnPoint;

    void Awake()
    {
        if (LevelGoal.ArrivalDirection >= 0 || returnPoint == null) return;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        Vector3 pos = returnPoint.position;
        player.transform.position = pos;

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.position = pos;
            rb.linearVelocity = Vector2.zero;
        }

        Camera cam = Camera.main;
        if (cam != null)
            cam.transform.position = new Vector3(pos.x, pos.y, cam.transform.position.z);
    }
}