using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class UltraSmoothBlackScreenSceneTransitionFadeController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Asigna aqu� tu Image de color negro (debe estar activa o se activar� en Start).")]
    public Image blackScreenImage;

    [Header("Transici�n")]
    [Tooltip("Duraci�n del fundido en segundos.")]
    public float fadeDurationInSeconds = 1f;

    private void Start()
    {
        if (blackScreenImage == null)
        {
            Debug.LogError("UltraSmoothBlackScreenSceneTransitionFadeController: falta asignar blackScreenImage en el Inspector.");
            return;
        }

        // Aseguramos que la imagen est� activa y empiece transparente
        blackScreenImage.gameObject.SetActive(true);
        Color col = blackScreenImage.color;
        col.a = 0f;
        blackScreenImage.color = col;

        // Iniciamos la transici�n inmediatamente al arrancar el script
        StartCoroutine(FadeInBlackScreen());
    }

    private IEnumerator FadeInBlackScreen()
    {
        float elapsed = 0f;
        Color col = blackScreenImage.color;

        while (elapsed < fadeDurationInSeconds)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Clamp01(elapsed / fadeDurationInSeconds);
            blackScreenImage.color = new Color(col.r, col.g, col.b, alpha);
            yield return null;
        }

        // Aseguramos alpha = 1 al terminar
        blackScreenImage.color = new Color(col.r, col.g, col.b, 1f);

        // Ejecutamos la acci�n tras completar el fundido
        GameManager.Instance?.OnBossDefeated();
        SceneManager.LoadScene("Creditos");
    }
}
