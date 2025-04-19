// BalaPlayer.cs
using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    private bool hasHit = false;  // evita colisiones dobles

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        VidaEnemigoGeneral enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            hasHit = true;
            enemigo.RecibirDanioPorBala(this.tag);
            Destroy(gameObject);
        }
    }
}
