
using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    public enum TipoBala { Ametralladora, Pistola, Escopeta }
    [Header("Tipo de bala (asignar en Inspector)")]
    public TipoBala tipoBala;

    private bool hasHit = false;  // evita colisiones dobles

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        VidaEnemigoGeneral enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            hasHit = true;
            enemigo.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
        }
    }
}
































//using UnityEngine;

//public class BalaPlayer : MonoBehaviour
//{
//    private bool hasHit = false;  // evita colisiones dobles

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasHit) return;

//        VidaEnemigoGeneral enemigo = other.GetComponent<VidaEnemigoGeneral>();
//        if (enemigo != null)
//        {
//            hasHit = true;
//            enemigo.RecibirDanioPorBala(this.tag);
//            Destroy(gameObject);
//        }
//    }
//}
