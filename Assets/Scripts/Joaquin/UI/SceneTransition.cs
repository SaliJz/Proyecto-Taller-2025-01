using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Elements")]
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private Color fadeColor = Color.white;
    [SerializeField] private Image fadeImage;

    [Header("Elementos de la Tienda")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private Button continueButton;

    private CanvasGroup canvasGroup;
    private string sceneToLoad;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (fadeImage != null) fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0);
        if (shopUI != null) shopUI.SetActive(false);
    }


    public void LoadSceneWithFade(string sceneName)
    {
        gameObject.SetActive(true);
        
        MenuPausa menuPausa = FindObjectOfType<MenuPausa>();

        if (menuPausa != null)
        {
            menuPausa.SetIsDead(true);
        }
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator TransitionToShopRoutine()
    {
        FindObjectOfType<PlayerMovement>().enabled = false;
        FindObjectOfType<MoveCamera>().enabled = false;
        FindObjectOfType<PlayerDash>().enabled = false;

        Weapon[] weapons = Resources.FindObjectsOfTypeAll<Weapon>().ToArray();
        foreach (var weapon in weapons)
        {
            weapon.enabled = false;
        }

        yield return Fade(1f);

        if (shopUI != null) shopUI.SetActive(true);

        yield return Fade(0f);
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        Time.timeScale = 0f;
        canvasGroup.alpha = 0f;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f);

        Time.timeScale = 1f;
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator Fade(float targetAlpha)
    {
        canvasGroup.alpha = 1 - targetAlpha;
        fadeImage.gameObject.SetActive(true);

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(canvasGroup.alpha, targetAlpha, t / fadeDuration);
            fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, alpha);
            yield return null;
        }
        fadeImage.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, targetAlpha);

        if (targetAlpha == 0)
        {
            fadeImage.gameObject.SetActive(false);
        }
    }
}