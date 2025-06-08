using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Header("Configuraci�n de Autodestrucci�n")]
    [Tooltip("Tiempo en segundos antes de que este objeto se destruya")]
    public float lifeTime = 5f;

    void Start()
    {
        // Programa la destrucci�n de este GameObject pasados lifeTime segundos
        Destroy(gameObject, lifeTime);
    }
}
