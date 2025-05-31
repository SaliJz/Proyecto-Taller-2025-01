using UnityEngine;

public class Fase3Vida : MonoBehaviour
{
    [Header("Vida total de la Cobra Elevada")]
    [Tooltip("Vida acumulada de todas las interacciones.")]
    public int vida = 300;

    [Header("Daño por tipo de bala")]
    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
    public int danioAmetralladora = 15;
    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
    public int danioPistola = 20;
    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    [Tooltip("Arrastra aquí los componentes que quieras habilitar cuando la Cobra Elevada muera.")]
    public MonoBehaviour[] scriptsToActivate;

    [Header("Scripts a eliminar al morir")]
    [Tooltip("Arrastra aquí los componentes que quieras destruir cuando la Cobra Elevada muera.")]
    public MonoBehaviour[] scriptsToRemove;

    private bool isDead = false;
    private CobraElevadaController cobraController;
    private TipoColorController tipoColorController;

    void Start()
    {
        // Intentamos obtener el componente CobraElevadaController en este GameObject
        cobraController = GetComponent<CobraElevadaController>();
        if (cobraController == null)
        {
            Debug.LogError("[Fase3Vida] No se encontró CobraElevadaController en este GameObject.");
        }

        // Intentamos obtener el TipoColorController para el parpadeo
        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
        {
            Debug.LogWarning("[Fase3Vida] No se encontró TipoColorController. El parpadeo no funcionará.");
        }
    }

    /// <summary>
    /// Método público que puede ser llamado desde BalaPlayer al impactar la Cobra Elevada.
    /// Resta vida según el tipo de bala y, cuando llegue a 0, activa/elimina scripts, aplica parpadeo y destruye el GameObject.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        if (isDead) return;

        int danioAplicado = 0;
        switch (tipoBala)
        {
            case BalaPlayer.TipoBala.Ametralladora:
                danioAplicado = danioAmetralladora;
                break;
            case BalaPlayer.TipoBala.Pistola:
                danioAplicado = danioPistola;
                break;
            case BalaPlayer.TipoBala.Escopeta:
                danioAplicado = danioEscopeta;
                break;
        }

        // Disparar parpadeo si tenemos TipoColorController
        if (tipoColorController != null)
        {
            tipoColorController.RecibirDanio(danioAplicado);
        }

        vida -= danioAplicado;

        if (vida <= 0)
        {
            vida = 0;
            isDead = true;
            Debug.Log("[Fase3Vida] La Cobra Elevada ha muerto.");

            // 1. Activar los scripts que estén en el array 'scriptsToActivate'
            if (scriptsToActivate != null && scriptsToActivate.Length > 0)
            {
                foreach (MonoBehaviour script in scriptsToActivate)
                {
                    if (script != null)
                    {
                        script.enabled = true;
                    }
                }
            }

            // 2. Eliminar los scripts que estén en el array 'scriptsToRemove'
            if (scriptsToRemove != null && scriptsToRemove.Length > 0)
            {
                foreach (MonoBehaviour script in scriptsToRemove)
                {
                    if (script != null)
                    {
                        Destroy(script);
                    }
                }
            }

            // 3. Deshabilitar (o destruir) el propio CobraElevadaController
            if (cobraController != null)
            {
                cobraController.enabled = false;
                // Si prefieres eliminarlo por completo en lugar de solo deshabilitar:
                // Destroy(cobraController);
            }

            // 4. Destruir TODO el GameObject que contiene este componente
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[Fase3Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
        }
    }

    /// <summary>
    /// Indica si la Cobra Elevada ya llegó a cero de vida.
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}



//using UnityEngine;

//public class Fase3Vida : MonoBehaviour
//{
//    [Header("Vida total de la Cobra Elevada")]
//    [Tooltip("Vida acumulada de todas las interacciones.")]
//    public int vida = 300;

//    [Header("Daño por tipo de bala")]
//    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
//    public int danioAmetralladora = 15;
//    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
//    public int danioPistola = 20;
//    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
//    public int danioEscopeta = 30;

//    [Header("Scripts a activar al morir")]
//    [Tooltip("Arrastra aquí los componentes que quieras habilitar cuando la Cobra Elevada muera.")]
//    public MonoBehaviour[] scriptsToActivate;

//    [Header("Scripts a eliminar al morir")]
//    [Tooltip("Arrastra aquí los componentes que quieras destruir cuando la Cobra Elevada muera.")]
//    public MonoBehaviour[] scriptsToRemove;

//    private bool isDead = false;
//    private CobraElevadaController cobraController;
//    private TipoColorController tipoColorController;

//    void Start()
//    {
//        // Intentamos obtener el componente CobraElevadaController en este GameObject
//        cobraController = GetComponent<CobraElevadaController>();
//        if (cobraController == null)
//        {
//            Debug.LogError("[Fase3Vida] No se encontró CobraElevadaController en este GameObject.");
//        }

//        // Intentamos obtener el TipoColorController para el parpadeo
//        tipoColorController = GetComponent<TipoColorController>();
//        if (tipoColorController == null)
//        {
//            Debug.LogWarning("[Fase3Vida] No se encontró TipoColorController. El parpadeo no funcionará.");
//        }
//    }

//    /// <summary>
//    /// Método público que puede ser llamado desde BalaPlayer al impactar la Cobra Elevada.
//    /// Resta vida según el tipo de bala y, cuando llegue a 0, activa/elimina scripts, aplica parpadeo y deshabilita el controlador.
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        if (isDead) return;

//        int danioAplicado = 0;
//        switch (tipoBala)
//        {
//            case BalaPlayer.TipoBala.Ametralladora:
//                danioAplicado = danioAmetralladora;
//                break;
//            case BalaPlayer.TipoBala.Pistola:
//                danioAplicado = danioPistola;
//                break;
//            case BalaPlayer.TipoBala.Escopeta:
//                danioAplicado = danioEscopeta;
//                break;
//        }

//        // Disparar parpadeo si tenemos TipoColorController
//        if (tipoColorController != null)
//        {
//            tipoColorController.RecibirDanio(danioAplicado);
//        }

//        vida -= danioAplicado;

//        if (vida <= 0)
//        {
//            vida = 0;
//            isDead = true;
//            Debug.Log("[Fase3Vida] La Cobra Elevada ha muerto.");

//            // 1. Activar los scripts que estén en el array 'scriptsToActivate'
//            if (scriptsToActivate != null && scriptsToActivate.Length > 0)
//            {
//                foreach (MonoBehaviour script in scriptsToActivate)
//                {
//                    if (script != null)
//                    {
//                        script.enabled = true;
//                    }
//                }
//            }

//            // 2. Eliminar los scripts que estén en el array 'scriptsToRemove'
//            if (scriptsToRemove != null && scriptsToRemove.Length > 0)
//            {
//                foreach (MonoBehaviour script in scriptsToRemove)
//                {
//                    if (script != null)
//                    {
//                        Destroy(script);
//                    }
//                }
//            }

//            // 3. Deshabilitar (o destruir) el propio CobraElevadaController
//            if (cobraController != null)
//            {
//                // Deshabilitamos para que no siga realizando pose/coletazo
//                cobraController.enabled = false;

//                // Si prefieres eliminarlo por completo en lugar de solo deshabilitar:
//                // Destroy(cobraController);
//            }

//            // 4. Destruir este componente Fase3Vida para que deje de procesar más daño
//            Destroy(this);
//        }
//        else
//        {
//            Debug.Log($"[Fase3Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
//        }
//    }

//    /// <summary>
//    /// Indica si la Cobra Elevada ya llegó a cero de vida.
//    /// </summary>
//    public bool IsDead()
//    {
//        return isDead;
//    }
//}
