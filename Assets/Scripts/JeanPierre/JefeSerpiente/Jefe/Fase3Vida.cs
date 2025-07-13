using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CobraElevadaController))]
public class Fase3Vida : MonoBehaviour
{
    [Header("Vida total de la Cobra Elevada")]
    [Tooltip("Vida acumulada de todas las interacciones.")]
    public int vida = 300;

    [Header("Slider de vida")]
    [Tooltip("Muestra la vida restante de la Cobra Elevada.")]
    public Slider vidaSlider;

    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f;
    public float returnDuration = 0.1f;
    public float blinkFrequency = 10f;

    [Header("Daño por tipo de bala")]
    public int danioAmetralladora = 15;
    public int danioPistola = 20;
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a eliminar al morir")]
    public MonoBehaviour[] scriptsToRemove;

    private bool isDead = false;
    private CobraElevadaController cobraController;
    private TipoColorController tipoColorController;

    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress = false;
    private float blinkTimer = 0f;

    void Start()
    {
        cobraController = GetComponent<CobraElevadaController>();
        if (cobraController == null)
            Debug.LogError("[Fase3Vida] Falta CobraElevadaController.");

        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
            Debug.LogWarning("[Fase3Vida] Falta TipoColorController (parpadeo no funcionará).");

        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vida;
            vidaSlider.value = vida;
            if (vidaSlider.fillRect != null)
            {
                fillImage = vidaSlider.fillRect.GetComponent<Image>();
                originalColor = fillImage.color;
            }
            else
            {
                Debug.LogWarning("[Fase3Vida] Slider.fillRect es null.");
            }
        }
        else
        {
            Debug.LogError("[Fase3Vida] Slider 'vidaSlider' NO está asignado en el Inspector.");
        }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {

        Debug.Log("[Fase3Vida] RecibirDanioPorBala INVOCADO");
        // Depuración: ver si entra al método
        Debug.Log($"[Fase3Vida] Llamado RecibirDanioPorBala con {tipoBala}. Vida antes: {vida}");

        if (isDead)
        {
            Debug.Log("[Fase3Vida] Ya está muerto, ignoro daño.");
            return;
        }

        // Comprueba color
        var balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase3Vida] ¡Mismo color! No aplico daño.");
            return;
        }

        // Calcula daño
        int danioAplicado = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        vida = Mathf.Max(0, vida - danioAplicado);
        Debug.Log($"[Fase3Vida] Daño aplicado: {danioAplicado}. Vida ahora: {vida}");

        // Actualiza slider
        if (vidaSlider != null)
        {
            vidaSlider.value = vida;
        }

        // Parpadeo
        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null)
            StartCoroutine(BlinkRoutine());

        if (vida <= 0)
            Die();
    }

    private IEnumerator BlinkRoutine()
    {
        blinkInProgress = true;
        float total = blinkDuration + returnDuration;

        while (blinkTimer < total)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer <= blinkDuration)
            {
                float t = Mathf.PingPong(blinkTimer * blinkFrequency, 1f);
                fillImage.color = Color.Lerp(originalColor, Color.white, t);
            }
            else
            {
                float t2 = (blinkTimer - blinkDuration) / returnDuration;
                fillImage.color = Color.Lerp(Color.white, originalColor, t2);
            }
            yield return null;
        }

        fillImage.color = originalColor;
        blinkInProgress = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[Fase3Vida] ¡La cobra ha muerto!");

        foreach (var s in scriptsToActivate)
            if (s != null) s.enabled = true;
        foreach (var s in scriptsToRemove)
            if (s != null) Destroy(s);

        if (cobraController != null)
            cobraController.enabled = false;

        if (vidaSlider != null)
            Destroy(vidaSlider.gameObject);

        Destroy(gameObject);
    }

    public bool IsDead() => isDead;
}



//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public class Fase3Vida : MonoBehaviour
//{
//    [Header("Vida total de la Cobra Elevada")]
//    [Tooltip("Vida acumulada de todas las interacciones.")]
//    public int vida = 300;

//    [Header("Slider de vida")]
//    [Tooltip("Muestra la vida restante de la Cobra Elevada.")]
//    public Slider vidaSlider;

//    [Header("Parpadeo al recibir daño")]
//    [Tooltip("Duración de la fase de oscilación (s)")]
//    public float blinkDuration = 0.2f;
//    [Tooltip("Duración de la transición de vuelta (s)")]
//    public float returnDuration = 0.1f;
//    [Tooltip("Ciclos de parpadeo por segundo")]
//    public float blinkFrequency = 10f;

//    [Header("Daño por tipo de bala")]
//    [Tooltip("Daño aplicado si la bala es de tipo Ametralladora.")]
//    public int danioAmetralladora = 15;
//    [Tooltip("Daño aplicado si la bala es de tipo Pistola.")]
//    public int danioPistola = 20;
//    [Tooltip("Daño aplicado si la bala es de tipo Escopeta.")]
//    public int danioEscopeta = 30;

//    [Header("Scripts a activar al morir")]
//    [Tooltip("Componentes que se habilitan cuando la Cobra Elevada muere.")]
//    public MonoBehaviour[] scriptsToActivate;

//    [Header("Scripts a eliminar al morir")]
//    [Tooltip("Componentes que se destruyen cuando la Cobra Elevada muere.")]
//    public MonoBehaviour[] scriptsToRemove;

//    private bool isDead = false;
//    private CobraElevadaController cobraController;
//    private TipoColorController tipoColorController;

//    // Internos para parpadeo de slider
//    private Image fillImage;
//    private Color originalColor;
//    private bool blinkInProgress = false;
//    private float blinkTimer = 0f;

//    void Start()
//    {
//        // Obtener controladores
//        cobraController = GetComponent<CobraElevadaController>();
//        if (cobraController == null)
//            Debug.LogError("[Fase3Vida] No se encontró CobraElevadaController en este GameObject.");

//        tipoColorController = GetComponent<TipoColorController>();
//        if (tipoColorController == null)
//            Debug.LogWarning("[Fase3Vida] No se encontró TipoColorController. El parpadeo no funcionará.");

//        // Configurar slider
//        if (vidaSlider != null)
//        {
//            vidaSlider.maxValue = vida;
//            vidaSlider.value = vida;
//            if (vidaSlider.fillRect != null)
//            {
//                fillImage = vidaSlider.fillRect.GetComponent<Image>();
//                originalColor = fillImage.color;
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[Fase3Vida] No asignaste el Slider 'vidaSlider' en el Inspector.");
//        }
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        if (isDead) return;

//        // Comparar tipo de bala vs tipo actual
//        var balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
//            typeof(TipoColorController.TipoEnemigo),
//            tipoBala.ToString()
//        );

//        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
//        {
//            Debug.Log("[Fase3Vida] Bala de tipo correcto: no se aplica daño.");
//            return;
//        }

//        // Efecto visual de daño
//        tipoColorController?.RecibirDanio(0f);

//        // Calcular daño
//        int danioAplicado = tipoBala switch
//        {
//            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
//            BalaPlayer.TipoBala.Pistola => danioPistola,
//            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
//            _ => 0
//        };

//        // Restar vida
//        vida = Mathf.Max(0, vida - danioAplicado);

//        // Actualizar slider
//        if (vidaSlider != null)
//            vidaSlider.value = vida;

//        // Iniciar parpadeo
//        blinkTimer = 0f;
//        if (!blinkInProgress && fillImage != null)
//            StartCoroutine(BlinkRoutine());

//        if (vida <= 0)
//            Die();
//        else
//            Debug.Log($"[Fase3Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
//    }

//    private IEnumerator BlinkRoutine()
//    {
//        blinkInProgress = true;
//        float total = blinkDuration + returnDuration;

//        while (blinkTimer < total)
//        {
//            blinkTimer += Time.deltaTime;
//            if (blinkTimer <= blinkDuration)
//            {
//                float t = Mathf.PingPong(blinkTimer * blinkFrequency, 1f);
//                fillImage.color = Color.Lerp(originalColor, Color.white, t);
//            }
//            else
//            {
//                float t2 = (blinkTimer - blinkDuration) / returnDuration;
//                fillImage.color = Color.Lerp(Color.white, originalColor, t2);
//            }
//            yield return null;
//        }

//        fillImage.color = originalColor;
//        blinkInProgress = false;
//    }

//    private void Die()
//    {
//        isDead = true;
//        Debug.Log("[Fase3Vida] La Cobra Elevada ha muerto.");

//        // Activar y destruir scripts
//        if (scriptsToActivate != null)
//            foreach (var s in scriptsToActivate)
//                if (s != null) s.enabled = true;
//        if (scriptsToRemove != null)
//            foreach (var s in scriptsToRemove)
//                if (s != null) Destroy(s);

//        // Deshabilitar controlador
//        if (cobraController != null)
//            cobraController.enabled = false;

//        // Destruir slider y GameObject
//        if (vidaSlider != null)
//            Destroy(vidaSlider.gameObject);
//        Destroy(gameObject);
//    }

//    public bool IsDead() => isDead;
//}












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
//        cobraController = GetComponent<CobraElevadaController>();
//        if (cobraController == null)
//            Debug.LogError("[Fase3Vida] No se encontró CobraElevadaController en este GameObject.");

//        tipoColorController = GetComponent<TipoColorController>();
//        if (tipoColorController == null)
//            Debug.LogWarning("[Fase3Vida] No se encontró TipoColorController. El parpadeo no funcionará.");
//    }

//    /// <summary>
//    /// Método público que puede ser llamado desde BalaPlayer al impactar la Cobra Elevada.
//    /// Solo resta vida si el tipo de bala NO coincide con el tipo actual del jefe.
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        if (isDead) return;

//        // Convertir TipoBala a TipoEnemigo para comparar con el tipo actual
//        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
//            typeof(TipoColorController.TipoEnemigo),
//            tipoBala.ToString()
//        );

//        // Si coincide, no aplicar daño
//        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
//        {
//            Debug.Log("[Fase3Vida] Bala de tipo correcto: no se aplica daño.");
//            return;
//        }

//        // Disparar parpadeo si tenemos TipoColorController
//        if (tipoColorController != null)
//            tipoColorController.RecibirDanio(0f);

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

//        if (vida <= 0)
//        {
//            vida = 0;
//            isDead = true;
//            Debug.Log("[Fase3Vida] La Cobra Elevada ha muerto.");

//            // Activar scripts
//            if (scriptsToActivate != null)
//                foreach (var script in scriptsToActivate)
//                    if (script != null) script.enabled = true;

//            // Eliminar scripts
//            if (scriptsToRemove != null)
//                foreach (var script in scriptsToRemove)
//                    if (script != null) Destroy(script);

//            // Deshabilitar controlador de comportamiento
//            if (cobraController != null)
//                cobraController.enabled = false;

//            // Destruir el GameObject entero
//            Destroy(gameObject);
//        }
//        else
//        {
//            Debug.Log($"[Fase3Vida] Se recibió {danioAplicado} de daño. Vida restante: {vida}.");
//        }
//    }

//    public bool IsDead()
//    {
//        return isDead;
//    }
//}
