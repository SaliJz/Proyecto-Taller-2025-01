

using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    // Fuerza de la explosi�n.
    public float explosionForce = 10f;

    // Radio de acci�n de la explosi�n (se usa si se utiliza AddExplosionForce).
    public float explosionRadius = 5f;

    // Modificador vertical para dar un peque�o 'salto'.
    public float upwardModifier = 0f;

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el GameObject que entra tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            // Intenta obtener el componente Rigidbody del jugador
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Calcula la direcci�n de explosi�n: del centro del objeto que contiene el trigger hacia el jugador.
                Vector3 direction = other.transform.position - transform.position;

                // Aplica la fuerza en la direcci�n opuesta (hacia fuera)
                // Opci�n 1: Aplicar fuerza de forma manual (como impulso)
                rb.AddForce(direction.normalized * explosionForce, ForceMode.Impulse);

                // Opci�n 2 (alternativa, descomentar si se prefiere):
                // Utiliza el m�todo AddExplosionForce para simular la explosi�n.
                // rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);
            }
        }
    }
}
