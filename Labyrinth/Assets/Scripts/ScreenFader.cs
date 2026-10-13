using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ScreenFader : MonoBehaviour
{
    static ScreenFader instance;

    public static ScreenFader Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = new GameObject("ScreenFader");
                DontDestroyOnLoad(go);
                instance = go.AddComponent<ScreenFader>();
                instance.Build();
            }
            return instance;
        }
    }

    [SerializeField] float duration = 0.4f;

    CanvasGroup group;

    void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        group = gameObject.AddComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;

        GameObject imgObj = new GameObject("Black");
        imgObj.transform.SetParent(transform, false);
        Image img = imgObj.AddComponent<Image>();
        img.color = Color.black;
        img.raycastTarget = false;

        RectTransform rt = img.rectTransform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    public void FadeAndLoad(int buildIndex)
    {
        StartCoroutine(Routine(buildIndex));
    }

    IEnumerator Routine(int buildIndex)
    {
        yield return Fade(1f);
        SceneManager.LoadScene(buildIndex);
        yield return null;
        yield return Fade(0f);
    }

    IEnumerator Fade(float target)
    {
        float start = group.alpha;
        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            group.alpha = Mathf.Lerp(start, target, t / duration);
            yield return null;
        }
        group.alpha = target;
    }
}