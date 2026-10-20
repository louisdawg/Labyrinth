using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Dialogfenster am unteren Bildschirmrand. Baut sich selbst auf, in der Szene muss nichts eingerichtet werden.
// Benutzen (innerhalb einer Coroutine, mit StartCoroutine):
//   yield return StartCoroutine(DialogueBox.Say("Text"));      // Schreibmaschine, Leertaste/Enter/E = weiter
//   yield return StartCoroutine(DialogueBox.Ask("Frage?", new[] { "Ja", "Nein" }, i => choice = i));
//   DialogueBox.Close();
// Auswahl: A/D oder Pfeiltasten, bestätigen mit Leertaste/Enter/E.
public class DialogueBox : MonoBehaviour
{
    private const float CharactersPerSecond = 30f;
    private const int SortingOrder = 500;   // unter dem ScreenFader (999)

    private static readonly Color panelColor = new Color32(20, 16, 26, 235);
    private static readonly Color borderColor = new Color32(163, 111, 210, 255);
    private const string SelectedColor = "#E8C93A";

    private static DialogueBox instance;

    private static DialogueBox Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("DialogueBox").AddComponent<DialogueBox>();
            }
            return instance;
        }
    }

    private GameObject panel;
    private TMP_Text label;
    private TMP_Text hint;

    void Awake()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = SortingOrder;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(transform, false);

        RectTransform rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.sizeDelta = new Vector2(1500f, 320f);
        rt.anchoredPosition = new Vector2(0f, 60f);

        Image image = panel.GetComponent<Image>();
        image.color = panelColor;
        image.raycastTarget = false;

        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = borderColor;
        outline.effectDistance = new Vector2(4f, -4f);

        label = CreateText("Text", 46f, TextAlignmentOptions.TopLeft);
        RectTransform labelRect = label.rectTransform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(60f, 70f);
        labelRect.offsetMax = new Vector2(-60f, -40f);

        hint = CreateText("Hint", 30f, TextAlignmentOptions.BottomRight);
        hint.color = new Color32(180, 180, 190, 255);
        RectTransform hintRect = hint.rectTransform;
        hintRect.anchorMin = hintRect.anchorMax = new Vector2(1f, 0f);
        hintRect.pivot = new Vector2(1f, 0f);
        hintRect.sizeDelta = new Vector2(700f, 50f);
        hintRect.anchoredPosition = new Vector2(-40f, 20f);

        panel.SetActive(false);
    }

    private TMP_Text CreateText(string objectName, float size, TextAlignmentOptions alignment)
    {
        GameObject obj = new GameObject(objectName, typeof(RectTransform));
        obj.transform.SetParent(panel.transform, false);

        TextMeshProUGUI tmp = obj.AddComponent<TextMeshProUGUI>();
        tmp.fontSize = size;
        tmp.color = new Color32(242, 242, 242, 255);
        tmp.alignment = alignment;
        tmp.raycastTarget = false;
        return tmp;
    }

    // ---------- Benutzen ----------
    public static IEnumerator Say(string text)
    {
        DialogueBox box = Instance;
        box.panel.SetActive(true);
        box.hint.text = "";
        box.label.text = text;
        box.label.maxVisibleCharacters = 0;

        float typed = 0f;
        while (typed < text.Length)
        {
            typed += CharactersPerSecond * Time.deltaTime;
            if (ConfirmPressed()) typed = text.Length;   // Drücken = Rest sofort anzeigen
            box.label.maxVisibleCharacters = Mathf.FloorToInt(typed);
            yield return null;
        }

        box.label.maxVisibleCharacters = 99999;
        box.hint.text = "[Leertaste]";

        while (!ConfirmPressed()) yield return null;
        box.hint.text = "";
    }

    public static IEnumerator Ask(string question, string[] options, Action<int> onChosen)
    {
        DialogueBox box = Instance;
        box.panel.SetActive(true);
        box.label.maxVisibleCharacters = 99999;
        box.hint.text = "[A / D] wählen   [Leertaste] bestätigen";

        int selected = 0;
        while (true)
        {
            box.label.text = question + "\n\n" + BuildOptions(options, selected);
            yield return null;   // erst im nächsten Frame Eingaben lesen (der letzte Tastendruck zählt nicht)

            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)
                || Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                selected = (selected - 1 + options.Length) % options.Length;
            }
            else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)
                || Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))
            {
                selected = (selected + 1) % options.Length;
            }
            else if (ConfirmPressed())
            {
                break;
            }
        }

        box.hint.text = "";
        onChosen(selected);
    }

    public static void Close()
    {
        if (instance != null) instance.panel.SetActive(false);
    }

    // ---------- intern ----------
    private static bool ConfirmPressed()
    {
        return Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)
            || Input.GetKeyDown(KeyCode.KeypadEnter) || Input.GetKeyDown(KeyCode.E);
    }

    private static string BuildOptions(string[] options, int selected)
    {
        string text = "";
        for (int i = 0; i < options.Length; i++)
        {
            text += i == selected
                ? "<color=" + SelectedColor + ">> " + options[i] + "</color>"
                : "   " + options[i];
            text += "        ";
        }
        return text;
    }
}