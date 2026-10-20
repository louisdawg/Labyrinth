using UnityEngine;
using UnityEngine.Rendering.Universal;

// Pro Szene einstellbar: Grundhelligkeit, Lichtfarbe und Sichtradius des Spielers.
// Auf ein leeres Objekt "LevelLighting" in jeder Levelszene legen.
// Offene Außenlevels (Level 4): Global Intensity 1, Use Global Color an (warmes Weiß), Disable Player Light an.
public class LevelLighting : MonoBehaviour
{
    [SerializeField] private float globalIntensity = 0.1f;
    [SerializeField] private bool useGlobalColor = false;                       // aus = Farbe vom Global Light 2D bleibt
    [SerializeField] private Color globalColor = new Color32(255, 244, 220, 255);
    [SerializeField] private float playerLightRadius = 3f;
    [SerializeField] private bool disablePlayerLight = false;                  // bei Tageslicht aus (sonst Überstrahlung)

    void Start()
    {
        foreach (Light2D l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (l.lightType != Light2D.LightType.Global) continue;

            l.intensity = globalIntensity;
            if (useGlobalColor) l.color = globalColor;
        }

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc == null) return;

        Light2D playerLight = pc.GetComponentInChildren<Light2D>();
        if (playerLight == null) return;

        playerLight.pointLightOuterRadius = playerLightRadius;
        playerLight.enabled = !disablePlayerLight;
    }
}