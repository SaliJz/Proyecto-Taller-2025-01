using System.Collections;
using UnityEngine;

public class TransitionPanel : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float totalDuration = 3f;       // Tiempo total del efecto
    [SerializeField] private float initialInterval = 0.1f;    // Parpadeo más rápido
    [SerializeField] private float finalInterval = 0.5f;      // Parpadeo más lento
    [SerializeField] private float delayBeforeBlink = 0.5f;   // Espera antes de empezar

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        canvasGroup.alpha = 1f;
        gameObject.SetActive(true);
        StartCoroutine(BlinkWithIncreasingDelay());
    }

    private IEnumerator BlinkWithIncreasingDelay()
    {
        // 1. Espera inicial
        yield return new WaitForSeconds(delayBeforeBlink);

        // 2. Parpadeo con tiempo creciente
        float elapsed = 0f;

        while (elapsed < totalDuration)
        {
            // Calcular t: qué tan avanzado va (0 a 1)
            float t = elapsed / totalDuration;

            // Interpolar el tiempo entre parpadeos (más lento con el tiempo)
            float interval = Mathf.Lerp(initialInterval, finalInterval, t);

            // Parpadeo OFF
            canvasGroup.alpha = 0f;
            yield return new WaitForSeconds(interval);
            elapsed += interval;

            // Parpadeo ON
            canvasGroup.alpha = 1f;
            yield return new WaitForSeconds(interval);
            elapsed += interval;
        }

        // 3. Apagar al final
        canvasGroup.alpha = 0f;
        gameObject.SetActive(false);
    }
}
