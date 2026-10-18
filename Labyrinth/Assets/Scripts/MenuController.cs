using UnityEngine;

// Liegt in StartMenu und WinScreen. Die Buttons rufen diese Methoden auf.
public class MenuController : MonoBehaviour
{
    void Start()
    {
        // Im Menü läuft kein Hunger
        GameManager.Instance.SetState(GameState.Menu);
    }

    public void StartGame()
    {
        GameSceneManager.Instance.StartNewGame();
    }

    public void QuitGame()
    {
        Application.Quit();
        Debug.Log("Spiel beendet (im Editor passiert nichts).");
    }
}