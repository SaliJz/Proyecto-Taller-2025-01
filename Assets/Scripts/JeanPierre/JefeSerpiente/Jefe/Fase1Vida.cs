






// Fase1Vida.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

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

    [Header("GameObject a activar al completar fase previa")]
    [Tooltip("Asignar aquí el GameObject que debe activarse.")]
    public GameObject objetoToActivateOnPhaseComplete;

    [Header("Retardo antes de fase normal")]
    [Tooltip("Segundos a esperar tras completar la fase previa.")]
    public float normalPhaseDelay = 3f;

    [Header("Vida total de este objeto")]
    [Tooltip("Vida acumulada de todas las interacciones después de la fase previa.")]
    public int vida = 300;

    [Header("Daño por tipo de bala")]
    public int danioAmetralladora = 15;
    public int danioPistola = 20;
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a eliminar al morir")]
    public MonoBehaviour[] scriptsToRemove;

    [Header("Slider de vida normal")]
    [Tooltip("Slider que mostrará la vida normal una vez termine la animación.")]
    public Slider vidaSlider;

    [Header("Parpadeo al recibir daño")]
    [Tooltip("Duración (s) de la fase de oscilación.")]
    public float blinkDuration = 0.2f;
    [Tooltip("Duración (s) de la transición de vuelta al color original.")]
    public float returnDuration = 0.1f;
    [Tooltip("Ciclos de oscilación por segundo.")]
    public float blinkFrequency = 10f;

    private bool fasePreviaCompleted = false;
    private bool normalPhaseActive = false;
    private bool isDead = false;

    private TipoColorController tipoColorController;
    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress = false;
    private float blinkTimer = 0f;

    void Start()
    {
        // 1) Captura del ColorController
        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
            Debug.LogError("[Fase1Vida] No se encontró TipoColorController.");

        // 2) Preparar componente de imagen del slider
        if (vidaSlider != null && vidaSlider.fillRect != null)
        {
            fillImage = vidaSlider.fillRect.GetComponent<Image>();
            originalColor = fillImage != null ? fillImage.color : Color.white;
        }
        else
        {
            Debug.LogWarning("[Fase1Vida] Slider 'vidaSlider' o su fillRect no asignados.");
        }
    }

    // Ajusta el slider de vida al inicio de fase normal
    void SetupVidaSlider()
    {
        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vida;
            vidaSlider.value = vida;
        }
        else
        {
            Debug.LogWarning("[Fase1Vida] Slider 'vidaSlider' no asignado.");
        }
    }

    public void ApplyAbilityDamage(float damage)
    {
        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null) StartCoroutine(BlinkRoutine());

        tipoColorController?.RecibirDanio(0f);

        vida -= (int)damage;
        if (vidaSlider != null) vidaSlider.value = vida;
        if (vida <= 0) Morir();

    }

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

                // Activar y destruir scripts de fase previa
                if (preScriptsToActivate != null)
                    foreach (var s in preScriptsToActivate)
                        if (s != null) s.enabled = true;

                if (preScriptsToRemove != null)
                    foreach (var s in preScriptsToRemove)
                        if (s != null) Destroy(s);

                // Activar objeto con SequentialSliderFill
                if (objetoToActivateOnPhaseComplete != null)
                {
                    objetoToActivateOnPhaseComplete.SetActive(true);
                    var seqFill = objetoToActivateOnPhaseComplete.GetComponent<SequentialSliderFill>();
                    if (seqFill != null)
                        seqFill.OnSequenceComplete += SetupVidaSlider;
                    else
                        Debug.LogWarning("[Fase1Vida] No se encontró SequentialSliderFill en el objeto activado.");

                    // Reasignar jugador en SnakeController
                    var snake = GetComponent<SnakeController>();
                    if (snake != null)
                    {
                        var playerObj = GameObject.FindWithTag("Player");
                        if (playerObj != null)
                            snake.jugador = playerObj.transform;
                        else
                            Debug.LogWarning("[Fase1Vida] No se encontró GameObject con tag 'Player'.");
                    }
                }
                else
                {
                    Debug.LogWarning("[Fase1Vida] 'objetoToActivateOnPhaseComplete' no asignado.");
                    SetupVidaSlider();
                }

                StartCoroutine(NormalPhaseDelayRoutine());
            }
            return;
        }

        // --- Retardo de fase normal aún en curso ---
        if (!normalPhaseActive)
        {
            Debug.Log("[Fase1Vida] Fase normal aún no activa, daño ignorado.");
            return;
        }

        // --- Fase normal: aplicar daño ---
        var balaComoEnemigo = (TipoColorController.TipoEnemigo)
            Enum.Parse(typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());

        // Sin daño si coinciden colores
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase1Vida] Bala de tipo correcto: sin daño.");
            return;
        }

        int danio = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        // Iniciar parpadeo
        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null)
            StartCoroutine(BlinkRoutine());

        tipoColorController?.RecibirDanio(0f);

        vida = Mathf.Max(0, vida - danio);
        if (vidaSlider != null)
            vidaSlider.value = vida;

        if (vida <= 0)
            Morir();
        else
            Debug.Log($"[Fase1Vida] Daño {danio}. Vida restante: {vida}.");
    }

    private IEnumerator NormalPhaseDelayRoutine()
    {
        yield return new WaitForSeconds(normalPhaseDelay);
        normalPhaseActive = true;
        Debug.Log($"[Fase1Vida] Fase normal activada tras {normalPhaseDelay} segundos.");
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

    private void Morir()
    {
        isDead = true;
        Debug.Log("[Fase1Vida] El objeto ha muerto.");

        // Activar scripts finales
        if (scriptsToActivate != null)
            foreach (var s in scriptsToActivate)
                if (s != null) s.enabled = true;

        // Destruir scripts finales
        if (scriptsToRemove != null)
            foreach (var s in scriptsToRemove)
                if (s != null) Destroy(s);

        // Destruir todo el GameObject para limpiar componentes
        //Destroy(gameObject);
    }

    void OnDestroy()
    {
        if (vidaSlider != null)
            Destroy(vidaSlider.gameObject);
    }

    public bool IsDead() => isDead;
}







































