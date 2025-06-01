using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    public enum TipoBala { Ametralladora, Pistola, Escopeta }
    public TipoBala tipoBala;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        // 1) Intentar encontrar Fase1Vida en el objeto golpeado o en sus padres
        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            hasHit = true;
            fase1.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        // 2) Si no hay Fase1Vida, buscar Fase2Vida en padres
        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            hasHit = true;
            fase2.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        // 3) Si no hay Fase2Vida, buscar Fase3Vida en padres
        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            hasHit = true;
            fase3.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        // 4) Buscar VidaEnemigoGeneral directamente en el objeto golpeado
        VidaEnemigoGeneral enemigo = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigo != null)
        {
            hasHit = true;
            enemigo.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
            return;
        }

        //// 5) Buscar VidaJefe en un padre (si en el futuro la necesitas)
        //VidaJefe jefe = BuscarComponenteEnPadres<VidaJefe>(other.transform);
        //if (jefe != null)
        //{
        //    hasHit = true;
        //    jefe.RecibirDanioPorBala(tipoBala, other);
        //    Destroy(gameObject);
        //}
    }

    // Busca un componente T en este transform o cualquiera de sus padres
    private T BuscarComponenteEnPadres<T>(Transform hijo) where T : Component
    {
        Transform actual = hijo;
        while (actual != null)
        {
            T componente = actual.GetComponent<T>();
            if (componente != null)
                return componente;
            actual = actual.parent;
        }
        return null;
    }
}

