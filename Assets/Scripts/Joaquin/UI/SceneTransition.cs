using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("Elementos de Transición")]
    [SerializeField] private float fadeDuration = 1.0f;
    [SerializeField] private Image fadeImage;

    private Coroutine activeFade;

    private void Awake()
    {
        if (fadeImage != null)
        {
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, 0);
            fadeImage.gameObject.SetActive(false);
        }
    }

    public void LoadSceneWithFade(string sceneName)
    {
        if (activeFade != null) StopCoroutine(activeFade);
        activeFade = StartCoroutine(LoadSceneRoutine(sceneName));
    }

    public Coroutine Fade(float targetAlpha)
    {
        if (activeFade != null) StopCoroutine(activeFade);
        activeFade = StartCoroutine(FadeRoutine(targetAlpha));
        return activeFade;
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        fadeImage.gameObject.SetActive(true);
        float startAlpha = fadeImage.color.a;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t / fadeDuration);
            fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, alpha);
            yield return null;
        }

        fadeImage.color = new Color(fadeImage.color.r, fadeImage.color.g, fadeImage.color.b, targetAlpha);
        if (targetAlpha == 0)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        yield return Fade(1f);
        SceneManager.LoadScene(sceneName);
    }
}