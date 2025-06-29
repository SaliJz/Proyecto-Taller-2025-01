using UnityEngine;

public class Fase1Vida : MonoBehaviour
{
    [Header("Vida total de este objeto")]
    [Tooltip("Vida acumulada de todas las interacciones.")]
    public int vida = 300;

    [Header("Daño por tipo de bala")]
    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
    public int danioAmetralladora = 15;
    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
    public int danioPistola = 20;
    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
    public int danioEscopeta = 30;

    [Header("Scripts a eliminar al morir")]
    [Tooltip("Arrastra aquí los componentes que quieras destruir cuando la vida llegue a 0.")]
    public MonoBehaviour[] scriptsToRemove;

    [Header("Scripts a activar al morir")]
    [Tooltip("Arrastra aquí los componentes que quieras habilitar cuando la vida llegue a 0.")]
    public MonoBehaviour[] scriptsToActivate;

    private bool isDead = false;
    private TipoColorController tipoColorController;

    void Start()
    {
        // Intentar obtener el componente TipoColorController en este GameObject
        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
        {
            Debug.LogError("[Fase1Vida] No se encontró TipoColorController en este GameObject.");
        }
    }

    /// <summary>
    /// Llamado por BalaPlayer al impactar este objeto.
    /// Solo resta vida si el tipo de bala NO coincide con el tipo actual del jefe.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        if (isDead) return;

        // Convertir TipoBala a TipoEnemigo para comparar contra el tipo actual
        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );

        // Solo aplicar daño si no coinciden
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase1Vida] Bala de tipo correcto: no se aplica daño.");
            return;
        }

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

        // Disparar parpadeo si existe TipoColorController
        if (tipoColorController != null)
        {
            tipoColorController.RecibirDanio(0f);
        }

        vida -= danioAplicado;

        if (vida <= 0)
        {
            vida = 0;
            isDead = true;
            Debug.Log("[Fase1Vida] El objeto ha muerto.");

            // 1. Activar los scripts que estén en el array 'scriptsToActivate'
            if (scriptsToActivate != null)
            {
                foreach (MonoBehaviour script in scriptsToActivate)
                    if (script != null) script.enabled = true;
            }

            // 2. Eliminar los scripts que estén en el array 'scriptsToRemove'
            if (scriptsToRemove != null)
            {
                foreach (MonoBehaviour script in scriptsToRemove)
                    if (script != null) Destroy(script);
            }

            // 3. Eliminar este mismo componente (Fase1Vida)
            Destroy(this);
        }
        else
        {
            Debug.Log($"[Fase1Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
        }
    }

    /// <summary>
    /// Indica si la vida ya llegó a cero.
    /// </summary>
    public bool IsDead()
    {
        return isDead;
    }
}






























//using UnityEngine;

//public class Fase1Vida : MonoBehaviour
//{
//    [Header("Vida total de este objeto")]
//    [Tooltip("Vida acumulada de todas las interacciones.")]
//    public int vida = 300;

//    [Header("Daño por tipo de bala")]
//    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
//    public int danioAmetralladora = 15;
//    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
//    public int danioPistola = 20;
//    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
//    public int danioEscopeta = 30;

//    [Header("Scripts a eliminar al morir")]
//    [Tooltip("Arrastra aquí los componentes que quieras destruir cuando la vida llegue a 0.")]
//    public MonoBehaviour[] scriptsToRemove;

//    [Header("Scripts a activar al morir")]
//    [Tooltip("Arrastra aquí los componentes que quieras habilitar cuando la vida llegue a 0.")]
//    public MonoBehaviour[] scriptsToActivate;

//    private bool isDead = false;
//    private TipoColorController tipoColorController;

//    void Start()
//    {
//        // Intentar obtener el componente TipoColorController en este GameObject
//        tipoColorController = GetComponent<TipoColorController>();
//    }

//    /// <summary>
//    /// Llamado por BalaPlayer al impactar este objeto.
//    /// Resta vida según el tipo de bala y activa parpadeo si existe TipoColorController.
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

//        vida -= danioAplicado;

//        // Si existe TipoColorController, pedirle que parpadee
//        if (tipoColorController != null)
//        {
//            // El método RecibirDanio de TipoColorController dispara el parpadeo
//            tipoColorController.RecibirDanio(danioAplicado);
//        }

//        if (vida <= 0)
//        {
//            vida = 0;
//            isDead = true;
//            Debug.Log("[Fase1Vida] El objeto ha muerto.");

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

//            // 3. Eliminar este mismo componente (Fase1Vida)
//            Destroy(this);

//            // Aquí podrías añadir lógica adicional de muerte, por ejemplo:
//            // - Reproducir animación de muerte
//            // - Desactivar colisiones
//            // - Destruir el GameObject después de un retardo, etc.
//        }
//        else
//        {
//            Debug.Log($"[Fase1Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
//        }
//    }

//    /// <summary>
//    /// Indica si la vida ya llegó a cero.
//    /// </summary>
//    public bool IsDead()
//    {
//        return isDead;
//    }
//}


