// Fase2Vida.cs
using System.Collections;
using System.Collections.Generic;
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
    public TipoColorController tipoColorController;

    [Header("Columnas por Vida")]
    public Transform columna1;
    public Transform columna2;
    public Transform columna3;

    [Header("Vidas")]
    [Tooltip("Vida 1: cuando llegue a 0, cambiar a columna2")]
    public int vida1 = 1;
    [Tooltip("Vida 2: cuando llegue a 0, cambiar a columna3")]
    public int vida2 = 1;
    [Tooltip("Vida 3: cuando llegue a 0, terminar")]
    public int vida3 = 1;

    [Header("Slider de vida total")]
    [Tooltip("Muestra la vida total combinada de las tres fases")]
    public Slider vidaSlider;

    [Header("Parpadeo al recibir daño")]
    [Tooltip("Duración de la fase de oscilación (s)")]
    public float blinkDuration = 0.2f;
    [Tooltip("Duración de la transición de vuelta (s)")]
    public float returnDuration = 0.1f;
    [Tooltip("Ciclos de parpadeo por segundo")]
    public float blinkFrequency = 10f;

    [Header("Daño por Tipo de Bala (cuando NO coinciden)")]
    public int danioAmetralladora = 15;
    public int danioPistola = 20;
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a eliminar al morir")]
    public MonoBehaviour[] scriptsToRemove;

    [Header("Configuración de Ataque Repetido")]
    public float initialAttackDelay = 2f;
    public float attackInterval = 2f;

    // Fases: 0=vida1, 1=vida2, 2=vida3, 3=terminado
    private int currentPhase = 0;
    private Coroutine phaseCoroutine;

    // Slider & blink
    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress = false;
    private float blinkTimer = 0f;

    // Vida total y actual
    private int totalVidaInicial;
    private int vidaActual;

    void Start()
    {
        // validar referencias
        if (columnWrapper == null || snakeAttack == null || tipoColorController == null)
        {
            Debug.LogError("Fase2Vida: faltan referencias obligatorias.");
            enabled = false;
            return;
        }

        // calcular vida total
        totalVidaInicial = vida1 + vida2 + vida3;
        vidaActual = totalVidaInicial;

        // preparar slider
        if (vidaSlider != null && vidaSlider.fillRect != null)
        {
            vidaSlider.maxValue = totalVidaInicial;
            vidaSlider.value = vidaActual;
            fillImage = vidaSlider.fillRect.GetComponent<Image>();
            originalColor = fillImage.color;
        }

        // iniciar primera fase
        currentPhase = 0;
        columnWrapper.columna = columna1;
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

        // detener rutina de fase
        if (phaseCoroutine != null)
        {
            StopCoroutine(phaseCoroutine);
            phaseCoroutine = null;
        }

        // detener ataques
        snakeAttack.enabled = false;
        snakeAttack.StopAllCoroutines();

        if (nextColumn == null)
        {
            // fin total
            columnWrapper.enabled = false;
            columnWrapper.StopAllCoroutines();

            SnakeController snakeCtrl = GetComponent<SnakeController>();
            if (snakeCtrl != null)
            {
                var p = GameObject.FindWithTag("Player");
                if (p != null) snakeCtrl.jugador = p.transform;
                snakeCtrl.enabled = true;
            }

            StartCoroutine(HandleScriptArraysAfterDelay());
            return;
        }

        columnWrapper.columna = nextColumn;
        // no cambiamos slider.maxValue, sigue siendo total
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    private IEnumerator HandleScriptArraysAfterDelay()
    {
        yield return new WaitForSeconds(1f);
        if (scriptsToRemove != null)
            foreach (var s in scriptsToRemove) if (s != null) Destroy(s);
        if (scriptsToActivate != null)
            foreach (var s in scriptsToActivate) if (s != null) s.enabled = true;
    }

    private IEnumerator ExecutePhase2Sequence()
    {
        columnWrapper.TriggerWrap();
        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);
        yield return new WaitForSeconds(initialAttackDelay);

        snakeAttack.enabled = true;
        SnakeController snakeCtrl = snakeAttack.GetComponent<SnakeController>();
        var segmentsField = typeof(SnakeColumnWrapOnInput)
            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
        var preShakeMethod = typeof(SnakeColumnWrapOnInput)
            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

        while (true)
        {
            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
            var coro = (IEnumerator)preShakeMethod.Invoke(snakeAttack, null);
            StartCoroutine(coro);

            float total = snakeAttack.shakeDuration
                        + snakeAttack.shakeToAttackDelay
                        + snakeAttack.attackDuration
                        + attackInterval;
            yield return new WaitForSeconds(total);
        }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        var balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
        if (tipoColorController.CurrentTipo == balaComoEnemigo)
            return;

        int danio = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        // restar de la fase actual
        switch (currentPhase)
        {
            case 0: vida1 = Mathf.Max(0, vida1 - danio); break;
            case 1: vida2 = Mathf.Max(0, vida2 - danio); break;
            case 2: vida3 = Mathf.Max(0, vida3 - danio); break;
        }

        // recalcular vida total restante
        vidaActual = vida1 + vida2 + vida3;
        if (vidaSlider != null)
            vidaSlider.value = vidaActual;

        // iniciar/continuar parpadeo
        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null)
            StartCoroutine(BlinkRoutine());

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
        if (vidaSlider != null)
            Destroy(vidaSlider.gameObject);
    }
}









