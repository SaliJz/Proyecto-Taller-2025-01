using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathBox : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();

        if (collision.gameObject.CompareTag("Player"))
        {
            // Verifica si el jugador ha colisionado con la caja de muerte
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(1000); // Aplica daño al jugador
                Debug.Log("Player hit the death box!");
            }
        }
    }
}
