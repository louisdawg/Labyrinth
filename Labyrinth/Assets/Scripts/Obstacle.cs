using UnityEngine;
using UnityEngine.SceneManagement;

public class Obstacle : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            string scene = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(scene);
        }
    }
}