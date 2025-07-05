using System.Collections;
using UnityEngine;

public class ActivateAfterDelay : MonoBehaviour
{
    [Header("Objeto a activar")]
    [Tooltip("Referencia al GameObject que está desactivado.")]
    [SerializeField] private GameObject targetObject;

    [Header("Tiempo de espera")]
    [Tooltip("Segundos que esperará antes de activar el objeto.")]
    [SerializeField] private float delayInSeconds = 2f;

    void Start()
    {
        // Inicia la corrutina automáticamente al arrancar la escena
        StartCoroutine(ActivateTarget());
    }

    /// <summary>
    /// Publica este método si prefieres activar manualmente desde otro script o evento.
    /// </summary>
    public void ActivateWithDelay()
    {
        StartCoroutine(ActivateTarget());
    }

    private IEnumerator ActivateTarget()
    {
        // Espera el tiempo configurado
        yield return new WaitForSeconds(delayInSeconds);

        // Activa el GameObject
        if (targetObject != null)
            targetObject.SetActive(true);
        else
            Debug.LogWarning("ActivateAfterDelay: targetObject no está asignado.", this);
    }
}
