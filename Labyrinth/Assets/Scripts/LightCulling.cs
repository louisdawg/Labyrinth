using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

// Schaltet Fackel-Lichter weit weg vom Spieler aus. Bei vielen Fackeln (Level 2) spart das viel Leistung.
// Auf das Objekt "LevelLighting" legen. Das Licht am Spieler und das Global Light bleiben immer an.
public class LightCulling : MonoBehaviour
{
    [SerializeField] private float activeDistance = 12f;   // ca. halbe Bildbreite + Lichtradius
    [SerializeField] private float checkInterval = 0.2f;

    private Light2D[] lights;
    private Transform player;
    private float timer;

    void Start()
    {
        PlayerController pc = FindFirstObjectByType<PlayerController>();
        if (pc != null) player = pc.transform;

        List<Light2D> list = new List<Light2D>();
        foreach (Light2D l in FindObjectsByType<Light2D>(FindObjectsSortMode.None))
        {
            if (l.lightType == Light2D.LightType.Global) continue;
            if (player != null && l.transform.IsChildOf(player)) continue;
            list.Add(l);
        }
        lights = list.ToArray();
    }

    void Update()
    {
        if (player == null) return;

        timer -= Time.deltaTime;
        if (timer > 0f) return;
        timer = checkInterval;

        float maxSqr = activeDistance * activeDistance;
        foreach (Light2D l in lights)
        {
            if (l == null) continue;
            bool near = ((Vector2)l.transform.position - (Vector2)player.position).sqrMagnitude <= maxSqr;
            if (l.enabled != near) l.enabled = near;
        }
    }
}