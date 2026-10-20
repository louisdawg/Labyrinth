using UnityEngine;
using UnityEngine.SceneManagement;

// Wrapper um den Unity-SceneManager (Singleton): Szenennamen und Szenenwechsel an einer Stelle.
// Heißt "GameSceneManager", weil "SceneManager" schon eine Unity-Klasse ist.
public class GameSceneManager : MonoBehaviour
{
    public const string StartMenuScene = "StartMenu";
    public const string FirstLevelScene = "Level1";
    public const string WinScene = "WinScreen";
    public const string LoseScene = "LoseScreen";

    private static GameSceneManager instance;
    public static GameSceneManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("GameSceneManager").AddComponent<GameSceneManager>();
            }
            return instance;
        }
    }

    void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    public void LoadScene(int buildIndex)
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(buildIndex);
    }

    public void ReloadCurrent()
    {
        LoadScene(SceneManager.GetActiveScene().name);
    }

    // Dynamisch: offset +1 = nächste Szene in den Build Settings, -1 = vorherige
    public void LoadRelative(int offset)
    {
        int target = SceneManager.GetActiveScene().buildIndex + offset;
        if (target < 0 || target >= SceneManager.sceneCountInBuildSettings)
        {
            Debug.LogWarning("Keine Szene mit Build-Index " + target + " in den Build Settings.");
            return;
        }
        LoadScene(target);
    }

    public void LoadNext() { LoadRelative(1); }

    public void LoadStartMenu() { LoadScene(StartMenuScene); }

    public void LoadWinScreen() { LoadScene(WinScene); }

    // Mit Ausblenden (Wächter-Ending)
    public void LoadLoseScreen()
    {
        Time.timeScale = 1f;
        ScreenFader.Instance.FadeAndLoad(LoseScene);
    }

    // Neues Spiel: alles zurücksetzen, dann Level 1
    public void StartNewGame()
    {
        GameManager.Instance.ResetGame();
        LoadScene(FirstLevelScene);
    }
}