using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(SpriteRenderer))]
public class TorchFlicker : MonoBehaviour
{
    [SerializeField] Sprite[] frames;
    [SerializeField] float fps = 8f;
    [SerializeField] Light2D torchLight;
    [SerializeField] float baseIntensity = 1.2f;
    [SerializeField] float flickerAmount = 0.2f;
    [SerializeField] float flickerSpeed = 3f;

    SpriteRenderer sr;
    float offset;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        offset = Random.value * 100f;
    }

    void Update()
    {
        if (frames != null && frames.Length > 0)
        {
            int i = (int)((Time.time + offset) * fps) % frames.Length;
            sr.sprite = frames[i];
        }

        if (torchLight != null)
        {
            float n = Mathf.PerlinNoise(offset, Time.time * flickerSpeed);
            torchLight.intensity = baseIntensity + (n - 0.5f) * 2f * flickerAmount;
        }
    }
}