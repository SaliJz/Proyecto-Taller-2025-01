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










//using System.Collections;
//using System.Collections.Generic;
//using System.Reflection;
//using UnityEngine;

//[RequireComponent(typeof(ColumnWrapOnInput))]
//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//public class Fase2Vida : MonoBehaviour
//{
//    [Header("Referencias")]
//    public ColumnWrapOnInput columnWrapper;
//    public SnakeColumnWrapOnInput snakeAttack;
//    public TipoColorController tipoColorController; // Debe asignarse desde el Inspector

//    [Header("Columnas por Vida")]
//    [Tooltip("Columna para la primera vida")]
//    public Transform columna1;
//    [Tooltip("Columna para la segunda vida")]
//    public Transform columna2;
//    [Tooltip("Columna para la tercera vida")]
//    public Transform columna3;

//    [Header("Vidas")]
//    [Tooltip("Vida 1: cuando llegue a 0, cambiar a columna2")]
//    public int vida1 = 1;
//    [Tooltip("Vida 2: cuando llegue a 0, cambiar a columna3")]
//    public int vida2 = 1;
//    [Tooltip("Vida 3: cuando llegue a 0, terminar")]
//    public int vida3 = 1;

//    [Header("Daño por Tipo de Bala (cuando NO coinciden)")]
//    [Tooltip("Daño aplicado si la bala es Ametralladora y el tipo no coincide")]
//    public int danioAmetralladora = 15;
//    [Tooltip("Daño aplicado si la bala es Pistola y el tipo no coincide")]
//    public int danioPistola = 20;
//    [Tooltip("Daño aplicado si la bala es Escopeta y el tipo no coincide")]
//    public int danioEscopeta = 30;

//    [Header("Scripts a activar al morir")]
//    [Tooltip("Componentes que se activarán 1 segundo después de la fase final")]
//    public MonoBehaviour[] scriptsToActivate;

//    [Header("Scripts a eliminar al morir")]
//    [Tooltip("Componentes que se destruirán 1 segundo después de la fase final")]
//    public MonoBehaviour[] scriptsToRemove;

//    [Header("Configuración de Ataque Repetido")]
//    [Tooltip("Tiempo de espera antes del primer ataque tras envolver columna")]
//    public float initialAttackDelay = 2f;
//    [Tooltip("Tiempo de espera entre ataques, después de terminar el anterior")]
//    public float attackInterval = 2f;

//    // Índice interno de fase: 0 = vida1; 1 = vida2; 2 = vida3; 3 = terminado
//    private int currentPhase = 0;
//    // Referencia a la coroutine activa (para poder detenerla)
//    private Coroutine phaseCoroutine = null;

//    private void Start()
//    {
//        // Validar referencias
//        if (columnWrapper == null)
//        {
//            Debug.LogError("Fase2Vida: ColumnWrapOnInput no asignado.");
//            enabled = false;
//            return;
//        }
//        if (snakeAttack == null)
//        {
//            Debug.LogError("Fase2Vida: SnakeColumnWrapOnInput no asignado.");
//            enabled = false;
//            return;
//        }
//        if (tipoColorController == null)
//        {
//            Debug.LogError("Fase2Vida: TipoColorController no asignado.");
//            enabled = false;
//            return;
//        }

//        // Iniciar en la primera columna
//        currentPhase = 0;
//        columnWrapper.columna = columna1;
//        // Arrancar la secuencia de la fase actual
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    private void Update()
//    {
//        // Verificar si debemos cambiar de fase según las vidas
//        switch (currentPhase)
//        {
//            case 0:
//                if (vida1 <= 0)
//                {
//                    // Cambiar a fase 1 (segunda vida)
//                    TransitionToPhase(1, columna2);
//                }
//                break;
//            case 1:
//                if (vida2 <= 0)
//                {
//                    // Cambiar a fase 2 (tercera vida)
//                    TransitionToPhase(2, columna3);
//                }
//                break;
//            case 2:
//                if (vida3 <= 0)
//                {
//                    // Fase 3 terminada, no hay más columnas
//                    TransitionToPhase(3, null);
//                }
//                break;
//            default:
//                // currentPhase >= 3: nada que hacer
//                break;
//        }
//    }

//    /// <summary>
//    /// Detiene la coroutine de fase actual y detiene cualquier ataque activo en snakeAttack.
//    /// Luego arranca la siguiente fase, asignando la nueva columna.
//    /// </summary>
//    /// <param name="nextPhase">Índice de la próxima fase (0,1,2,3)</param>
//    /// <param name="nextColumn">Transform de la columna para la siguiente fase (puede ser null si es fase final)</param>
//    private void TransitionToPhase(int nextPhase, Transform nextColumn)
//    {
//        currentPhase = nextPhase;

//        // Detener cualquier coroutine de fase en curso
//        if (phaseCoroutine != null)
//        {
//            StopCoroutine(phaseCoroutine);
//            phaseCoroutine = null;
//        }

//        // Detener cualquier ataque o sacudida en SnakeColumnWrapOnInput
//        if (snakeAttack != null)
//        {
//            snakeAttack.enabled = false;
//            snakeAttack.StopAllCoroutines();
//        }

//        // Si nextColumn es null, significa que no hay más fases (vida 0 en la última vida)
//        if (nextColumn == null)
//        {
//            Debug.Log("Fase2Vida: Todas las vidas agotadas. Secuencia finalizada.");

//            // Desactivar el ColumnWrapOnInput para que deje de mantener la serpiente alrededor de la columna
//            if (columnWrapper != null)
//            {
//                columnWrapper.enabled = false;
//                columnWrapper.StopAllCoroutines();
//            }

//            // Desactivar SnakeColumnWrapOnInput (por si aún quedaba alguna rutina interna)
//            if (snakeAttack != null)
//            {
//                snakeAttack.enabled = false;
//                snakeAttack.StopAllCoroutines();
//            }

//            // Habilitar el SnakeController para que la serpiente pase a moverse libremente
//            SnakeController snakeCtrl = GetComponent<SnakeController>();
//            if (snakeCtrl != null)
//            {
//                // Apuntar al objeto "Player" en lugar de a la columna
//                GameObject playerObj = GameObject.FindWithTag("Player");
//                if (playerObj != null)
//                {
//                    snakeCtrl.jugador = playerObj.transform;
//                }
//                else
//                {
//                    Debug.LogWarning("Fase2Vida: No se encontró ningún GameObject con tag 'Player'.");
//                }

//                snakeCtrl.enabled = true;
//            }
//            else
//            {
//                Debug.LogWarning("Fase2Vida: No se encontró SnakeController para habilitar movimiento libre.");
//            }

//            // Iniciar coroutine que, tras 1 segundo, destruye y activa los scripts correspondientes
//            StartCoroutine(HandleScriptArraysAfterDelay());

//            return;
//        }

//        // Si hay siguiente columna, actualizar la columna en el wrapper y reiniciar la secuencia
//        columnWrapper.columna = nextColumn;
//        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
//    }

//    /// <summary>
//    /// Espera 1 segundo y luego destruye los scripts en scriptsToRemove y activa los de scriptsToActivate.
//    /// </summary>
//    private IEnumerator HandleScriptArraysAfterDelay()
//    {
//        yield return new WaitForSeconds(1f);

//        // Eliminar scripts
//        if (scriptsToRemove != null)
//        {
//            foreach (MonoBehaviour script in scriptsToRemove)
//            {
//                if (script != null)
//                {
//                    Destroy(script);
//                }
//            }
//        }

//        // Activar scripts
//        if (scriptsToActivate != null)
//        {
//            foreach (MonoBehaviour script in scriptsToActivate)
//            {
//                if (script != null)
//                {
//                    script.enabled = true;
//                }
//            }
//        }
//    }

//    /// <summary>
//    /// Coroutine principal que envuelve la columna y luego entra en ciclo de ataques.
//    /// </summary>
//    private IEnumerator ExecutePhase2Sequence()
//    {
//        // 1) Iniciar comportamiento de enrollar en columna
//        if (columnWrapper == null)
//        {
//            Debug.LogError("Fase2Vida: ColumnWrapOnInput no asignado.");
//            yield break;
//        }

//        // Disparar el wrap sin esperar input
//        columnWrapper.TriggerWrap();

//        // Esperar a que termine el enrollado
//        float wrapDuration = columnWrapper.velocidadEnrollado;
//        yield return new WaitForSeconds(wrapDuration + 0.1f);

//        // Añadir retraso antes del primer ataque para que no sea inmediato
//        yield return new WaitForSeconds(initialAttackDelay);

//        // 2) Repetir ciclo de ataque indefinidamente
//        if (snakeAttack == null)
//        {
//            Debug.LogError("Fase2Vida: SnakeColumnWrapOnInput no asignado.");
//            yield break;
//        }

//        // Asegurarse de que el componente de ataque esté habilitado
//        snakeAttack.enabled = true;

//        // Obtener la instancia de SnakeController para asignar segmentos
//        SnakeController snakeCtrl = snakeAttack.GetComponent<SnakeController>();
//        if (snakeCtrl == null)
//        {
//            Debug.LogError("Fase2Vida: SnakeController no encontrado en SnakeColumnWrapOnInput.");
//            yield break;
//        }

//        // Reflection para acceso a campo privado "segments" dentro de SnakeColumnWrapOnInput
//        FieldInfo segmentsField = typeof(SnakeColumnWrapOnInput)
//            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);

//        if (segmentsField == null)
//        {
//            Debug.LogError("Fase2Vida: No se encontró el campo 'segments' en SnakeColumnWrapOnInput.");
//            yield break;
//        }

//        while (true)
//        {
//            // Antes de invocar PreAttackShake, asignar la lista de segmentos actual
//            List<Transform> currentSegments = snakeCtrl.Segmentos;
//            segmentsField.SetValue(snakeAttack, currentSegments);

//            // Reflection para obtener el método privado PreAttackShake
//            MethodInfo preShakeMethod = typeof(SnakeColumnWrapOnInput)
//                .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

//            if (preShakeMethod != null)
//            {
//                // Iniciar la coroutine de sacudido y ataque
//                IEnumerator preShakeCoroutine = (IEnumerator)preShakeMethod.Invoke(snakeAttack, null);
//                StartCoroutine(preShakeCoroutine);

//                // Calcular tiempo total que dura: sacudido + pausa + ataque
//                float shakeDur = snakeAttack.shakeDuration;
//                float delay = snakeAttack.shakeToAttackDelay;
//                float atkDur = snakeAttack.attackDuration;
//                float totalAttackTime = shakeDur + delay + atkDur;

//                // Esperar a que termine la secuencia antes de repetir
//                yield return new WaitForSeconds(totalAttackTime + attackInterval);
//            }
//            else
//            {
//                Debug.LogError("Fase2Vida: No se encontró PreAttackShake() en SnakeColumnWrapOnInput.");
//                yield break;
//            }
//        }
//    }

//    /// <summary>
//    /// Método público para recibir daño de una BalaPlayer.
//    /// Verifica si el tipo de bala NO coincide con el tipo actual del jefe,
//    /// aplica un valor de daño según el tipo de bala y luego dispara parpadeo.
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
//    {
//        // Convertir TipoBala a TipoEnemigo para comparar contra el tipo actual
//        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
//            typeof(TipoColorController.TipoEnemigo),
//            tipoBala.ToString()
//        );

//        // Aplicar daño solo si el tipo de bala NO coincide con el tipo actual
//        if (tipoColorController.CurrentTipo != balaComoEnemigo)
//        {
//            // Disparar parpadeo en el controlador de color
//            tipoColorController.RecibirDanio(0f);

//            // Determinar cuánto daño aplicar según tipo de bala
//            int danioAplicado = 0;
//            switch (tipoBala)
//            {
//                case BalaPlayer.TipoBala.Ametralladora:
//                    danioAplicado = danioAmetralladora;
//                    break;
//                case BalaPlayer.TipoBala.Pistola:
//                    danioAplicado = danioPistola;
//                    break;
//                case BalaPlayer.TipoBala.Escopeta:
//                    danioAplicado = danioEscopeta;
//                    break;
//            }

//            // Reducir la vida según la fase actual usando el valor de daño calculado
//            switch (currentPhase)
//            {
//                case 0:
//                    vida1 = Mathf.Max(vida1 - danioAplicado, 0);
//                    break;
//                case 1:
//                    vida2 = Mathf.Max(vida2 - danioAplicado, 0);
//                    break;
//                case 2:
//                    vida3 = Mathf.Max(vida3 - danioAplicado, 0);
//                    break;
//                default:
//                    // Fase final: ignorar
//                    break;
//            }
//        }
//        else
//        {
//            Debug.Log("Bala de tipo correcto: no se aplica daño.");
//        }
//    }
//}



