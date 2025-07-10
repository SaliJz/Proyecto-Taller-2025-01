using UnityEngine;

public class Fase1Vida : MonoBehaviour
{
    [Header("Vida de Fase Previa")]
    [Tooltip("Vida inicial de la fase previa.")]
    public int vidaFasePrevia = 1;

    [Header("Scripts a activar en fase previa")]
    [Tooltip("Se habilitan cuando vidaFasePrevia llega a 0.")]
    public MonoBehaviour[] preScriptsToActivate;
    [Tooltip("Se destruyen cuando vidaFasePrevia llega a 0.")]
    public MonoBehaviour[] preScriptsToRemove;

    [Header("Vida total de este objeto")]
    [Tooltip("Vida acumulada de todas las interacciones después de la fase previa.")]
    public int vida = 300;

    [Header("Daño por tipo de bala")]
    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
    public int danioAmetralladora = 15;
    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
    public int danioPistola = 20;
    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    [Tooltip("Componentes que se habilitan cuando la vida normal llega a 0.")]
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a eliminar al morir")]
    [Tooltip("Componentes que se destruyen cuando la vida normal llega a 0.")]
    public MonoBehaviour[] scriptsToRemove;

    private bool fasePreviaCompleted = false;
    private bool isDead = false;
    private TipoColorController tipoColorController;

    void Start()
    {
        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
            Debug.LogError("[Fase1Vida] No se encontró TipoColorController en este GameObject.");
    }

    /// <summary>
    /// Llamado por BalaPlayer al impactar este objeto.
    /// Primero consume la vida de fase previa; al agotarse, activa/destruye pre-scripts,
    /// asigna el jugador al SnakeController y luego procede a la fase normal de vida.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        if (isDead) return;

        // --- Fase previa ---
        if (!fasePreviaCompleted)
        {
            vidaFasePrevia--;
            if (vidaFasePrevia <= 0)
            {
                fasePreviaCompleted = true;
                Debug.Log("[Fase1Vida] Fase previa completada.");

                // Activar pre-scripts
                if (preScriptsToActivate != null)
                    foreach (var script in preScriptsToActivate)
                        if (script != null) script.enabled = true;

                // Destruir pre-scripts
                if (preScriptsToRemove != null)
                    foreach (var script in preScriptsToRemove)
                        if (script != null) Destroy(script);

                // **Asignar jugador al SnakeController**
                var snake = GetComponent<SnakeController>();
                if (snake != null)
                {
                    var playerObj = GameObject.FindWithTag("Player");
                    if (playerObj != null)
                        snake.jugador = playerObj.transform;
                    else
                        Debug.LogWarning("[Fase1Vida] No se encontró ningún GameObject con tag 'Player'.");
                }
                else
                {
                    Debug.LogWarning("[Fase1Vida] No se encontró SnakeController en este GameObject.");
                }
            }
            return;
        }

        // --- Fase normal ---
        // Convertir TipoBala a TipoEnemigo para comparar
        var balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );

        // No dañar si el tipo coincide
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase1Vida] Bala de tipo correcto: no se aplica daño.");
            return;
        }

        // Determinar daño
        int danioAplicado = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        // Parpadeo de color si existe el controlador
        tipoColorController?.RecibirDanio(0f);

        // Aplicar daño
        vida -= danioAplicado;

        if (vida <= 0)
        {
            vida = 0;
            isDead = true;
            Debug.Log("[Fase1Vida] El objeto ha muerto.");

            // Activar scripts de muerte
            if (scriptsToActivate != null)
                foreach (var script in scriptsToActivate)
                    if (script != null) script.enabled = true;

            // Eliminar scripts de muerte
            if (scriptsToRemove != null)
                foreach (var script in scriptsToRemove)
                    if (script != null) Destroy(script);

            // Eliminar este componente
            Destroy(this);
        }
        else
        {
            Debug.Log($"[Fase1Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
        }
    }

    /// <summary>
    /// Indica si la vida normal ya llegó a cero.
    /// </summary>
    public bool IsDead() => isDead;
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
//        if (tipoColorController == null)
//        {
//            Debug.LogError("[Fase1Vida] No se encontró TipoColorController en este GameObject.");
//        }
//    }

//    /// <summary>
//    /// Llamado por BalaPlayer al impactar este objeto.
//    /// Solo resta vida si el tipo de bala NO coincide con el tipo actual del jefe.
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        if (isDead) return;

//        // Convertir TipoBala a TipoEnemigo para comparar contra el tipo actual
//        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
//            typeof(TipoColorController.TipoEnemigo),
//            tipoBala.ToString()
//        );

//        // Solo aplicar daño si no coinciden
//        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
//        {
//            Debug.Log("[Fase1Vida] Bala de tipo correcto: no se aplica daño.");
//            return;
//        }

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

//        // Disparar parpadeo si existe TipoColorController
//        if (tipoColorController != null)
//        {
//            tipoColorController.RecibirDanio(0f);
//        }

//        vida -= danioAplicado;

//        if (vida <= 0)
//        {
//            vida = 0;
//            isDead = true;
//            Debug.Log("[Fase1Vida] El objeto ha muerto.");

//            // 1. Activar los scripts que estén en el array 'scriptsToActivate'
//            if (scriptsToActivate != null)
//            {
//                foreach (MonoBehaviour script in scriptsToActivate)
//                    if (script != null) script.enabled = true;
//            }

//            // 2. Eliminar los scripts que estén en el array 'scriptsToRemove'
//            if (scriptsToRemove != null)
//            {
//                foreach (MonoBehaviour script in scriptsToRemove)
//                    if (script != null) Destroy(script);
//            }

//            // 3. Eliminar este mismo componente (Fase1Vida)
//            Destroy(this);
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



























