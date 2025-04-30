using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [Header("Fade Elements")]
    [SerializeField] private GameObject loadingIcon;
    [SerializeField] private GameObject loadingText;
    [SerializeField] private float fadeDuration = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
    }

    public void LoadSceneWithFade(string sceneName)
    {
        gameObject.SetActive(true); // Asegúrate de que el objeto está activo
        
        MenuPausa menuPausa = FindObjectOfType<MenuPausa>();

        if (menuPausa != null)
        {
            menuPausa.SetIsDead(true); // Desactiva el menú de pausa
        }
        StartCoroutine(LoadSceneCoroutine(sceneName));
    }

    private IEnumerator LoadSceneCoroutine(string sceneName)
    {
        // Desactiva el juego
        Time.timeScale = 0f;
        canvasGroup.alpha = 0f;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime; // Ignora timeScale = 0
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(1f); // espera opcional en pantalla

        Time.timeScale = 1f; // restaurar tiempo antes de cambiar de escena
        SceneManager.LoadScene(sceneName);
    }
}
