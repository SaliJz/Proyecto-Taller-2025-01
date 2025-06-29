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
        cobraController = GetComponent<CobraElevadaController>();
        if (cobraController == null)
            Debug.LogError("[Fase3Vida] No se encontró CobraElevadaController en este GameObject.");

        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
            Debug.LogWarning("[Fase3Vida] No se encontró TipoColorController. El parpadeo no funcionará.");
    }

    /// <summary>
    /// Método público que puede ser llamado desde BalaPlayer al impactar la Cobra Elevada.
    /// Solo resta vida si el tipo de bala NO coincide con el tipo actual del jefe.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        if (isDead) return;

        // Convertir TipoBala a TipoEnemigo para comparar con el tipo actual
        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );

        // Si coincide, no aplicar daño
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase3Vida] Bala de tipo correcto: no se aplica daño.");
            return;
        }

        // Disparar parpadeo si tenemos TipoColorController
        if (tipoColorController != null)
            tipoColorController.RecibirDanio(0f);

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

        vida -= danioAplicado;

        if (vida <= 0)
        {
            vida = 0;
            isDead = true;
            Debug.Log("[Fase3Vida] La Cobra Elevada ha muerto.");

            // Activar scripts
            if (scriptsToActivate != null)
                foreach (var script in scriptsToActivate)
                    if (script != null) script.enabled = true;

            // Eliminar scripts
            if (scriptsToRemove != null)
                foreach (var script in scriptsToRemove)
                    if (script != null) Destroy(script);

            // Deshabilitar controlador de comportamiento
            if (cobraController != null)
                cobraController.enabled = false;

            // Destruir el GameObject entero
            Destroy(gameObject);
        }
        else
        {
            Debug.Log($"[Fase3Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
        }
    }

    public bool IsDead()
    {
        return isDead;
    }
}
