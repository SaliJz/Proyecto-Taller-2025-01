using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ColumnWrapOnInput))]
[RequireComponent(typeof(SnakeColumnWrapOnInput))]
[RequireComponent(typeof(AnimacionMuerteCobraEpic))]
[RequireComponent(typeof(CobraElevadaController))]
[RequireComponent(typeof(SnakeDissolveController))]
public class Fase2Vida : MonoBehaviour
{
    // === NUEVA REFERENCIA AL GENERADOR DE ENEMIGOS ===
    [Header("Referencia a Generador de Enemigos")]
    [Tooltip("Arrastra aquí el objeto que tiene el GeneradorUnicoDeEnemigosAleatoriosYUbicaciones")]
    public GeneradorUnicoDeEnemigosAleatoriosYUbicaciones enemyGenerator;

    // === REFERENCIAS A OTROS COMPONENTES ===
    [Header("Referencias")]
    public ColumnWrapOnInput columnWrapper;
    public SnakeColumnWrapOnInput snakeAttack;
    public SnakeAttackAnticipation anticipation;
    public TipoColorController tipoColorController;

    // === CONFIGURACIÓN DE LAS COLUMNAS ===
    [Header("Columnas por Vida")]
    public Transform columna1, columna2, columna3;
    [Header("Vidas de Columnas")]
    public int vida1 = 1, vida2 = 1, vida3 = 1;
    [Header("Slider de Vida de Columnas")]
    public Slider vidaSlider;
    [Header("Scripts tras Columnas")]
    public MonoBehaviour[] scriptsToRemoveAfterColumns;
    public MonoBehaviour[] scriptsToActivateAfterColumns;

    // === CONFIGURACIÓN DE LA VIDA FINAL ===
    [Header("Vida Final (sin columna)")]
    public int vidaFinal = 5;
    [Header("Slider de Vida Final")]
    public Slider finalLifeSlider;
    [Header("Scripts tras Vida Final")]
    public MonoBehaviour[] scriptsToRemove;
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a DESTRUIR tras Última Vida")]
    public MonoBehaviour[] scriptsToDestroyAfterFinal;
    [Header("Scripts a ACTIVAR tras Última Vida")]
    public MonoBehaviour[] scriptsToActivateAfterFinal;

    // === PARÁMETROS DE DAÑO Y PARPADEO DE UI ===
    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f;
    public float returnDuration = 0.1f;
    public float blinkFrequency = 10f;
    [Header("Daño por Bala")]
    public int danioAmetralladora = 15;
    public int danioPistola = 20;
    public int danioEscopeta = 30;

    // === CONFIGURACIÓN DE ATAQUE ===
    [Header("Configuración de Ataque")]
    public float attackStartDelay = 3f;
    public float attackInterval = 2f;

    // === ESTADO INTERNO ===
    private int currentPhase;
    private Coroutine phaseCoroutine, anticipationCoroutine;
    private Image fillImage, finalFillImage;
    private Color originalColor, finalOriginalColor;
    private bool blinkInProgress, blinkFinalInProgress;

    void Start()
    {
        if (columnWrapper == null || snakeAttack == null || anticipation == null
            || tipoColorController == null || vidaSlider == null || finalLifeSlider == null)
        {
            Debug.LogError("Fase2Vida: faltan referencias asignadas en el Inspector.");
            enabled = false;
            return;
        }

        int totalCols = vida1 + vida2 + vida3;
        vidaSlider.maxValue = totalCols;
        vidaSlider.value = totalCols;
        fillImage = vidaSlider.fillRect.GetComponent<Image>();
        originalColor = fillImage.color;

        finalLifeSlider.gameObject.SetActive(false);
        finalLifeSlider.maxValue = vidaFinal;
        finalLifeSlider.value = vidaFinal;
        finalFillImage = finalLifeSlider.fillRect.GetComponent<Image>();
        finalOriginalColor = finalFillImage.color;

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
        else if (currentPhase == 3 && vidaFinal <= 0) TransitionToPhase(4, null);
    }

    private void TransitionToPhase(int nextPhase, Transform nextColumn)
    {
        currentPhase = nextPhase;

        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
        StopAllCoroutines();
        snakeAttack.StopAllCoroutines();
        columnWrapper.StopAllCoroutines();
        snakeAttack.enabled = false;
        columnWrapper.enabled = false;

        if (nextPhase == 3)
        {
            // Al terminar la última columna:
            var snakeCtrl = GetComponent<SnakeController>();
            snakeCtrl.jugador = GameObject.FindWithTag("Player")?.transform;

            foreach (var s in scriptsToRemoveAfterColumns) if (s) s.enabled = false;
            foreach (var s in scriptsToActivateAfterColumns) if (s) s.enabled = true;
            vidaSlider.gameObject.SetActive(false);
            finalLifeSlider.gameObject.SetActive(true);
            return;
        }

        if (nextPhase == 4)
        {
            // Detener spawn y eliminar enemigos
            if (enemyGenerator != null)
                enemyGenerator.stopSpawning = true;
            else
                Debug.LogWarning("Fase2Vida: no está asignado enemyGenerator en el Inspector.");

            foreach (var s in scriptsToRemove) if (s) s.enabled = false;
            foreach (var s in scriptsToActivate) if (s) s.enabled = true;
            foreach (var s in scriptsToDestroyAfterFinal) if (s) Destroy(s);
            foreach (var s in scriptsToActivateAfterFinal) if (s) s.enabled = true;

            var cobraElevada = GetComponent<CobraElevadaController>();
            if (cobraElevada) cobraElevada.enabled = false;
            var epic = GetComponent<AnimacionMuerteCobraEpic>();
            if (epic) { epic.enabled = true; epic.activarEpic = true; }
            var dissolveCtrl = GetComponent<SnakeDissolveController>();
            if (dissolveCtrl) { dissolveCtrl.enabled = true; dissolveCtrl.dissolveActive = true; }
            return;
        }

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

    public void ApplyAbilityDamage(float damage)
    {
        switch (currentPhase)
        {
            case 0:
                vida1 = Mathf.Max(0, vida1 - (int)damage);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 1:
                vida2 = Mathf.Max(0, vida2 - (int)damage);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 2:
                vida3 = Mathf.Max(0, vida3 - (int)damage);
                vidaSlider.value = vida1 + vida2 + vida3;
                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
                break;
            case 3:
                vidaFinal = Mathf.Max(0, vidaFinal - (int)damage);
                finalLifeSlider.value = vidaFinal;
                if (!blinkFinalInProgress) StartCoroutine(BlinkFinalRoutine());
                break;
        }
        tipoColorController.RecibirDanio(0f);
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        var enemigo = (TipoColorController.TipoEnemigo)
                      System.Enum.Parse(typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
        if (tipoColorController.CurrentTipo == enemigo) return;

        int danio = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        ApplyAbilityDamage(danio);
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




//using System.Collections;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.UI;

//[RequireComponent(typeof(ColumnWrapOnInput))]
//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//[RequireComponent(typeof(AnimacionMuerteCobraEpic))]
//[RequireComponent(typeof(CobraElevadaController))]
//[RequireComponent(typeof(SnakeDissolveController))]
//public class Fase2Vida : MonoBehaviour
//{
//    // === NUEVA REFERENCIA AL GENERADOR DE ENEMIGOS ===
//    [Header("Referencia a Generador de Enemigos")]
//    [Tooltip("Arrastra aquí el objeto que tiene el GeneradorUnicoDeEnemigosAleatoriosYUbicaciones")]
//    public GeneradorUnicoDeEnemigosAleatoriosYUbicaciones enemyGenerator;

//    // === REFERENCIAS A OTROS COMPONENTES ===
//    [Header("Referencias")]
//    public ColumnWrapOnInput columnWrapper;
//    public SnakeColumnWrapOnInput snakeAttack;
//    public SnakeAttackAnticipation anticipation;
//    public TipoColorController tipoColorController;

//    // === CONFIGURACIÓN DE LAS COLUMNAS ===
//    [Header("Columnas por Vida")]
//    public Transform columna1, columna2, columna3;
//    [Header("Vidas de Columnas")]
//    public int vida1 = 1, vida2 = 1, vida3 = 1;
//    [Header("Slider de Vida de Columnas")]
//    public Slider vidaSlider;
//    [Header("Scripts tras Columnas")]
//    public MonoBehaviour[] scriptsToRemoveAfterColumns;
//    public MonoBehaviour[] scriptsToActivateAfterColumns;

//    // === CONFIGURACIÓN DE LA VIDA FINAL ===
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

//    // === PARÁMETROS DE DAÑO Y PARPADEO DE UI ===
//    [Header("Parpadeo al recibir daño")]
//    public float blinkDuration = 0.2f;
//    public float returnDuration = 0.1f;
//    public float blinkFrequency = 10f;
//    [Header("Daño por Bala")]
//    public int danioAmetralladora = 15;
//    public int danioPistola = 20;
//    public int danioEscopeta = 30;

//    // === CONFIGURACIÓN DE ATAQUE ===
//    [Header("Configuración de Ataque")]
//    public float attackStartDelay = 3f;
//    public float attackInterval = 2f;

//    // === ESTADO INTERNO ===
//    private int currentPhase;
//    private Coroutine phaseCoroutine, anticipationCoroutine;
//    private Image fillImage, finalFillImage;
//    private Color originalColor, finalOriginalColor;
//    private bool blinkInProgress, blinkFinalInProgress;

//    void Start()
//    {
//        if (columnWrapper == null || snakeAttack == null || anticipation == null
//            || tipoColorController == null || vidaSlider == null || finalLifeSlider == null)
//        {
//            Debug.LogError("Fase2Vida: faltan referencias asignadas en el Inspector.");
//            enabled = false;
//            return;
//        }

//        int totalCols = vida1 + vida2 + vida3;
//        vidaSlider.maxValue = totalCols;
//        vidaSlider.value = totalCols;
//        fillImage = vidaSlider.fillRect.GetComponent<Image>();
//        originalColor = fillImage.color;

//        finalLifeSlider.gameObject.SetActive(false);
//        finalLifeSlider.maxValue = vidaFinal;
//        finalLifeSlider.value = vidaFinal;
//        finalFillImage = finalLifeSlider.fillRect.GetComponent<Image>();
//        finalOriginalColor = finalFillImage.color;

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
//        else if (currentPhase == 3 && vidaFinal <= 0) TransitionToPhase(4, null);
//    }

//    private void TransitionToPhase(int nextPhase, Transform nextColumn)
//    {
//        currentPhase = nextPhase;

//        // Detener corutinas y desactivar envoltorios
//        if (phaseCoroutine != null) StopCoroutine(phaseCoroutine);
//        if (anticipationCoroutine != null) StopCoroutine(anticipationCoroutine);
//        StopAllCoroutines();
//        snakeAttack.StopAllCoroutines();
//        columnWrapper.StopAllCoroutines();
//        snakeAttack.enabled = false;
//        columnWrapper.enabled = false;

//        if (nextPhase == 3)
//        {
//            // Al terminar la última columna:
//            // 1) Asigna el jugador al SnakeController
//            var snakeCtrl = GetComponent<SnakeController>();
//            snakeCtrl.jugador = GameObject.FindWithTag("Player")?.transform;

//            // 2) Ajusta UI para vida final
//            foreach (var s in scriptsToRemoveAfterColumns) if (s) s.enabled = false;
//            foreach (var s in scriptsToActivateAfterColumns) if (s) s.enabled = true;
//            vidaSlider.gameObject.SetActive(false);
//            finalLifeSlider.gameObject.SetActive(true);

//            return;
//        }

//        if (nextPhase == 4)
//        {
//            // Detener spawn antes de animación
//            if (enemyGenerator != null)
//                enemyGenerator.stopSpawning = true;

//            // Desactivar/activar scripts generales
//            foreach (var s in scriptsToRemove) if (s) s.enabled = false;
//            foreach (var s in scriptsToActivate) if (s) s.enabled = true;
//            foreach (var s in scriptsToDestroyAfterFinal) if (s) Destroy(s);
//            foreach (var s in scriptsToActivateAfterFinal) if (s) s.enabled = true;

//            // Animación épica y dissolve
//            var cobraElevada = GetComponent<CobraElevadaController>();
//            if (cobraElevada) cobraElevada.enabled = false;
//            var epic = GetComponent<AnimacionMuerteCobraEpic>();
//            if (epic) { epic.enabled = true; epic.activarEpic = true; }
//            var dissolveCtrl = GetComponent<SnakeDissolveController>();
//            if (dissolveCtrl) { dissolveCtrl.enabled = true; dissolveCtrl.dissolveActive = true; }

//            return;
//        }

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
//        columnWrapper.TriggerWrap();
//        yield return new WaitForSeconds(columnWrapper.velocidadEnrollado + 0.1f);
//        yield return new WaitForSeconds(attackStartDelay);

//        var snakeCtrl = snakeAttack.GetComponent<SnakeController>();
//        var segmentsField = typeof(SnakeColumnWrapOnInput)
//                            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);
//        var preShake = typeof(SnakeColumnWrapOnInput)
//                            .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

//        bool firstLoop = true;
//        while (true)
//        {
//            if (!firstLoop) yield return new WaitForSeconds(attackInterval);
//            firstLoop = false;

//            anticipation.Initialize(snakeCtrl, columnWrapper);
//            anticipationCoroutine = StartCoroutine(anticipation.AnticipationRoutine());
//            yield return anticipationCoroutine;
//            anticipationCoroutine = null;

//            snakeAttack.enabled = true;
//            segmentsField.SetValue(snakeAttack, snakeCtrl.Segmentos);
//            StartCoroutine((IEnumerator)preShake.Invoke(snakeAttack, null));

//            float waitTime = snakeAttack.shakeDuration
//                           + snakeAttack.shakeToAttackDelay
//                           + snakeAttack.attackDuration;
//            yield return new WaitForSeconds(waitTime);
//        }
//    }

//    public void ApplyAbilityDamage(float damage)
//    {
//        switch (currentPhase)
//        {
//            case 0:
//                vida1 = Mathf.Max(0, vida1 - (int)damage);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 1:
//                vida2 = Mathf.Max(0, vida2 - (int)damage);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 2:
//                vida3 = Mathf.Max(0, vida3 - (int)damage);
//                vidaSlider.value = vida1 + vida2 + vida3;
//                if (!blinkInProgress) StartCoroutine(BlinkRoutine());
//                break;
//            case 3:
//                vidaFinal = Mathf.Max(0, vidaFinal - (int)damage);
//                finalLifeSlider.value = vidaFinal;
//                if (!blinkFinalInProgress) StartCoroutine(BlinkFinalRoutine());
//                break;
//        }
//        tipoColorController.RecibirDanio(0f);
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        var enemigo = (TipoColorController.TipoEnemigo)
//                      System.Enum.Parse(typeof(TipoColorController.TipoEnemigo), tipoBala.ToString());
//        if (tipoColorController.CurrentTipo == enemigo) return;

//        int danio = tipoBala switch
//        {
//            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
//            BalaPlayer.TipoBala.Pistola => danioPistola,
//            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
//            _ => 0
//        };

//        ApplyAbilityDamage(danio);
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


