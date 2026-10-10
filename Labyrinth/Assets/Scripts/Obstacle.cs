using UnityEngine;
using UnityEngine.SceneManagement;

public class Obstacle : MonoBehaviour
{
    private bool triggered;

    void OnCollisionEnter2D(Collision2D collision)
    {
        Check(collision);
    }
    
    void OnCollisionStay2D(Collision2D collision)
    {
        Check(collision);
    }

    private void Check(Collision2D collision)
    {
        if (triggered) return;

        if (collision.gameObject.CompareTag("Player"))
        {
            triggered = true;
            string scene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(scene);
        }
    }
}