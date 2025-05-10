
// BalaPlayer.cs
using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    public enum TipoBala { Ametralladora, Pistola, Escopeta }
    [Header("Tipo de bala (asignar en Inspector)")]
    public TipoBala tipoBala;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        var enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            hasHit = true;
            enemigo.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        var jefe = other.GetComponentInParent<VidaJefe>();
        if (jefe != null)
        {
            hasHit = true;
            jefe.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
        }
    }
}






//using UnityEngine;

//public class BalaPlayer : MonoBehaviour
//{
//    public enum TipoBala { Ametralladora, Pistola, Escopeta }
//    [Header("Tipo de bala (asignar en Inspector)")]
//    public TipoBala tipoBala;

//    private bool hasHit = false;  // evita colisiones dobles

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasHit) return;

//        // Intentar afectar enemigo genérico
//        var enemigo = other.GetComponent<VidaEnemigoGeneral>();
//        if (enemigo != null)
//        {
//            hasHit = true;
//            enemigo.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }

//        // Intentar afectar al jefe buscando en este objeto o en sus padres
//        var jefe = other.GetComponentInParent<VidaJefe>();
//        if (jefe != null)
//        {
//            hasHit = true;
//            // Reutilizamos la misma lógica de daño por tipo de bala
//            jefe.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }
//    }
//}




