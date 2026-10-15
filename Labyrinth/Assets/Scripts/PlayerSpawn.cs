using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerSpawn : MonoBehaviour
{
    [SerializeField] Transform returnPoint;     // Ankunft von hinten (Level 2 -> Level 1)
    [SerializeField] Transform dungeonSpawn;    // Respawn hinter der Tür (Level 2), vom LevelBuilder gesetzt

    // Merkt sich, welche Szene zuletzt geladen wurde: gleiche Szene nochmal = Neustart nach Tod
    private static string lastScene;

    void Awake()
    {
        string scene = SceneManager.GetActiveScene().name;
        bool isRestart = scene == lastScene;
        lastScene = scene;

        // Tür schon geöffnet und gestorben: im Dungeon neu starten, nicht im Rätselraum
        if (isRestart && dungeonSpawn != null && GameManager.Instance.DoorOpened)
        {
            MovePlayer(dungeonSpawn.position);
            return;
        }

        if (LevelGoal.ArrivalDirection >= 0 || returnPoint == null) return;
        MovePlayer(returnPoint.position);
    }

    private void MovePlayer(Vector3 pos)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

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