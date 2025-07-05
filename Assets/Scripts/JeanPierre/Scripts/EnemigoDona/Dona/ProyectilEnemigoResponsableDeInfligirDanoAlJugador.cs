using UnityEngine;

public class ProyectilEnemigoResponsableDeInfligirDanoAlJugador : MonoBehaviour
{
    [Tooltip("Da�o que inflige la bala al jugador")]
    public int damage = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Ahora pasamos tambi�n la posici�n del proyectil como attackerPosition
                ph.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
    }
}
