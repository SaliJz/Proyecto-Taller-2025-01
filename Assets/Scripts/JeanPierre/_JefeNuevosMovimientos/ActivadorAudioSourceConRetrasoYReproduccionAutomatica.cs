using UnityEngine;
using System.Collections;

[RequireComponent(typeof(AudioSource))]
public class ActivadorAudioSourceConRetrasoYReproduccionAutomatica : MonoBehaviour
{
    [Header("Configuración de Retraso")]
    [Tooltip("Tiempo en segundos que esperará antes de activar y reproducir el AudioSource")]
    public float tiempoDeRetraso = 5f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        // Asegurarse de que el AudioSource esté desactivado inicialmente
        audioSource.enabled = false;
    }

    private void Start()
    {
        // Iniciar la corrutina que maneja el retraso
        StartCoroutine(ActivarYReproducirAudioTrasRetraso());
    }

    private IEnumerator ActivarYReproducirAudioTrasRetraso()
    {
        // Espera el tiempo configurado
        yield return new WaitForSeconds(tiempoDeRetraso);

        // Activa el componente AudioSource
        audioSource.enabled = true;

        // Opcional: reproducir inmediatamente
        audioSource.Play();
    }
}
