

using UnityEngine;

public class ExplosionTrigger : MonoBehaviour
{
    // Fuerza de la explosión.
    public float explosionForce = 10f;

    // Radio de acción de la explosión (se usa si se utiliza AddExplosionForce).
    public float explosionRadius = 5f;

    // Modificador vertical para dar un pequeño 'salto'.
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
                // Calcula la dirección de explosión: del centro del objeto que contiene el trigger hacia el jugador.
                Vector3 direction = other.transform.position - transform.position;

                // Aplica la fuerza en la dirección opuesta (hacia fuera)
                // Opción 1: Aplicar fuerza de forma manual (como impulso)
                rb.AddForce(direction.normalized * explosionForce, ForceMode.Impulse);

                // Opción 2 (alternativa, descomentar si se prefiere):
                // Utiliza el método AddExplosionForce para simular la explosión.
                // rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, upwardModifier, ForceMode.Impulse);
            }
        }
    }
}
