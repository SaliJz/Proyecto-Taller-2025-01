using UnityEngine;

public class DestruirEnTiempo : MonoBehaviour
{
    [Header("Tiempo antes de destruirse (segundos)")]
    public float tiempoDeDestruccion = 1.3f;

    void Start()
    {
        Destroy(gameObject, tiempoDeDestruccion);
    }
}
