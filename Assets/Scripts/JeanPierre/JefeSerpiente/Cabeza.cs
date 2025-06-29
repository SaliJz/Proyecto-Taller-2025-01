using UnityEngine;

public class Cabeza : MonoBehaviour
{
    [Header("Da�o al jugador")]
    [SerializeField] private int damageAmount = 10;

    [Header("Fuerza de expulsi�n")]
    [SerializeField] private float knockbackForce = 5f;

    private void OnTriggerEnter(Collider other)
    {
        AplicarDa�oYKnockback(other.gameObject, other.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Usamos el punto de contacto para calcular una expulsi�n m�s precisa
        Vector3 contactPoint = collision.GetContact(0).point;
        AplicarDa�oYKnockback(collision.gameObject, contactPoint);
    }

    private void AplicarDa�oYKnockback(GameObject obj, Vector3 contacto)
    {
        // 1) Aplica da�o si tiene PlayerHealth
        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }

        // 2) Calcula y aplica knockback si tiene Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Direcci�n desde el punto de contacto hacia afuera
            Vector3 direccion = (obj.transform.position - contacto).normalized;
            rb.AddForce(direccion * knockbackForce, ForceMode.Impulse);
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
//        AplicarDa�o(other.gameObject);
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        AplicarDa�o(collision.gameObject);
//    }

//    private void AplicarDa�o(GameObject obj)
//    {
//        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
//        if (playerHealth != null)
//        {
//            playerHealth.TakeDamage(damageAmount);
//        }
//    }
//}

