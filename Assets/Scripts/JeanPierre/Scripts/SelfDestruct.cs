using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    [Header("Configuración de Autodestrucción")]
    [Tooltip("Tiempo en segundos antes de que este objeto se destruya")]
    public float lifeTime = 5f;

    void Start()
    {
        // Programa la destrucción de este GameObject pasados lifeTime segundos
        Destroy(gameObject, lifeTime);
    }
}
