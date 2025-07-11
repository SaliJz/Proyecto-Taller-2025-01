using UnityEngine;

public class BalaPlayer : MonoBehaviour
{
    public enum TipoBala { Ametralladora, Pistola, Escopeta }
    public TipoBala tipoBala;

    private bool hasHit = false;

    private void OnTriggerEnter(Collider other)
    {
        if (hasHit) return;

        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
        if (fase1 != null)
        {
            hasHit = true;
            fase1.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
        if (fase2 != null)
        {
            hasHit = true;
            fase2.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
        if (fase3 != null)
        {
            hasHit = true;
            fase3.RecibirDanioPorBala(tipoBala);
            Destroy(gameObject);
            return;
        }

        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
        if (enemigoPistola != null)
        {
            hasHit = true;
            enemigoPistola.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
            return;
        }

        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
        if (enemigoGeneral != null)
        {
            hasHit = true;
            enemigoGeneral.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
            return;
        }

        // Nuevo: EnemigoRosa
        EnemigoRosa enemigoRosa = other.GetComponent<EnemigoRosa>();
        if (enemigoRosa != null)
        {
            hasHit = true;
            enemigoRosa.RecibirDanioPorBala(tipoBala, other);
            Destroy(gameObject);
            return;
        }
    }

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


//using UnityEngine;

//public class BalaPlayer : MonoBehaviour
//{
//    public enum TipoBala { Ametralladora, Pistola, Escopeta }
//    public TipoBala tipoBala;

//    private bool hasHit = false;

//    private void OnTriggerEnter(Collider other)
//    {
//        if (hasHit) return;

//        // 1) Intentar encontrar Fase1Vida en el objeto golpeado o en sus padres
//        Fase1Vida fase1 = BuscarComponenteEnPadres<Fase1Vida>(other.transform);
//        if (fase1 != null)
//        {
//            hasHit = true;
//            fase1.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }

//        // 2) Si no hay Fase1Vida, buscar Fase2Vida en padres
//        Fase2Vida fase2 = BuscarComponenteEnPadres<Fase2Vida>(other.transform);
//        if (fase2 != null)
//        {
//            hasHit = true;
//            fase2.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }

//        // 3) Si no hay Fase2Vida, buscar Fase3Vida en padres
//        Fase3Vida fase3 = BuscarComponenteEnPadres<Fase3Vida>(other.transform);
//        if (fase3 != null)
//        {
//            hasHit = true;
//            fase3.RecibirDanioPorBala(tipoBala);
//            Destroy(gameObject);
//            return;
//        }

//        // 4) Buscar EnemigoPistolaTutorial directamente en el objeto golpeado
//        EnemigoPistolaTutorial enemigoPistola = other.GetComponent<EnemigoPistolaTutorial>();
//        if (enemigoPistola != null)
//        {
//            hasHit = true;
//            enemigoPistola.RecibirDanioPorBala(tipoBala, other);
//            Destroy(gameObject);
//            return;
//        }

//        // 5) Buscar VidaEnemigoGeneral directamente en el objeto golpeado
//        VidaEnemigoGeneral enemigoGeneral = other.GetComponent<VidaEnemigoGeneral>();
//        if (enemigoGeneral != null)
//        {
//            hasHit = true;
//            enemigoGeneral.RecibirDanioPorBala(tipoBala, other);
//            Destroy(gameObject);
//            return;
//        }
//    }

//    // Busca un componente T en este transform o cualquiera de sus padres
//    private T BuscarComponenteEnPadres<T>(Transform hijo) where T : Component
//    {
//        Transform actual = hijo;
//        while (actual != null)
//        {
//            T componente = actual.GetComponent<T>();
//            if (componente != null)
//                return componente;
//            actual = actual.parent;
//        }
//        return null;
//    }
//}

