using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UltraSmoothBlackScreenSceneTransitionFadeController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Asigna aquí tu Image de color negro (debe estar desactivada al iniciar).")]
    public Image blackScreenImage;

    [Header("Transición")]
    [Tooltip("Duración del fundido en segundos.")]
    public float fadeDurationInSeconds = 1f;
    [Tooltip("Marca este bool en el Inspector (o desde otro script) para iniciar la transición.")]
    public bool triggerSceneTransitionFade = false;

    private bool isFading = false;

    private void Start()
    {
        if (blackScreenImage != null)
        {
            blackScreenImage.gameObject.SetActive(false);
            Color col = blackScreenImage.color;
            col.a = 0f;
            blackScreenImage.color = col;
        }
        else
        {
            Debug.LogError("UltraSmoothBlackScreenSceneTransitionFadeController: falta asignar blackScreenImage en el Inspector.");
        }
    }

    private void Update()
    {
        if (triggerSceneTransitionFade && !isFading)
        {
            StartCoroutine(FadeInBlackScreen());
            // Si prefieres que el bool se resetee automáticamente:
            // triggerSceneTransitionFade = false;
        }
    }

    private IEnumerator FadeInBlackScreen()
    {
        isFading = true;
        blackScreenImage.gameObject.SetActive(true);

        float elapsed = 0f;
        Color col = blackScreenImage.color;

        while (elapsed < fadeDurationInSeconds)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDurationInSeconds);
            blackScreenImage.color = new Color(col.r, col.g, col.b, alpha);
            yield return null;
        }

        blackScreenImage.color = new Color(col.r, col.g, col.b, 1f);
    }
}
