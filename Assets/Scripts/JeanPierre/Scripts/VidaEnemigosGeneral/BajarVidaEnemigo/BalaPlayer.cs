using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Intenta obtener el componente VidaEnemigoGeneral del objeto con el que colisiona
        VidaEnemigoGeneral enemigo = other.GetComponent<VidaEnemigoGeneral>();

        // Si se encontró el componente, la bala ha golpeado a un enemigo
        if (enemigo != null)
        {
            // Aplica 30 puntos de daño al enemigo
            enemigo.RecibirDanio(30f);

            // Destruye la bala luego del impacto (opcional)
            Destroy(gameObject);
        }
    }
}
