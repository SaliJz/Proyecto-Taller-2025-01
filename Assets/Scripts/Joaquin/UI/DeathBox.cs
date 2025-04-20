using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
    [SerializeField] private int damageAmount = 1000; // Daño configurable desde el Inspector

    private void OnCollisionEnter(Collision collision)
    {
        // Verificar si el objeto colisionado tiene el tag "Player"
        if (collision.gameObject.CompareTag("Player"))
        {
            // Intentar obtener el componente PlayerHealth
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Aplicar daño al jugador
                playerHealth.TakeDamage(damageAmount);
                Debug.Log($"Player hit the death box! Applied {damageAmount} damage to {collision.gameObject.name}.");
            }
            else
            {
                Debug.LogWarning($"El objeto con tag 'Player' no tiene el componente PlayerHealth: {collision.gameObject.name}");
            }
        }
    }
}
