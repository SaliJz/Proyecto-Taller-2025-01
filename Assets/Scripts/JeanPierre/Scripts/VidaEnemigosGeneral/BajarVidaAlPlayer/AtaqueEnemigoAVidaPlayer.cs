using System.Collections;
using UnityEngine;

public class AtaqueEnemigoAVidaPlayer : MonoBehaviour
{
    [SerializeField] private int damageAmount = 15;        // Cantidad de da�o que se aplicar� al jugador.
    [SerializeField] private float damageInterval = 1.5f; // Intervalo en segundos entre cada da�o.

    private Coroutine damageCoroutine;
    private float lastStayTime;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && damageCoroutine == null)
            {
                // Inicializamos el timestamp y arrancamos la corrutina
                lastStayTime = Time.time;
                damageCoroutine = StartCoroutine(DamagePlayerRoutine(playerHealth));
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Cada vez que Unity nos diga que el Player sigue dentro,
            // actualizamos el �ltimo �ping� de permanencia.
            lastStayTime = Time.time;
        }
    }

    // (Opcional) Si OnTriggerExit funciona correctamente, sigue estando aqu� como refuerzo
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && damageCoroutine != null)
        {
            StopCoroutine(damageCoroutine);
            damageCoroutine = null;
        }
    }

    private IEnumerator DamagePlayerRoutine(PlayerHealth playerHealth)
    {
        // Primer golpe inmediato
        playerHealth.TakeDamage(damageAmount);

        while (true)
        {
            yield return new WaitForSeconds(damageInterval);

            // Si hace m�s de damageInterval que no lleg� ning�n OnTriggerStay,
            // asumimos que el player ha salido y salimos de la corrutina.
            if (Time.time - lastStayTime > damageInterval)
                break;

            // Si sigue dentro, aplicamos siguiente golpe
            playerHealth.TakeDamage(damageAmount);
        }

        damageCoroutine = null;
    }
}

