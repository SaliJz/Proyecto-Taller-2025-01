using UnityEngine;

public class ProyectilEnemigoResponsableDeInfligirDanoAlJugador : MonoBehaviour
{
    [Tooltip("Daño que inflige la bala al jugador")]
    public int damage = 20;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph != null)
            {
                // Ahora pasamos también la posición del proyectil como attackerPosition
                ph.TakeDamage(damage, transform.position);
            }
            Destroy(gameObject);
        }
    }
}
