using System.Collections;
using UnityEngine;

public class FadeTransition : MonoBehaviour
{
    [Header("Componentes")]
    public CanvasGroup canvasGroup;

    [Header("Configuración")]
    public float fadeDuration = 1f;
    public float delayBeforeFade = 1f;
    public bool isFadeIn = true;
    TutorialManager0 manager;
    private int currentDialogue;

    void Start()
    {
        manager = TutorialManager0.Instance;
        StartCoroutine(FadeOut());
    }

    public IEnumerator FadeOut()
    {
        yield return new WaitForSecondsRealtime(delayBeforeFade);

        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
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
        yield return new WaitForSecondsRealtime(delayBeforeFade);
        float elapsed = 0f;
        float startAlpha = canvasGroup.alpha;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
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
        yield return new WaitForSecondsRealtime(waitBetween);
        
        if (manager.currentDialogueIndex==5)
        {
            manager.ConfirmAdvance();
        }
        yield return StartCoroutine(FadeOut());
    }
}
