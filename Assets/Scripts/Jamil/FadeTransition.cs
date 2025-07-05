using UnityEngine;
using System.Collections;

public class FadeTransition : MonoBehaviour
{
    [Header("Componentes")]
    public CanvasGroup canvasGroup;

    [Header("Configuración")]
    public float fadeDuration = 1f;
    public float delayBeforeFade = 1f;
    public bool isFadeIn = true;

    void Start()
    {
       StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSeconds(delayBeforeFade);

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float easedT = t * t;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, easedT);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    public IEnumerator FadeIn()
    {
        isFadeIn = true;
        yield return new WaitForSeconds(delayBeforeFade);
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;
            float easedT = t * t;

            canvasGroup.alpha = Mathf.Lerp(startAlpha, 1f, easedT);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        isFadeIn = false;
    }

    public IEnumerator FadeInOut(float waitBetween)
    {      
        yield return StartCoroutine(FadeIn());
        yield return new WaitForSeconds(waitBetween);
        yield return StartCoroutine(FadeOut());
    }
}
