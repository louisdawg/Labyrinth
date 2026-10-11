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

    private static readonly Color green = new Color32(91, 194, 54, 255);
    private static readonly Color yellow = new Color32(232, 201, 58, 255);
    private static readonly Color red = new Color32(216, 64, 58, 255);

    private int lastHintCount;
    private bool pausedByHud;

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
        UpdateItemSlot(gm.HeldItem);

        if (gm.FoundHintCount > lastHintCount)
        {
            hintPopupText.text = gm.GetHintText(gm.LastHintId) + "\n\n<size=60%>Weiter mit [Leertaste]</size>";
            hintPopupPanel.SetActive(true);
            notesPanel.SetActive(false);
        }
        lastHintCount = gm.FoundHintCount;

        if (hintPopupPanel.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.Escape))
            {
                hintPopupPanel.SetActive(false);
            }
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
        }

        UpdatePause(gm);
    }

    private void UpdatePause(GameManager gm)
    {
        bool panelOpen = hintPopupPanel.activeSelf || notesPanel.activeSelf;

        if (panelOpen && gm.State == GameState.Playing)
        {
            gm.SetState(GameState.Paused);
            pausedByHud = true;
        }
        else if (!panelOpen && pausedByHud)
        {
            gm.SetState(GameState.Playing);
            pausedByHud = false;
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
        itemIcon.enabled = icon is not null;
    }

    void OnDestroy()
    {
        if (pausedByHud && GameManager.HasInstance)
        {
            GameManager.Instance.SetState(GameState.Playing);
        }
    }
}