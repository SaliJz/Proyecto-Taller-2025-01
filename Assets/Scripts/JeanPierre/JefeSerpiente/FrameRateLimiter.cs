using System.Threading;
using UnityEngine;

/// <summary>
/// Fuerza un límite de FPS usando Sleep para rellenar
/// el tiempo sobrante de cada frame.
/// Attach este script a un GameObject activo en la escena.
/// </summary>
public class FrameRateLimiter : MonoBehaviour
{
    [Tooltip("FPS máximo que permitirá la aplicación")]
    [SerializeField] private int targetFPS = 36;

    private float frameInterval;
    private float lastFrameTime;

    private void Awake()
    {
        // Desactiva VSync para que Application.targetFrameRate surta efecto
        QualitySettings.vSyncCount = 0;

        // Establece el límite de FPS (funciona en build y en Play Mode del Editor)
        Application.targetFrameRate = targetFPS;

        // Calcula el intervalo de cada frame
        frameInterval = 1f / targetFPS;
        lastFrameTime = Time.unscaledTime;

        Debug.Log($"[FrameRateLimiter] Objetivo: {targetFPS} FPS (intervalo {frameInterval:F4}s)");
    }

    private void LateUpdate()
    {
        // Tiempo transcurrido desde el último frame
        float now = Time.unscaledTime;
        float elapsed = now - lastFrameTime;
        float toWait = frameInterval - elapsed;

        // Si hay tiempo sobrante, duerme el hilo principal
        if (toWait > 0f)
        {
            int ms = Mathf.CeilToInt(toWait * 1000f);
            Thread.Sleep(ms);
        }

        // Reinicia el contador para el próximo frame
        lastFrameTime = Time.unscaledTime;
    }
}
