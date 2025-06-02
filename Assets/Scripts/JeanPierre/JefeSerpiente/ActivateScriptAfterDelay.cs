using System.Collections;
using UnityEngine;

/// <summary>
/// Este componente permite asignar otro script (componente) para activar
/// despu�s de un tiempo de espera configurado.
/// </summary>
public class ActivateScriptAfterDelay : MonoBehaviour
{
    [Tooltip("El componente (script) que se activar� despu�s del retraso.")]
    public Behaviour scriptToActivate;

    [Tooltip("Tiempo de espera en segundos antes de activar el script.")]
    public float delay = 0.5f;

    private void Awake()
    {
        // Inicialmente desactivamos el script asignado si existe.
        if (scriptToActivate != null)
        {
            scriptToActivate.enabled = false;
        }
        else
        {
            Debug.LogWarning("No se ha asignado ning�n script para activar.", gameObject);
        }
    }

    private void Start()
    {
        // Iniciar la corrutina que activar� el script tras el retraso.
        StartCoroutine(ActivateAfterDelay());
    }

    private IEnumerator ActivateAfterDelay()
    {
        // Esperar el tiempo indicado.
        yield return new WaitForSeconds(delay);

        // Activar el script asignado.
        if (scriptToActivate != null)
        {
            scriptToActivate.enabled = true;
        }
    }
}
