using TMPro;
using UnityEngine;

public class FPSText : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fpsText;
    [SerializeField, Tooltip("Intervalo de actualización en segundos")]
    private float updateInterval = 0.5f;

    private float accum;
    private int frames;
    private float timer;
    private int lastFPS;

    void Start()
    {
        if (fpsText == null)
            fpsText = GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        if (!fpsText.gameObject.activeSelf) return;

        accum += Time.unscaledDeltaTime;
        frames++;

        timer += Time.unscaledDeltaTime;
        if (timer >= updateInterval)
        {
            int fps = Mathf.RoundToInt(frames / accum);
            if (fps != lastFPS)
            {
                fpsText.text = $"FPS: {fps}";
                lastFPS = fps;
            }
            timer = 0f;
            accum = 0f;
            frames = 0;
        }
    }
}