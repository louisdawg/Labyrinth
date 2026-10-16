using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Hunger (senkrechte Leiste)")]
    [SerializeField] private Image hungerFill;

    [Header("Item-Slot")]
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite meatSprite;
    [SerializeField] private Sprite keySprite;

    [Header("Hinweise")]
    [SerializeField] private TMP_Text hintCounterText;

    [SerializeField] private GameObject hintPopupPanel;
    [SerializeField] private TMP_Text hintPopupText;
    [SerializeField] private GameObject notesPanel;
    [SerializeField] private TMP_Text notesText;

    [Header("Hebel (nur Level 2, sonst leer lassen)")]
    [SerializeField] private TMP_Text leverCounterText;

    [Header("Tasten-Legende")]
    [SerializeField] private TMP_Text legendText;

    [Header("Hinweis-Fenster")]
    [SerializeField] private float charactersPerSecond = 30f;
    [SerializeField] private float autoCloseSeconds = 8f;

    private static readonly Color green = new Color32(91, 194, 54, 255);
    private static readonly Color yellow = new Color32(232, 201, 58, 255);
    private static readonly Color red = new Color32(216, 64, 58, 255);

    private int lastLegendMask = -1;
    private int lastHintCount;
    private string fullHintText = "";
    private float typedCharacters;
    private bool typing;
    private float closeTimer;

    void Start()
    {
        lastHintCount = GameManager.Instance.FoundHintCount;
        hintPopupPanel.SetActive(false);
        notesPanel.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;

        UpdateHunger(gm);
        hintCounterText.text = gm.FoundHintCount + " / " + GameManager.TotalHints;
        if (leverCounterText != null)
        {
            leverCounterText.text = gm.PulledLeverCount + " / " + GameManager.TotalLevers;
        }
        UpdateItemSlot(gm.HeldItem);

        if (gm.FoundHintCount > lastHintCount)
        {
            OpenHintPopup(gm.GetHintText(gm.LastHintId));
        }
        lastHintCount = gm.FoundHintCount;

        if (hintPopupPanel.activeSelf)
        {
            UpdateHintPopup();
        }
        else
        {
            bool tab = Input.GetKeyDown(KeyCode.Tab);
            bool esc = notesPanel.activeSelf && Input.GetKeyDown(KeyCode.Escape);
            if (tab || esc) notesPanel.SetActive(!notesPanel.activeSelf);
        }

        if (notesPanel.activeSelf)
        {
            notesText.text = gm.GetNotesText();
            gm.UsedNotes = true;
        }

        UpdateLegend(gm);
    }

    // Zeigt nur die Tasten, die der Spieler noch nie benutzt hat
    private void UpdateLegend(GameManager gm)
    {
        if (legendText == null) return;

        int mask = (gm.UsedInteract ? 1 : 0) | (gm.UsedEat ? 2 : 0) | (gm.UsedNotes ? 4 : 0);
        if (mask == lastLegendMask) return;
        lastLegendMask = mask;

        string text = "";
        if (!gm.UsedInteract) text += "<b>E</b>  aufheben / ablegen\n";
        if (!gm.UsedEat) text += "<b>F</b>  essen\n";
        if (!gm.UsedNotes) text += "<b>Tab</b>  Notizen\n";

        legendText.text = text.TrimEnd('\n');
    }

    private void OpenHintPopup(string text)
    {
        fullHintText = text;
        typedCharacters = 0f;
        typing = true;
        closeTimer = 0f;

        hintPopupText.text = fullHintText;
        hintPopupText.maxVisibleCharacters = 0;

        hintPopupPanel.SetActive(true);
        notesPanel.SetActive(false);
    }

    private void UpdateHintPopup()
    {
        bool confirm = Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape);

        if (typing)
        {
            typedCharacters += charactersPerSecond * Time.deltaTime;
            if (confirm) typedCharacters = fullHintText.Length;

            hintPopupText.maxVisibleCharacters = Mathf.FloorToInt(typedCharacters);

            if (typedCharacters >= fullHintText.Length)
            {
                typing = false;
                hintPopupText.text = fullHintText + "\n\n<size=60%>[Leertaste]</size>";
                hintPopupText.maxVisibleCharacters = 99999;
                closeTimer = autoCloseSeconds;
            }
            return;
        }

        closeTimer -= Time.deltaTime;
        if (confirm || closeTimer <= 0f)
        {
            hintPopupPanel.SetActive(false);
        }
    }

    private void UpdateHunger(GameManager gm)
    {
        float value = gm.Hunger / gm.MaxHunger;
        hungerFill.fillAmount = value;

        Color c = value > 0.5f
            ? Color.Lerp(yellow, green, (value - 0.5f) * 2f)
            : Color.Lerp(red, yellow, value * 2f);
        if (value <= 0.2f)
        {
            c.a = 0.6f + 0.4f * Mathf.PingPong(Time.unscaledTime * 3f, 1f);
        }
        hungerFill.color = c;
    }

    private void UpdateItemSlot(Item item)
    {
        Sprite icon = null;
        if (item == Item.Fleisch) icon = meatSprite;
        else if (item == Item.Schluessel) icon = keySprite;

        itemIcon.sprite = icon;
        itemIcon.enabled = icon != null;
    }
}