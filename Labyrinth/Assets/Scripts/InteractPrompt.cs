using TMPro;
using UnityEngine;

// Kleines Tasten-Symbol ("[E]") in der Spielwelt.
// Erstellt sich selbst, es muss nichts in der Szene eingerichtet werden.
// Benutzen: InteractPrompt.Show(position, "[E]");  InteractPrompt.Hide();
public class InteractPrompt : MonoBehaviour
{
    private const float FontSize = 3f;      // Größe anpassen, falls zu klein/groß
    private const float HeightOffset = 0.5f;
    private const int SortingOrder = 20;

    private static InteractPrompt instance;

    private TextMeshPro label;

    private static InteractPrompt Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameObject("InteractPrompt").AddComponent<InteractPrompt>();
            }
            return instance;
        }
    }

    void Awake()
    {
        label = gameObject.AddComponent<TextMeshPro>();
        label.fontSize = FontSize;
        label.alignment = TextAlignmentOptions.Center;
        label.color = Color.white;
        label.sortingOrder = SortingOrder;
        label.rectTransform.sizeDelta = new Vector2(3f, 1f);
    }

    public static void Show(Vector3 worldPosition, string text)
    {
        InteractPrompt prompt = Instance;
        prompt.gameObject.SetActive(true);
        prompt.label.text = text;

        float bob = 0.06f * Mathf.Sin(Time.time * 4f);
        prompt.transform.position = worldPosition + new Vector3(0f, HeightOffset + bob, 0f);
    }

    public static void Hide()
    {
        if (instance != null) instance.gameObject.SetActive(false);
    }
}