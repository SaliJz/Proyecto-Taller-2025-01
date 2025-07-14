// Fase2Vida.cs
//
// Gestiona múltiples fases de vida de un enemigo: tres fases de “columnas” con wrap,
// anticipación y ataque, y una fase final de vida independiente.
// Al agotarse cada fase, desactiva/activa scripts previstos y, tras la última vida,
// destruye componentes, ejecuta acciones finales y elimina este GameObject.

using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColumnWrapOnInput))]
[RequireComponent(typeof(SnakeColumnWrapOnInput))]
public class Fase2Vida : MonoBehaviour
{
    // === REFERENCIAS A OTROS COMPONENTES ===
    [Header("Referencias")]
    public ColumnWrapOnInput columnWrapper;        // Script que envuelve (“wrap”) en una columna
    public SnakeColumnWrapOnInput snakeAttack;     // Script que controla el ataque fluido tras anticipación
    public SnakeAttackAnticipation anticipation;   // Script que genera la animación de anticipación en U
    public TipoColorController tipoColorController;// Controla el color y lógica de daño según tipo

    // === CONFIGURACIÓN DE LAS COLUMNAS ===
    [Header("Columnas por Vida")]
    public Transform columna1, columna2, columna3; // Transforms de las tres columnas a usar

    [Header("Vidas de Columnas")]
    public int vida1 = 1, vida2 = 1, vida3 = 1;     // Puntos de vida iniciales de cada fase columna

    [Header("Slider de Vida de Columnas")]
    public Slider vidaSlider;                      // UI Slider que muestra la vida total de columnas

    [Header("Scripts tras Columnas")]
    public MonoBehaviour[] scriptsToRemoveAfterColumns;   // Scripts a desactivar al acabar columnas
    public MonoBehaviour[] scriptsToActivateAfterColumns; // Scripts a activar al acabar columnas

    // === CONFIGURACIÓN DE LA VIDA FINAL ===
    [Header("Vida Final (sin columna)")]
    public int vidaFinal = 5;                      // Puntos de vida de la fase final sin columna

    [Header("Slider de Vida Final")]
    public Slider finalLifeSlider;                 // UI Slider específico de la vida final

    [Header("Scripts tras Vida Final")]
    public MonoBehaviour[] scriptsToRemove;        // Scripts a desactivar al acabar la vida final
    public MonoBehaviour[] scriptsToActivate;      // Scripts a activar al acabar la vida final

    [Header("Scripts a DESTRUIR tras Última Vida")]
    public MonoBehaviour[] scriptsToDestroyAfterFinal;   // Scripts que se destruyen tras última vida

    [Header("Scripts a ACTIVAR tras Última Vida")]
    public MonoBehaviour[] scriptsToActivateAfterFinal;  // Scripts que se activan tras última vida

    // === PARÁMETROS DE DAÑO Y PARPADEO DE UI ===
    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f;             // Tiempo en que el color del slider parpadea
    public float returnDuration = 0.1f;            // Tiempo de transición de vuelta al color original
    public float blinkFrequency = 10f;             // Frecuencia de oscilación del parpadeo

    [Header("Daño por Bala")]
    public int danioAmetralladora = 15;            // Daño recibido por bala de ametralladora
    public int danioPistola = 20;            // Daño recibido por bala de pistola
    public int danioEscopeta = 30;            // Daño recibido por bala de escopeta

    // === CONFIGURACIÓN DE ATAQUE ===
    [Header("Configuración de Ataque")]
    [Tooltip("Segundos tras completar el wrap antes de iniciar anticipación y ataque")]
    public float attackStartDelay = 3f;            // Retardo tras subir por la columna
    public float attackInterval = 2f;            // Intervalo entre ataques sucesivos

    // === ESTADO INTERNO ===
    private int currentPhase;                      // Fase actual: 0,1,2→columnas; 3→vida final; 4→terminado
    private Coroutine phaseCoroutine;              // Coroutine principal de ejecución de fase
    private Coroutine anticipationCoroutine;       // Coroutine de animación de anticipación

    // Datos para parpadeo de vida de columnas
    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress;
    private float blinkTimer;

    // Datos para parpadeo de vida final
    private Image finalFillImage;
    private Color finalOriginalColor;
    private bool blinkFinalInProgress;
    private float blinkFinalTimer;

    // === MÉTODO DE INICIO ===
    void Start()
    {
        // Validar que todas las referencias están asignadas
        if (columnWrapper == null || snakeAttack == null || anticipation == null
            || tipoColorController == null || vidaSlider == null || finalLifeSlider == null)
        {
            Debug.LogError("Fase2Vida: faltan referencias asignadas en el Inspector.");
            enabled = false;
            return;
        }

        // Configurar slider de columnas
        int totalColumnas = vida1 + vida2 + vida3;
        vidaSlider.maxValue = totalColumnas;
        vidaSlider.value = totalColumnas;
        fillImage = vidaSlider.fillRect.GetComponent<Image>();
        originalColor = fillImage.color;

        // Configurar slider de vida final (oculto inicialmente)
        finalLifeSlider.gameObject.SetActive(false);
        finalLifeSlider.maxValue = vidaFinal;
        finalLifeSlider.value = vidaFinal;
        finalFillImage = finalLifeSlider.fillRect.GetComponent<Image>();
        finalOriginalColor = finalFillImage.color;

        // Comenzar en fase 0, columna1
        currentPhase = 0;
        columnWrapper.columna = columna1;
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    // === ACTUALIZACIÓN POR FRAME ===
    void Update()
    {
        // Chequear si la vida de cada columna llega a cero → transición de fase
        if (currentPhase == 0 && vida1 <= 0) TransitionToPhase(1, columna2);
        else if (currentPhase == 1 && vida2 <= 0) TransitionToPhase(2, columna3);
        else if (currentPhase == 2 && vida3 <= 0) TransitionToPhase(3, null);

        // Si está en fase 3 (vida final) y llega a cero, avanzar a fase 4
        else if (currentPhase == 3 && vidaFinal <= 0) TransitionToPhase(4, null);
    }

    // === TRANSICIÓN DE FASE ===
    private void TransitionToPhase(int nextPhase, Transform nextColumn)
    {
        currentPhase = nextPhase;

        // Detener todas las coroutines propias e internas
        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
        StopAllCoroutines();
        snakeAttack.StopAllCoroutines();
        columnWrapper.StopAllCoroutines();
        snakeAttack.enabled = false;
        columnWrapper.enabled = false;

        // --- Fase 3: justo después de agotar las 3 columnas ---
        if (nextPhase == 3)
        {
            // Desactivar/activar scripts tras columnas
            foreach (var s in scriptsToRemoveAfterColumns) if (s != null) s.enabled = false;
            foreach (var s in scriptsToActivateAfterColumns) if (s != null) s.enabled = true;

            // Asignar la referencia al jugador en SnakeController
            var snakeCtrl = GetComponent<SnakeController>();
            var jugadorObj = GameObject.FindWithTag("Player");
            if (snakeCtrl != null && jugadorObj != null)
                snakeCtrl.jugador = jugadorObj.transform;

            // Cambiar UI: ocultar slider de columnas y mostrar slider de vida final
            vidaSlider.gameObject.SetActive(false);
            finalLifeSlider.gameObject.SetActive(true);
            return;
        }

        // --- Fase 4: tras agotar la vida final ---
        if (nextPhase == 4)
        {
            // Desactivar/activar scripts tras vida final
            foreach (var s in scriptsToRemove) if (s != null) s.enabled = false;
            foreach (var s in scriptsToActivate) if (s != null) s.enabled = true;

            // Destruir componentes específicos
            foreach (var s in scriptsToDestroyAfterFinal) if (s != null) Destroy(s);
            // Activar otros componentes adicionales
            foreach (var s in scriptsToActivateAfterFinal) if (s != null) s.enabled = true;

            // Liberar y reactivar SnakeController para que el enemigo persiga al jugador
            var snakeCtrl2 = GetComponent<SnakeController>();
            var p = GameObject.FindWithTag("Player");
            if (snakeCtrl2 != null && p != null)
                snakeCtrl2.jugador = p.transform;
            if (snakeCtrl2 != null)
                snakeCtrl2.enabled = true;

            // Finalmente, destruir este GameObject (el enemigo principal)
            Destroy(this.gameObject);
            return;
        }

        // --- Fases intermedias de columna (0→1→2) ---
        columnWrapper.columna = nextColumn;
        StartCoroutine(RestartPhaseSequence());
    }

    // Reinicia la secuencia tras cambiar de columna
    private IEnumerator RestartPhaseSequence()
    {
        yield return null;                // Esperar un frame
        columnWrapper.enabled = true;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    // Coroutine principal: wrap → espera → anticipación → ataque en bucle
    private IEnumerator ExecutePhase2Sequence()
    {
        // 1) Trigger de wrap
        columnWrapper.TriggerWrap();
        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);

        // 2) Retardo antes de anticipación+ataque
        yield return new WaitForSeconds(attackStartDelay);

        // Obtener reflexiones de SnakeColumnWrapOnInput para shake/ataque
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

            // 3.1) Ejecutar animación de anticipación en U
            anticipation.Initialize(snakeCtrl, columnWrapper);
            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
            yield return anticipationCoroutine;
            anticipationCoroutine = null;

            // 3.2) Iniciar ataque fluido
            snakeAttack.enabled = true;
            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

            // Esperar duración total del shake + ataque
            float waitTime = snakeAttack.shakeDuration
                           + snakeAttack.shakeToAttackDelay
                           + snakeAttack.attackDuration;
            yield return new WaitForSeconds(waitTime);
        }
    }

    // === MÉTODO DE DAÑO ===
    // Se llama cuando el jugador impacta con bala
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        // No dañar si el tipo coincide
        var enemigo = (TipoColorController.TipoEnemigo)
                      System.Enum.Parse(typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
        if (tipoColorController.CurrentTipo == enemigo) return;

        // Calcular daño según tipo de bala
        int danio = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        // Reducir vida según fase
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

        // Notificar al controlador de color para efecto visual
        tipoColorController.RecibirDanio(0f);
    }

    // Parpadeo del slider de columnas al recibir daño
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

    // Parpadeo del slider de vida final al recibir daño
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

    // Limpieza de sliders al destruir este componente
    void OnDestroy()
    {
        if (vidaSlider != null) Destroy(vidaSlider.gameObject);
        if (finalLifeSlider != null) Destroy(finalLifeSlider.gameObject);
    }
}




//// Fase2Vida.cs
////
//// Controla fases de vida, wrap, anticipación en U antes de cada ataque,
//// y luego ejecuta el ataque fluido en bucle. Añade destrucción del GameObject
//// cuando la última vida llega a 0.

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

//    [Header("Vidas de Columnas")]
//    public int vida1 = 1, vida2 = 1, vida3 = 1;

//    [Header("Slider de Vida de Columnas")]
//    public Slider vidaSlider;

//    [Header("Scripts tras Columnas")]
//    public MonoBehaviour[] scriptsToRemoveAfterColumns;
//    public MonoBehaviour[] scriptsToActivateAfterColumns;

//    [Header("Vida Final (sin columna)")]
//    public int vidaFinal = 5;

//    [Header("Slider de Vida Final")]
//    public Slider finalLifeSlider;

//    [Header("Scripts tras Vida Final")]
//    public MonoBehaviour[] scriptsToRemove;
//    public MonoBehaviour[] scriptsToActivate;

//    [Header("Scripts a DESTRUIR tras Última Vida")]
//    public MonoBehaviour[] scriptsToDestroyAfterFinal;
//    [Header("Scripts a ACTIVAR tras Última Vida")]
//    public MonoBehaviour[] scriptsToActivateAfterFinal;

//    [Header("Parpadeo al recibir daño")]
//    public float blinkDuration = 0.2f, returnDuration = 0.1f, blinkFrequency = 10f;

//    [Header("Daño por Bala")]
//    public int danioAmetralladora = 15, danioPistola = 20, danioEscopeta = 30;

//    [Header("Configuración de Ataque")]
//    [Tooltip("Segundos tras completar el wrap antes de iniciar anticipación y ataque")]
//    public float attackStartDelay = 3f;
//    public float attackInterval = 2f;

//    private int currentPhase;
//    private Coroutine phaseCoroutine;
//    private Coroutine anticipationCoroutine;

//    // Para parpadeo UI columnas
//    private Image fillImage;
//    private Color originalColor;
//    private bool blinkInProgress;
//    private float blinkTimer;

//    // Para parpadeo UI vida final
//    private Image finalFillImage;
//    private Color finalOriginalColor;
//    private bool blinkFinalInProgress;
//    private float blinkFinalTimer;

//    void Start()
//    {
//        if (columnWrapper == null || snakeAttack == null || anticipation == null
//            || tipoColorController == null || vidaSlider == null || finalLifeSlider == null)
//        {
//            Debug.LogError("Fase2Vida: faltan referencias.");
//            enabled = false;
//            return;
//        }

//        // Configurar slider de columnas
//        vidaSlider.maxValue = vida1 + vida2 + vida3;
//        vidaSlider.value = vida1 + vida2 + vida3;
//        fillImage = vidaSlider.fillRect.GetComponent<Image>();
//        originalColor = fillImage.color;

//        // Configurar slider de vida final (oculto inicialmente)
//        finalLifeSlider.gameObject.SetActive(false);
//        finalLifeSlider.maxValue = vidaFinal;
//        finalLifeSlider.value = vidaFinal;
//        finalFillImage = finalLifeSlider.fillRect.GetComponent<Image>();
//        finalOriginalColor = finalFillImage.color;

//        // Iniciar fase 0
//        currentPhase = 0;
//        columnWrapper.columna = columna1;
//        columnWrapper.enabled = true;
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    void Update()
//    {
//        // Transiciones columnas
//        if (currentPhase == 0 && vida1 <= 0) TransitionToPhase(1, columna2);
//        else if (currentPhase == 1 && vida2 <= 0) TransitionToPhase(2, columna3);
//        else if (currentPhase == 2 && vida3 <= 0) TransitionToPhase(3, null);

//        // Transición vida final
//        else if (currentPhase == 3 && vidaFinal <= 0) TransitionToPhase(4, null);
//    }

//    private void TransitionToPhase(int nextPhase, Transform nextColumn)
//    {
//        currentPhase = nextPhase;

//        // Detener coroutines propios y de wrappers
//        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
//        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
//        StopAllCoroutines();
//        snakeAttack.StopAllCoroutines();
//        columnWrapper.StopAllCoroutines();
//        snakeAttack.enabled = false;
//        columnWrapper.enabled = false;

//        // --- Tras agotar las columnas: fase 3 ---
//        if (nextPhase == 3)
//        {
//            // Desactivar/activar scripts de columnas
//            if (scriptsToRemoveAfterColumns != null)
//                foreach (var s in scriptsToRemoveAfterColumns) if (s != null) s.enabled = false;
//            if (scriptsToActivateAfterColumns != null)
//                foreach (var s in scriptsToActivateAfterColumns) if (s != null) s.enabled = true;

//            // Asignar Player a SnakeController
//            var snakeCtrl = GetComponent<SnakeController>();
//            var jugadorObj = GameObject.FindWithTag("Player");
//            if (snakeCtrl != null && jugadorObj != null)
//                snakeCtrl.jugador = jugadorObj.transform;

//            // UI: pasar a slider vida final
//            vidaSlider.gameObject.SetActive(false);
//            finalLifeSlider.gameObject.SetActive(true);
//            return;
//        }

//        // --- Tras agotar vida final: fase 4 ---
//        if (nextPhase == 4)
//        {
//            // Desactivar/activar scripts de vida final
//            if (scriptsToRemove != null)
//                foreach (var s in scriptsToRemove) if (s != null) s.enabled = false;
//            if (scriptsToActivate != null)
//                foreach (var s in scriptsToActivate) if (s != null) s.enabled = true;

//            // DESTRUIR scripts especificados tras última vida
//            if (scriptsToDestroyAfterFinal != null)
//                foreach (var s in scriptsToDestroyAfterFinal) if (s != null) Destroy(s);

//            // ACTIVAR scripts adicionales tras última vida
//            if (scriptsToActivateAfterFinal != null)
//                foreach (var s in scriptsToActivateAfterFinal) if (s != null) s.enabled = true;

//            // Liberar jugador
//            var snakeCtrl = GetComponent<SnakeController>();
//            var p = GameObject.FindWithTag("Player");
//            if (snakeCtrl != null && p != null)
//                snakeCtrl.jugador = p.transform;
//            if (snakeCtrl != null)
//                snakeCtrl.enabled = true;

//            // DESTRUIR ESTE GAMEOBJECT al final                // NUEVO
//            Destroy(this.gameObject);

//            return;
//        }

//        // --- Siguientes fases de columna (0→1→2) ---
//        columnWrapper.columna = nextColumn;
//        StartCoroutine(RestartPhaseSequence());
//    }

//    private IEnumerator RestartPhaseSequence()
//    {
//        yield return null;
//        columnWrapper.enabled = true;
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    private IEnumerator ExecutePhase2Sequence()
//    {
//        // 1) Wrap en columna
//        columnWrapper.TriggerWrap();
//        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);

//        // 2) Delay antes de anticipación+ataque
//        yield return new WaitForSeconds(attackStartDelay);

//        var snakeCtrl = snakeAttack.GetComponent<SnakeController>();
//        var segmentsField = typeof(SnakeColumnWrapOnInput)
//            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
//        var preShake = typeof(SnakeColumnWrapOnInput)
//            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

//        bool firstLoop = true;
//        while (true)
//        {
//            if (!firstLoop) yield return new WaitForSeconds(attackInterval);
//            firstLoop = false;

//            // Anticipación en U
//            anticipation.Initialize(snakeCtrl, columnWrapper);
//            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
//            yield return anticipationCoroutine;
//            anticipationCoroutine = null;

//            // Ataque fluido
//            snakeAttack.enabled = true;
//            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
//            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

//            float waitTime = snakeAttack.shakeDuration
//                           + snakeAttack.shakeToAttackDelay
//                           + snakeAttack.attackDuration;
//            yield return new WaitForSeconds(waitTime);
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
//            case 0:
//                vida1 = Mathf.Max(0, vida1 - danio);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 1:
//                vida2 = Mathf.Max(0, vida2 - danio);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 2:
//                vida3 = Mathf.Max(0, vida3 - danio);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 3:
//                vidaFinal = Mathf.Max(0, vidaFinal - danio);
//                finalLifeSlider.value = vidaFinal;
//                if (!blinkFinalInProgress) StartCoroutine(BlinkFinalRoutine());
//                break;
//        }

//        tipoColorController.RecibirDanio(0f);
//    }

//    private IEnumerator BlinkRoutine()
//    {
//        blinkInProgress = true;
//        float total = blinkDuration + returnDuration;
//        float timer = 0f;
//        while (timer < total)
//        {
//            timer += Time.deltaTime;
//            if (timer <= blinkDuration)
//            {
//                float t = Mathf.PingPong(timer * blinkFrequency, 1f);
//                fillImage.color = Color.Lerp(originalColor, Color.white, t);
//            }
//            else
//            {
//                float t2 = (timer - blinkDuration) / returnDuration;
//                fillImage.color = Color.Lerp(Color.white, originalColor, t2);
//            }
//            yield return null;
//        }
//        fillImage.color = originalColor;
//        blinkInProgress = false;
//    }

//    private IEnumerator BlinkFinalRoutine()
//    {
//        blinkFinalInProgress = true;
//        float total = blinkDuration + returnDuration;
//        float timer = 0f;
//        while (timer < total)
//        {
//            timer += Time.deltaTime;
//            if (timer <= blinkDuration)
//            {
//                float t = Mathf.PingPong(timer * blinkFrequency, 1f);
//                finalFillImage.color = Color.Lerp(finalOriginalColor, Color.white, t);
//            }
//            else
//            {
//                float t2 = (timer - blinkDuration) / returnDuration;
//                finalFillImage.color = Color.Lerp(Color.white, finalOriginalColor, t2);
//            }
//            yield return null;
//        }
//        finalFillImage.color = finalOriginalColor;
//        blinkFinalInProgress = false;
//    }

//    void OnDestroy()
//    {
//        if (vidaSlider != null) Destroy(vidaSlider.gameObject);
//        if (finalLifeSlider != null) Destroy(finalLifeSlider.gameObject);
//    }
//}










