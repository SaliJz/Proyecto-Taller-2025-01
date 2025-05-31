using UnityEngine;

public class Cabeza : MonoBehaviour
{
    [Header("Daño al jugador")]
    [SerializeField] private int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        AplicarDaño(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        AplicarDaño(collision.gameObject);
    }

    private void AplicarDaño(GameObject obj)
    {
        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }
    }
}


//using UnityEngine;

//public class Cabeza : MonoBehaviour
//{
//    [Header("Daño al jugador")]
//    [SerializeField] private int damageAmount = 10;

//    private void OnTriggerEnter(Collider other)
//    {
//        // Intentamos obtener el componente PlayerHealth del objeto que entra
//        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
//        if (playerHealth != null)
//        {
//            // Restamos vida al jugador
//            playerHealth.TakeDamage(damageAmount);
//        }
//    }
//}
