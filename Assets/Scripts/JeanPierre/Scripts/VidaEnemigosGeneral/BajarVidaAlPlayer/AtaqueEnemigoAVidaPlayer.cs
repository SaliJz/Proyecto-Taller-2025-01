using System.Collections;
using UnityEngine;

public class AtaqueEnemigoAVidaPlayer : MonoBehaviour
{
    [SerializeField] private int damageAmount = 15;        // Cantidad de da�o que se aplicar� al jugador.
    [SerializeField] private float damageInterval = 1.5f;    // Intervalo en segundos entre cada da�o.

    private Coroutine damageCoroutine;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entra tiene la etiqueta "Player"
        if (other.CompareTag("Player"))
        {
            // Busca el componente PlayerHealth en el objeto Player
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                // Inicia la corrutina para aplicar da�o repetidamente
                damageCoroutine = StartCoroutine(DamagePlayerRoutine(playerHealth));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Si el objeto que sale es el jugador, se detiene la corrutina de da�o
        if (other.CompareTag("Player"))
        {
            if (damageCoroutine != null)
            {
                StopCoroutine(damageCoroutine);
                damageCoroutine = null;
            }
        }
    }

    private IEnumerator DamagePlayerRoutine(PlayerHealth playerHealth)
    {
        // Aplica da�o cada "damageInterval" segundos mientras el jugador se encuentre en el trigger.
        while (true)
        {
            playerHealth.TakeDamage(damageAmount);
            yield return new WaitForSeconds(damageInterval);
        }
    }
}
