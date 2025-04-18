using System.Collections;
using UnityEngine;

public class AtaqueEnemigoAVidaPlayer : MonoBehaviour
{
    [SerializeField] private int damageAmount = 15;        // Cantidad de daño que se aplicará al jugador.
    [SerializeField] private float damageInterval = 1.5f; // Intervalo en segundos entre cada daño.

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
            // actualizamos el último “ping” de permanencia.
            lastStayTime = Time.time;
        }
    }

    // (Opcional) Si OnTriggerExit funciona correctamente, sigue estando aquí como refuerzo
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

            // Si hace más de damageInterval que no llegó ningún OnTriggerStay,
            // asumimos que el player ha salido y salimos de la corrutina.
            if (Time.time - lastStayTime > damageInterval)
                break;

            // Si sigue dentro, aplicamos siguiente golpe
            playerHealth.TakeDamage(damageAmount);
        }

        damageCoroutine = null;
    }
}



//using System.Collections;
//using UnityEngine;

//public class AtaqueEnemigoAVidaPlayer : MonoBehaviour
//{
//    [SerializeField] private int damageAmount = 15;        // Cantidad de daño que se aplicará al jugador.
//    [SerializeField] private float damageInterval = 1.5f;    // Intervalo en segundos entre cada daño.

//    private Coroutine damageCoroutine;

//    private void OnTriggerEnter(Collider other)
//    {
//        // Verifica si el objeto que entra tiene la etiqueta "Player"
//        if (other.CompareTag("Player"))
//        {
//            // Busca el componente PlayerHealth en el objeto Player
//            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
//            if (playerHealth != null)
//            {
//                // Inicia la corrutina para aplicar daño repetidamente
//                damageCoroutine = StartCoroutine(DamagePlayerRoutine(playerHealth));
//            }
//        }
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        // Si el objeto que sale es el jugador, se detiene la corrutina de daño
//        if (other.CompareTag("Player"))
//        {
//            if (damageCoroutine != null)
//            {
//                StopCoroutine(damageCoroutine);
//                damageCoroutine = null;
//            }
//        }
//    }

//    private IEnumerator DamagePlayerRoutine(PlayerHealth playerHealth)
//    {
//        // Aplica daño cada "damageInterval" segundos mientras el jugador se encuentre en el trigger.
//        while (true)
//        {
//            playerHealth.TakeDamage(damageAmount);
//            yield return new WaitForSeconds(damageInterval);
//        }
//    }
//}
