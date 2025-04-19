// BalaPlayer.cs (sin cambios)
using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        var enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            // Le pasamos al enemigo el tag de esta bala
            enemigo.RecibirDanioPorBala(this.tag);
            Destroy(gameObject);
        }
    }
}
