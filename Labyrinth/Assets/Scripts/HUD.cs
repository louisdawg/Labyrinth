using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUD : MonoBehaviour
{
    [Header("Anzeigen")]
    [SerializeField] private Slider hungerSlider;
    [SerializeField] private TMP_Text hintCounterText;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Sprite meatSprite;
    [SerializeField] private Sprite keySprite;

    [Header("Hinweise")]
    [SerializeField] private TMP_Text popupText;
    [SerializeField] private GameObject notesPanel;
    [SerializeField] private TMP_Text notesText;
    [SerializeField] private float popupDuration = 4f;

    private int lastHintCount;
    private float popupTimer;

    void Start()
    {
        lastHintCount = GameManager.Instance.FoundHintCount;
        popupText.gameObject.SetActive(false);
        notesPanel.SetActive(false);
    }

    void Update()
    {
        GameManager gm = GameManager.Instance;

        hungerSlider.value = gm.Hunger / gm.MaxHunger;
        hintCounterText.text = "Hinweise " + gm.FoundHintCount + "/" + GameManager.TotalHints;
        UpdateItemSlot(gm.HeldItem);
        
        if (gm.FoundHintCount > lastHintCount)
        {
            popupText.text = "Neuer Hinweis: " + gm.GetHintText(gm.LastHintId);
            popupText.gameObject.SetActive(true);
            popupTimer = popupDuration;
        }
        lastHintCount = gm.FoundHintCount;

        if (popupTimer > 0f)
        {
            popupTimer -= Time.unscaledDeltaTime;
            if (popupTimer <= 0f) popupText.gameObject.SetActive(false);
        }
        
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            notesPanel.SetActive(!notesPanel.activeSelf);
        }
        if (notesPanel.activeSelf)
        {
            notesText.text = gm.GetNotesText();
        }
    }

    private void UpdateItemSlot(Item item)
    {
        Sprite icon = null;
        if (item == Item.Fleisch) icon = meatSprite;
        else if (item == Item.Schluessel) icon = keySprite;

        itemIcon.sprite = icon;
        itemIcon.enabled = icon is not null;
    }
}