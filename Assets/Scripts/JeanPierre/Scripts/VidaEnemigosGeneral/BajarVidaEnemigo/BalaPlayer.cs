// BalaPlayer.cs
using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    public enum TipoBala { Ametralladora, Pistola, Escopeta }
    public TipoBala tipoBala;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        var enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            hasHit = true;
            enemigo.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
            return;
        }

        var jefe = other.GetComponentInParent<VidaJefe>();
        if (jefe != null)
        {
            hasHit = true;
            // Ahora existe la sobrecarga que acepta (TipoBala, Collider):
            jefe.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
        }
    }
}


//// BalaPlayer.cs
//using UnityEngine;

//public class BalaPlayer : MonoBehaviour
//{
//    public enum TipoBala { Ametralladora, Pistola, Escopeta }
//    [Header("Tipo de bala (asignar en Inspector)")]
//    public TipoBala tipoBala;

//    private bool hasHit = false;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasHit) return;

//        var enemigo = other.GetComponent<VidaEnemigoGeneral>();
//        if (enemigo != null)
//        {
//            hasHit = true;
//            enemigo.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }

//        var jefe = other.GetComponentInParent<VidaJefe>();
//        if (jefe != null)
//        {
//            hasHit = true;
//            jefe.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//        }
//    }
//}





