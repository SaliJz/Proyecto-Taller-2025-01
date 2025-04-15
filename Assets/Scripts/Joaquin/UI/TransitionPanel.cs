using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float durationFade; // Duración del fade-out
    [SerializeField] private float delayStart; // Tiempo de espera antes de iniciar el fade-out

    private void Awake()
    {
        gameObject.SetActive(true);
        if (canvasGroup != null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        // Iniciar la corutina de retraso
        StartCoroutine(WaitAndStartFade());
    }

    // Corutina para esperar antes de iniciar el fade-out
    private IEnumerator WaitAndStartFade()
    {
        yield return new WaitForSeconds(delayStart);
        StartCoroutine(FadeOut());
    }

    // Corutina para reducir la opacidad del panel
    private IEnumerator FadeOut()
    {
        canvasGroup.alpha = 1f;
        float t = 0f;

        // Mientras el tiempo sea menor que la duración, reducir opacidad
        while (t < durationFade)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / durationFade);
            yield return null;
        }

        // Asegurarse de que el alpha es 0 al finalizar
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
