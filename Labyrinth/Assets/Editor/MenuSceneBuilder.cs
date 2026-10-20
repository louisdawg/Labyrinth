using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// Menü: Labyrinth -> Menüszenen erstellen
// Baut StartMenu und WinScreen komplett (Kamera, Canvas, Texte, Buttons) und ordnet die Build Settings.
public static class MenuSceneBuilder
{
    enum ButtonAction { Start, Quit }

    static readonly Color Background = new Color32(20, 16, 26, 255);
    static readonly Color TitleColor = new Color32(163, 111, 210, 255);
    static readonly Color TextColor = new Color32(242, 242, 242, 255);
    static readonly Color ButtonNormal = new Color32(115, 72, 40, 255);
    static readonly Color ButtonHighlight = new Color32(146, 98, 56, 255);
    static readonly Color ButtonPressed = new Color32(69, 40, 23, 255);

    [MenuItem("Labyrinth/Menüszenen erstellen (StartMenu, WinScreen, LoseScreen)")]
    static void BuildMenuScenes()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string folder = GetSceneFolder();
        string startPath = folder + "/StartMenu.unity";
        string winPath = folder + "/WinScreen.unity";
        string losePath = folder + "/LoseScreen.unity";

        bool exists = AssetDatabase.LoadAssetAtPath<SceneAsset>(startPath) != null
                   || AssetDatabase.LoadAssetAtPath<SceneAsset>(winPath) != null
                   || AssetDatabase.LoadAssetAtPath<SceneAsset>(losePath) != null;
        if (exists && !EditorUtility.DisplayDialog("Menüszenen", "StartMenu, WinScreen oder LoseScreen gibt es schon. Überschreiben?",
                "Überschreiben", "Abbrechen")) return;

        // Texte sind Platzhalter, später im Inspector ändern
        CreateScene(startPath, "Labyrinth",
            "Tief im Dungeon erwacht ein hungriges Wesen. Es will nur eines: raus.",
            new[] { ("Spiel starten", ButtonAction.Start), ("Beenden", ButtonAction.Quit) });

        CreateScene(winPath, "Du hast gewonnen!",
            "Du bist entkommen. Der Dungeon vergisst dich nicht.\n\nTo be continued...",
            new[] { ("Nochmal spielen", ButtonAction.Start) });

        CreateLoseScene(losePath);

        UpdateBuildSettings();
        EditorSceneManager.OpenScene(startPath);
        Debug.Log("StartMenu, WinScreen und LoseScreen erstellt, Build Settings geordnet.");
    }

    // Nur den LoseScreen bauen, StartMenu und WinScreen (mit euren Änderungen) bleiben unberührt
    [MenuItem("Labyrinth/LoseScreen erstellen")]
    static void BuildLoseScreenOnly()
    {
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        string path = GetSceneFolder() + "/LoseScreen.unity";
        if (AssetDatabase.LoadAssetAtPath<SceneAsset>(path) != null
            && !EditorUtility.DisplayDialog("LoseScreen", "LoseScreen gibt es schon. Überschreiben?", "Überschreiben", "Abbrechen")) return;

        CreateLoseScene(path);
        UpdateBuildSettings();
        EditorSceneManager.OpenScene(path);
        Debug.Log("LoseScreen erstellt.");
    }

    // Build Settings neu ordnen (zum Beispiel, nachdem Level3 und Level4 angelegt wurden)
    [MenuItem("Labyrinth/Build Settings ordnen")]
    static void OrderBuildSettings()
    {
        UpdateBuildSettings();
        Debug.Log("Build Settings: StartMenu, Level1, Level2, Level3, Level4, WinScreen, LoseScreen (fehlende Szenen werden übersprungen).");
    }

    static void CreateLoseScene(string path)
    {
        // Texte sind Platzhalter, später im Inspector ändern
        CreateScene(path, "Du hast verloren",
            "Der Dungeon lässt niemanden gehen, der ihm nicht freundlich gesinnt ist.",
            new[] { ("Nochmal spielen", ButtonAction.Start) });
    }

    static void CreateScene(string path, string title, string subtitle, (string label, ButtonAction action)[] buttons)
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        GameObject camObj = new GameObject("Main Camera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(0f, 0f, -10f);
        Camera cam = camObj.AddComponent<Camera>();
        cam.orthographic = true;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Background;

        GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));

        MenuController controller = new GameObject("MenuController").AddComponent<MenuController>();

        CreateText(canvas.transform, "Title", title, 140f, TitleColor, new Vector2(0f, 260f), new Vector2(1600f, 200f));
        CreateText(canvas.transform, "Subtitle", subtitle, 44f, TextColor, new Vector2(0f, 90f), new Vector2(1300f, 140f));

        float y = -80f;
        foreach (var (label, action) in buttons)
        {
            CreateButton(canvas.transform, label, new Vector2(0f, y), controller, action);
            y -= 150f;
        }

        EditorSceneManager.SaveScene(scene, path);
    }

    static TextMeshProUGUI CreateText(Transform parent, string name, string text, float size, Color color, Vector2 position, Vector2 boxSize)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = boxSize;
        rt.anchoredPosition = position;

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.text = text;
        tmp.fontSize = size;
        tmp.color = color;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.raycastTarget = false;
        return tmp;
    }

    static void CreateButton(Transform parent, string label, Vector2 position, MenuController controller, ButtonAction action)
    {
        GameObject obj = new GameObject(label + "Button", typeof(RectTransform), typeof(Image), typeof(Button));
        obj.transform.SetParent(parent, false);

        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(520f, 110f);
        rt.anchoredPosition = position;

        Image image = obj.GetComponent<Image>();
        image.color = Color.white;

        Button button = obj.GetComponent<Button>();
        button.targetGraphic = image;
        ColorBlock colors = button.colors;
        colors.normalColor = ButtonNormal;
        colors.highlightedColor = ButtonHighlight;
        colors.selectedColor = ButtonHighlight;
        colors.pressedColor = ButtonPressed;
        colors.colorMultiplier = 1f;
        button.colors = colors;

        CreateText(obj.transform, "Label", label, 56f, TextColor, Vector2.zero, new Vector2(500f, 100f));

        if (action == ButtonAction.Start) UnityEventTools.AddVoidPersistentListener(button.onClick, controller.StartGame);
        else UnityEventTools.AddVoidPersistentListener(button.onClick, controller.QuitGame);
    }

    static string GetSceneFolder()
    {
        string level1 = FindScene("Level1");
        if (level1 != null) return System.IO.Path.GetDirectoryName(level1).Replace('\\', '/');
        if (!AssetDatabase.IsValidFolder("Assets/Scenes")) AssetDatabase.CreateFolder("Assets", "Scenes");
        return "Assets/Scenes";
    }

    static string FindScene(string sceneName)
    {
        foreach (string guid in AssetDatabase.FindAssets(sceneName + " t:Scene"))
        {
            string p = AssetDatabase.GUIDToAssetPath(guid);
            if (System.IO.Path.GetFileNameWithoutExtension(p) == sceneName) return p;
        }
        return null;
    }

    // Reihenfolge ist wichtig: LevelGoal lädt buildIndex + 1 (nächstes Level, danach der WinScreen)
    // Szenen, die es noch nicht gibt, werden übersprungen: Level3 und Level4 zuerst anlegen, dann "Build Settings ordnen".
    static void UpdateBuildSettings()
    {
        string[] order = { "StartMenu", "Level1", "Level2", "Level3", "Level4", "WinScreen", "LoseScreen" };
        List<EditorBuildSettingsScene> list = new List<EditorBuildSettingsScene>();

        foreach (string sceneName in order)
        {
            string path = FindScene(sceneName);
            if (path == null) { Debug.LogWarning("Szene '" + sceneName + "' nicht gefunden, nicht in den Build Settings."); continue; }
            list.Add(new EditorBuildSettingsScene(path, true));
        }

        foreach (EditorBuildSettingsScene existing in EditorBuildSettings.scenes)
        {
            if (!list.Any(s => s.path == existing.path)) list.Add(existing);
        }

        EditorBuildSettings.scenes = list.ToArray();
    }
}