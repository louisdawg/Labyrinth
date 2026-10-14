using UnityEngine;
using UnityEngine.Rendering.Universal;

// Pro Szene einstellbar: Grundhelligkeit und Sichtradius des Spielers.
// Auf ein leeres Objekt "LevelLighting" in jeder Levelszene legen.
public class LevelLighting : MonoBehaviour
{
    [SerializeField] private float globalIntensity = 0.1f;
    [SerializeField] private float playerLightRadius = 3f;

    void Start()
    {
        foreach (Light2D l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (l.lightType == Light2D.LightType.Global) l.intensity = globalIntensity;
        }

        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc == null) return;

        Light2D playerLight = pc.GetComponentInChildren<Light2D>();
        if (playerLight != null) playerLight.pointLightOuterRadius = playerLightRadius;
    }
}