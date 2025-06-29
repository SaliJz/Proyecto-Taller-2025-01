using UnityEngine;

public class Cabeza : MonoBehaviour
{
    [Header("Daño al jugador")]
    [SerializeField] private int damageAmount = 10;

    [Header("Fuerza de expulsión")]
    [SerializeField] private float knockbackForce = 5f;

    private void OnTriggerEnter(Collider other)
    {
        AplicarDañoYKnockback(other.gameObject, other.transform.position);
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Usamos el punto de contacto para calcular una expulsión más precisa
        Vector3 contactPoint = collision.GetContact(0).point;
        AplicarDañoYKnockback(collision.gameObject, contactPoint);
    }

    private void AplicarDañoYKnockback(GameObject obj, Vector3 contacto)
    {
        // 1) Aplica daño si tiene PlayerHealth
        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damageAmount);
        }

        // 2) Calcula y aplica knockback si tiene Rigidbody
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            // Dirección desde el punto de contacto hacia afuera
            Vector3 direccion = (obj.transform.position - contacto).normalized;
            rb.AddForce(direccion * knockbackForce, ForceMode.Impulse);
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
//        AplicarDaño(other.gameObject);
//    }

//    private void OnCollisionEnter(Collision collision)
//    {
//        AplicarDaño(collision.gameObject);
//    }

//    private void AplicarDaño(GameObject obj)
//    {
//        PlayerHealth playerHealth = obj.GetComponent<PlayerHealth>();
//        if (playerHealth != null)
//        {
//            playerHealth.TakeDamage(damageAmount);
//        }
//    }
//}

