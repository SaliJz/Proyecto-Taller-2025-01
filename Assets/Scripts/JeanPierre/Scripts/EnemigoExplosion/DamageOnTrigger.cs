using UnityEngine;

public class DamageOnTrigger : MonoBehaviour
{
    // Cantidad de daño a infligir
    public float danio = 1000f;

    // Este método se ejecuta cuando otro collider entra en el trigger de este objeto
    void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que colisionó tiene el tag "Enemigo"
        if (other.CompareTag("Enemigo"))
        {
            // Intenta obtener el componente VidaEnemigoGeneral del objeto colisionado
            VidaEnemigoGeneral enemigoVida = other.GetComponent<VidaEnemigoGeneral>();

            // Si el componente existe, se le aplica el daño
            if (enemigoVida != null)
            {
                enemigoVida.RecibirDanio(danio);
            }
            else
            {
                Debug.LogWarning("El objeto con tag 'Enemigo' no tiene el componente VidaEnemigoGeneral.");
            }
        }
    }
}
