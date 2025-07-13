// Fase2Vida.cs
//
// Controla fases de vida, wrap, anticipación en U antes de cada ataque,
// y luego ejecuta el ataque fluido en bucle.

using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColumnWrapOnInput))]
[RequireComponent(typeof(SnakeColumnWrapOnInput))]
public class Fase2Vida : MonoBehaviour
{
    [Header("Referencias")]
    public ColumnWrapOnInput columnWrapper;
    public SnakeColumnWrapOnInput snakeAttack;
    public SnakeAttackAnticipation anticipation;
    public TipoColorController tipoColorController;

    [Header("Columnas por Vida")]
    public Transform columna1, columna2, columna3;

    [Header("Vidas")]
    public int vida1 = 1, vida2 = 1, vida3 = 1;

    [Header("Slider de Vida")]
    public Slider vidaSlider;

    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f, returnDuration = 0.1f, blinkFrequency = 10f;

    [Header("Daño por Bala")]
    public int danioAmetralladora = 15, danioPistola = 20, danioEscopeta = 30;

    [Header("Scripts tras morir")]
    public MonoBehaviour[] scriptsToActivate, scriptsToRemove;

    [Header("Configuración de Ataque")]
    [Tooltip("Segundos tras completar el wrap antes de iniciar anticipación y ataque")]
    public float attackStartDelay = 3f;
    public float attackInterval = 2f;

    private int currentPhase;
    private Coroutine phaseCoroutine;
    private Coroutine anticipationCoroutine;
    private int totalVidaInicial, vidaActual;
    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress;
    private float blinkTimer;

    void Start()
    {
        if (columnWrapper == null || snakeAttack == null || anticipation == null || tipoColorController == null)
        {
            Debug.LogError("Fase2Vida: faltan referencias.");
            enabled = false;
            return;
        }

        totalVidaInicial = vida1 + vida2 + vida3;
        vidaActual = totalVidaInicial;
        if (vidaSlider != null && vidaSlider.fillRect != null)
        {
            vidaSlider.maxValue = totalVidaInicial;
            vidaSlider.value = vidaActual;
            fillImage = vidaSlider.fillRect.GetComponent<Image>();
            originalColor = fillImage.color;
        }

        currentPhase = 0;
        columnWrapper.columna = columna1;
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    void Update()
    {
        if (currentPhase == 0 && vida1 <= 0) TransitionToPhase(1, columna2);
        else if (currentPhase == 1 && vida2 <= 0) TransitionToPhase(2, columna3);
        else if (currentPhase == 2 && vida3 <= 0) TransitionToPhase(3, null);
    }

    private void TransitionToPhase(int nextPhase, Transform nextColumn)
    {
        currentPhase = nextPhase;

        // Detener todas las coroutines activas de esta clase
        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
        anticipationCoroutine = null;
        StopAllCoroutines();

        // También detener cualquier coroutine interna de snakeAttack/columnWrapper
        snakeAttack.StopAllCoroutines();
        columnWrapper.StopAllCoroutines();

        snakeAttack.enabled = false;
        columnWrapper.enabled = false;

        if (nextColumn == null)
        {
            // Última fase: liberar al jugador
            var snakeCtrl = GetComponent<SnakeController>();
            var p = GameObject.FindWithTag("Player");
            if (snakeCtrl != null && p != null) snakeCtrl.jugador = p.transform;
            if (snakeCtrl != null) snakeCtrl.enabled = true;
            StartCoroutine(HandleScriptArraysAfterDelay());
        }
        else
        {
            // Configurar nueva columna y reiniciar tras un frame
            columnWrapper.columna = nextColumn;
            StartCoroutine(RestartPhaseSequence());
        }
    }

    private IEnumerator RestartPhaseSequence()
    {
        yield return null; // un frame para asegurar que todo esté detenido
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    private IEnumerator ExecutePhase2Sequence()
    {
        // 1) Wrap en columna
        columnWrapper.TriggerWrap();
        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);

        // 2) Espera configurable tras subir antes de anticipación+ataque
        yield return new WaitForSeconds(attackStartDelay);

        var snakeCtrl = snakeAttack.GetComponent<SnakeController>();
        var segmentsField = typeof(SnakeColumnWrapOnInput)
            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
        var preShake = typeof(SnakeColumnWrapOnInput)
            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

        bool firstLoop = true;

        while (true)
        {
            if (!firstLoop)
                yield return new WaitForSeconds(attackInterval);
            firstLoop = false;

            // 3.1) Anticipación en U
            anticipation.Initialize(snakeCtrl, columnWrapper);
            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
            yield return anticipationCoroutine;
            anticipationCoroutine = null;

            // 3.2) Ataque fluido
            snakeAttack.enabled = true;
            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

            float waitTime = snakeAttack.shakeDuration
                           + snakeAttack.shakeToAttackDelay
                           + snakeAttack.attackDuration;
            yield return new WaitForSeconds(waitTime);
        }
    }

    private IEnumerator HandleScriptArraysAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        // En lugar de destruir los componentes, solo los desactivamos
        if (scriptsToRemove != null)
        {
            foreach (var s in scriptsToRemove)
            {
                if (s != null)
                    s.enabled = false;
            }
        }

        if (scriptsToActivate != null)
        {
            foreach (var s in scriptsToActivate)
            {
                if (s != null)
                    s.enabled = true;
            }
        }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        var enemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
        if (tipoColorController.CurrentTipo == enemigo) return;

        int danio = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        switch (currentPhase)
        {
            case 0: vida1 = Mathf.Max(0, vida1 - danio); break;
            case 1: vida2 = Mathf.Max(0, vida2 - danio); break;
            default: vida3 = Mathf.Max(0, vida3 - danio); break;
        }

        vidaActual = vida1 + vida2 + vida3;
        if (vidaSlider != null) vidaSlider.value = vidaActual;

        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null) StartCoroutine(BlinkRoutine());
        tipoColorController.RecibirDanio(0f);
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

    void OnDestroy()
    {
        if (vidaSlider != null) Destroy(vidaSlider.gameObject);
    }
}








