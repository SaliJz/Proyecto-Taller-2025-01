// Fase2Vida.cs
//
// Controla fases de vida, wrap, anticipación en U antes de cada ataque,
// y luego ejecuta el ataque fluido en bucle. Incluye arrays de scripts
// tras columnas y vida final, y asigna el jugador al SnakeController después de las columnas.

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

    [Header("Vidas de Columnas")]
    public int vida1 = 1, vida2 = 1, vida3 = 1;

    [Header("Slider de Vida de Columnas")]
    public Slider vidaSlider;

    [Header("Scripts tras Columnas")]
    public MonoBehaviour[] scriptsToRemoveAfterColumns;
    public MonoBehaviour[] scriptsToActivateAfterColumns;

    [Header("Vida Final (sin columna)")]
    public int vidaFinal = 5;

    [Header("Slider de Vida Final")]
    public Slider finalLifeSlider;

    [Header("Scripts tras Vida Final")]
    public MonoBehaviour[] scriptsToRemove;
    public MonoBehaviour[] scriptsToActivate;

    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f, returnDuration = 0.1f, blinkFrequency = 10f;

    [Header("Daño por Bala")]
    public int danioAmetralladora = 15, danioPistola = 20, danioEscopeta = 30;

    [Header("Configuración de Ataque")]
    [Tooltip("Segundos tras completar el wrap antes de iniciar anticipación y ataque")]
    public float attackStartDelay = 3f;
    public float attackInterval = 2f;

    private int currentPhase;
    private Coroutine phaseCoroutine;
    private Coroutine anticipationCoroutine;

    // Para parpadeo columnas
    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress;
    private float blinkTimer;

    // Para parpadeo vida final
    private Image finalFillImage;
    private Color finalOriginalColor;
    private bool blinkFinalInProgress;
    private float blinkFinalTimer;

    void Start()
    {
        if (columnWrapper == null || snakeAttack == null || anticipation == null
            || tipoColorController == null || vidaSlider == null || finalLifeSlider == null)
        {
            Debug.LogError("Fase2Vida: faltan referencias.");
            enabled = false;
            return;
        }

        // Slider columnas
        vidaSlider.maxValue = vida1 + vida2 + vida3;
        vidaSlider.value = vida1 + vida2 + vida3;
        fillImage = vidaSlider.fillRect.GetComponent<Image>();
        originalColor = fillImage.color;

        // Slider vida final
        finalLifeSlider.gameObject.SetActive(false);
        finalLifeSlider.maxValue = vidaFinal;
        finalLifeSlider.value = vidaFinal;
        finalFillImage = finalLifeSlider.fillRect.GetComponent<Image>();
        finalOriginalColor = finalFillImage.color;

        // Iniciar fase 0
        currentPhase = 0;
        columnWrapper.columna = columna1;
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    void Update()
    {
        // Transiciones columnas
        if (currentPhase == 0 && vida1 <= 0) TransitionToPhase(1, columna2);
        else if (currentPhase == 1 && vida2 <= 0) TransitionToPhase(2, columna3);
        else if (currentPhase == 2 && vida3 <= 0) TransitionToPhase(3, null);

        // Transición vida final
        else if (currentPhase == 3 && vidaFinal <= 0) TransitionToPhase(4, null);
    }

    private void TransitionToPhase(int nextPhase, Transform nextColumn)
    {
        currentPhase = nextPhase;

        // Detener coroutines propias e internas
        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
        StopAllCoroutines();
        snakeAttack.StopAllCoroutines();
        columnWrapper.StopAllCoroutines();
        snakeAttack.enabled = false;
        columnWrapper.enabled = false;

        // --- Tras agotar columnas: fase 3 ---
        if (nextPhase == 3)
        {
            // Desactivar/activar scripts específicos de columnas
            if (scriptsToRemoveAfterColumns != null)
                foreach (var s in scriptsToRemoveAfterColumns) if (s != null) s.enabled = false;
            if (scriptsToActivateAfterColumns != null)
                foreach (var s in scriptsToActivateAfterColumns) if (s != null) s.enabled = true;

            // Asignar el Player al SnakeController               // NUEVO
            var snakeCtrl = GetComponent<SnakeController>();
            var jugadorObj = GameObject.FindWithTag("Player");
            if (snakeCtrl != null && jugadorObj != null)
                snakeCtrl.jugador = jugadorObj.transform;

            // UI: ocultar slider columnas, mostrar slider vida final
            vidaSlider.gameObject.SetActive(false);
            finalLifeSlider.gameObject.SetActive(true);
            return;
        }

        // --- Tras agotar vida final: fase 4 ---
        if (nextPhase == 4)
        {
            // Desactivar/activar scripts de vida final
            if (scriptsToRemove != null)
                foreach (var s in scriptsToRemove) if (s != null) s.enabled = false;
            if (scriptsToActivate != null)
                foreach (var s in scriptsToActivate) if (s != null) s.enabled = true;

            // Liberar jugador (ya asignado)
            if (snakeAttack != null)
            {
                var snakeCtrl = GetComponent<SnakeController>();
                var p = GameObject.FindWithTag("Player");
                if (snakeCtrl != null && p != null) snakeCtrl.jugador = p.transform;
                if (snakeCtrl != null) snakeCtrl.enabled = true;
            }
            return;
        }

        // --- Nuevas fases de columna (0→1→2) ---
        columnWrapper.columna = nextColumn;
        StartCoroutine(RestartPhaseSequence());
    }

    private IEnumerator RestartPhaseSequence()
    {
        yield return null;
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    private IEnumerator ExecutePhase2Sequence()
    {
        columnWrapper.TriggerWrap();
        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);
        yield return new WaitForSeconds(attackStartDelay);

        var snakeCtrl = snakeAttack.GetComponent<SnakeController>();
        var segmentsField = typeof(SnakeColumnWrapOnInput)
            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
        var preShake = typeof(SnakeColumnWrapOnInput)
            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

        bool firstLoop = true;
        while (true)
        {
            if (!firstLoop) yield return new WaitForSeconds(attackInterval);
            firstLoop = false;

            anticipation.Initialize(snakeCtrl, columnWrapper);
            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
            yield return anticipationCoroutine;
            anticipationCoroutine = null;

            snakeAttack.enabled = true;
            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

            float waitTime = snakeAttack.shakeDuration
                           + snakeAttack.shakeToAttackDelay
                           + snakeAttack.attackDuration;
            yield return new WaitForSeconds(waitTime);
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
            case 0:
                vida1 = Mathf.Max(0, vida1 - danio);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 1:
                vida2 = Mathf.Max(0, vida2 - danio);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 2:
                vida3 = Mathf.Max(0, vida3 - danio);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 3:
                vidaFinal = Mathf.Max(0, vidaFinal - danio);
                finalLifeSlider.value = vidaFinal;
                if (!blinkFinalInProgress) StartCoroutine(BlinkFinalRoutine());
                break;
        }

        tipoColorController.RecibirDanio(0f);
    }

    private IEnumerator BlinkRoutine()
    {
        blinkInProgress = true;
        float total = blinkDuration + returnDuration;
        float timer = 0f;
        while (timer < total)
        {
            timer += Time.deltaTime;
            if (timer <= blinkDuration)
            {
                float t = Mathf.PingPong(timer * blinkFrequency, 1f);
                fillImage.color = Color.Lerp(originalColor, Color.white, t);
            }
            else
            {
                float t2 = (timer - blinkDuration) / returnDuration;
                fillImage.color = Color.Lerp(Color.white, originalColor, t2);
            }
            yield return null;
        }
        fillImage.color = originalColor;
        blinkInProgress = false;
    }

    private IEnumerator BlinkFinalRoutine()
    {
        blinkFinalInProgress = true;
        float total = blinkDuration + returnDuration;
        float timer = 0f;
        while (timer < total)
        {
            timer += Time.deltaTime;
            if (timer <= blinkDuration)
            {
                float t = Mathf.PingPong(timer * blinkFrequency, 1f);
                finalFillImage.color = Color.Lerp(finalOriginalColor, Color.white, t);
            }
            else
            {
                float t2 = (timer - blinkDuration) / returnDuration;
                finalFillImage.color = Color.Lerp(Color.white, finalOriginalColor, t2);
            }
            yield return null;
        }
        finalFillImage.color = finalOriginalColor;
        blinkFinalInProgress = false;
    }

    void OnDestroy()
    {
        if (vidaSlider != null) Destroy(vidaSlider.gameObject);
        if (finalLifeSlider != null) Destroy(finalLifeSlider.gameObject);
    }
}





//// Fase2Vida.cs
////
//// Controla fases de vida, wrap, anticipación en U antes de cada ataque,
//// y luego ejecuta el ataque fluido en bucle.

//using System.Collections;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(ColumnWrapOnInput))]
//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//public class Fase2Vida : MonoBehaviour
//{
//    [Header("Referencias")]
//    public ColumnWrapOnInput columnWrapper;
//    public SnakeColumnWrapOnInput snakeAttack;
//    public SnakeAttackAnticipation anticipation;
//    public TipoColorController tipoColorController;

//    [Header("Columnas por Vida")]
//    public Transform columna1, columna2, columna3;

//    [Header("Vidas")]
//    public int vida1 = 1, vida2 = 1, vida3 = 1;

//    [Header("Slider de Vida")]
//    public Slider vidaSlider;

//    [Header("Parpadeo al recibir daño")]
//    public float blinkDuration = 0.2f, returnDuration = 0.1f, blinkFrequency = 10f;

//    [Header("Daño por Bala")]
//    public int danioAmetralladora = 15, danioPistola = 20, danioEscopeta = 30;

//    [Header("Scripts tras morir")]
//    public MonoBehaviour[] scriptsToActivate, scriptsToRemove;

//    [Header("Configuración de Ataque")]
//    [Tooltip("Segundos tras completar el wrap antes de iniciar anticipación y ataque")]
//    public float attackStartDelay = 3f;
//    public float attackInterval = 2f;

//    private int currentPhase;
//    private Coroutine phaseCoroutine;
//    private Coroutine anticipationCoroutine;
//    private int totalVidaInicial, vidaActual;
//    private Image fillImage;
//    private Color originalColor;
//    private bool blinkInProgress;
//    private float blinkTimer;

//    void Start()
//    {
//        if (columnWrapper == null || snakeAttack == null || anticipation == null || tipoColorController == null)
//        {
//            Debug.LogError("Fase2Vida: faltan referencias.");
//            enabled = false;
//            return;
//        }

//        totalVidaInicial = vida1 + vida2 + vida3;
//        vidaActual = totalVidaInicial;
//        if (vidaSlider != null && vidaSlider.fillRect != null)
//        {
//            vidaSlider.maxValue = totalVidaInicial;
//            vidaSlider.value = vidaActual;
//            fillImage = vidaSlider.fillRect.GetComponent<Image>();
//            originalColor = fillImage.color;
//        }

//        currentPhase = 0;
//        columnWrapper.columna = columna1;
//        columnWrapper.enabled = true;
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    void Update()
//    {
//        if (currentPhase == 0 && vida1 <= 0) TransitionToPhase(1, columna2);
//        else if (currentPhase == 1 && vida2 <= 0) TransitionToPhase(2, columna3);
//        else if (currentPhase == 2 && vida3 <= 0) TransitionToPhase(3, null);
//    }

//    private void TransitionToPhase(int nextPhase, Transform nextColumn)
//    {
//        currentPhase = nextPhase;

//        // Detener todas las coroutines activas de esta clase
//        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
//        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
//        anticipationCoroutine = null;
//        StopAllCoroutines();

//        // También detener cualquier coroutine interna de snakeAttack/columnWrapper
//        snakeAttack.StopAllCoroutines();
//        columnWrapper.StopAllCoroutines();

//        snakeAttack.enabled = false;
//        columnWrapper.enabled = false;

//        if (nextColumn == null)
//        {
//            // Última fase: liberar al jugador
//            var snakeCtrl = GetComponent<SnakeController>();
//            var p = GameObject.FindWithTag("Player");
//            if (snakeCtrl != null && p != null) snakeCtrl.jugador = p.transform;
//            if (snakeCtrl != null) snakeCtrl.enabled = true;
//            StartCoroutine(HandleScriptArraysAfterDelay());
//        }
//        else
//        {
//            // Configurar nueva columna y reiniciar tras un frame
//            columnWrapper.columna = nextColumn;
//            StartCoroutine(RestartPhaseSequence());
//        }
//    }

//    private IEnumerator RestartPhaseSequence()
//    {
//        yield return null; // un frame para asegurar que todo esté detenido
//        columnWrapper.enabled = true;
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    private IEnumerator ExecutePhase2Sequence()
//    {
//        // 1) Wrap en columna
//        columnWrapper.TriggerWrap();
//        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);

//        // 2) Espera configurable tras subir antes de anticipación+ataque
//        yield return new WaitForSeconds(attackStartDelay);

//        var snakeCtrl = snakeAttack.GetComponent<SnakeController>();
//        var segmentsField = typeof(SnakeColumnWrapOnInput)
//            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
//        var preShake = typeof(SnakeColumnWrapOnInput)
//            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

//        bool firstLoop = true;

//        while (true)
//        {
//            if (!firstLoop)
//                yield return new WaitForSeconds(attackInterval);
//            firstLoop = false;

//            // 3.1) Anticipación en U
//            anticipation.Initialize(snakeCtrl, columnWrapper);
//            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
//            yield return anticipationCoroutine;
//            anticipationCoroutine = null;

//            // 3.2) Ataque fluido
//            snakeAttack.enabled = true;
//            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
//            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

//            float waitTime = snakeAttack.shakeDuration
//                           + snakeAttack.shakeToAttackDelay
//                           + snakeAttack.attackDuration;
//            yield return new WaitForSeconds(waitTime);
//        }
//    }

//    private IEnumerator HandleScriptArraysAfterDelay()
//    {
//        yield return new WaitForSeconds(1f);

//        // En lugar de destruir los componentes, solo los desactivamos
//        if (scriptsToRemove != null)
//        {
//            foreach (var s in scriptsToRemove)
//            {
//                if (s != null)
//                    s.enabled = false;
//            }
//        }

//        if (scriptsToActivate != null)
//        {
//            foreach (var s in scriptsToActivate)
//            {
//                if (s != null)
//                    s.enabled = true;
//            }
//        }
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        var enemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
//            typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
//        if (tipoColorController.CurrentTipo == enemigo) return;

//        int danio = tipoBala switch
//        {
//            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
//            BalaPlayer.TipoBala.Pistola => danioPistola,
//            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
//            _ => 0
//        };

//        switch (currentPhase)
//        {
//            case 0: vida1 = Mathf.Max(0, vida1 - danio); break;
//            case 1: vida2 = Mathf.Max(0, vida2 - danio); break;
//            default: vida3 = Mathf.Max(0, vida3 - danio); break;
//        }

//        vidaActual = vida1 + vida2 + vida3;
//        if (vidaSlider != null) vidaSlider.value = vidaActual;

//        blinkTimer = 0f;
//        if (!blinkInProgress && fillImage != null) StartCoroutine(BlinkRoutine());
//        tipoColorController.RecibirDanio(0f);
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

//    void OnDestroy()
//    {
//        if (vidaSlider != null) Destroy(vidaSlider.gameObject);
//    }
//}








