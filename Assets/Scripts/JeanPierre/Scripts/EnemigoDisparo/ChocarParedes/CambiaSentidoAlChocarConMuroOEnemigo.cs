// CambiaSentidoAlChocarConMuroOEnemigo.cs
using UnityEngine;

[RequireComponent(typeof(EnemigoMovimientoDisparador))]
public class CambiaSentidoAlChocarConMuroOEnemigo : MonoBehaviour
{
    private EnemigoMovimientoDisparador movimiento;

    private void Awake()
    {
        movimiento = GetComponent<EnemigoMovimientoDisparador>();
    }

    //private void OnCollisionEnter(Collision collision)
    //{
    //    // Si choca con un muro o con otro enemigo, invierte el sentido
    //    if (collision.gameObject.CompareTag("Wall") ||
    //        collision.gameObject.CompareTag("Enemy"))
    //    {
    //        movimiento.InvertirSentido();
    //    }
    //}

    // Si prefieres usar trigger en vez de colisión física:

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Enemy"))
        {
            movimiento.InvertirSentido();
        }
    }

}
