using UnityEngine;

public class Cabeza : MonoBehaviour
{
    [Header("Da�o al jugador")]
    [SerializeField] private int damageAmount = 10;

    private void OnTriggerEnter(Collider other)
    {
        AplicarDa�o(other.gameObject);
    }

    private void OnCollisionEnter(Collision collision)
    {
        AplicarDa�o(collision.gameObject);
    }

    private void AplicarDa�o(GameObject obj)
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
//    [Header("Da�o al jugador")]
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
