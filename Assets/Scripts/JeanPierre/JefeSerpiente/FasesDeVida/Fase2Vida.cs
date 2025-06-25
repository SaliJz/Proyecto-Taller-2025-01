using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(ColumnWrapOnInput))]
[RequireComponent(typeof(SnakeColumnWrapOnInput))]
public class Fase2Vida : MonoBehaviour
{
    [Header("Referencias")]
    public ColumnWrapOnInput columnWrapper;
    public SnakeColumnWrapOnInput snakeAttack;
    public TipoColorController tipoColorController; // Debe asignarse desde el Inspector

    [Header("Columnas por Vida")]
    [Tooltip("Columna para la primera vida")]
    public Transform columna1;
    [Tooltip("Columna para la segunda vida")]
    public Transform columna2;
    [Tooltip("Columna para la tercera vida")]
    public Transform columna3;

    [Header("Vidas")]
    [Tooltip("Vida 1: cuando llegue a 0, cambiar a columna2")]
    public int vida1 = 1;
    [Tooltip("Vida 2: cuando llegue a 0, cambiar a columna3")]
    public int vida2 = 1;
    [Tooltip("Vida 3: cuando llegue a 0, terminar")]
    public int vida3 = 1;

    [Header("Daño por Tipo de Bala (cuando NO coinciden)")]
    [Tooltip("Daño aplicado si la bala es Ametralladora y el tipo no coincide")]
    public int danioAmetralladora = 15;
    [Tooltip("Daño aplicado si la bala es Pistola y el tipo no coincide")]
    public int danioPistola = 20;
    [Tooltip("Daño aplicado si la bala es Escopeta y el tipo no coincide")]
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    [Tooltip("Componentes que se activarán 1 segundo después de la fase final")]
    public MonoBehaviour[] scriptsToActivate;

    [Header("Scripts a eliminar al morir")]
    [Tooltip("Componentes que se destruirán 1 segundo después de la fase final")]
    public MonoBehaviour[] scriptsToRemove;

    [Header("Configuración de Ataque Repetido")]
    [Tooltip("Tiempo de espera antes del primer ataque tras envolver columna")]
    public float initialAttackDelay = 2f;
    [Tooltip("Tiempo de espera entre ataques, después de terminar el anterior")]
    public float attackInterval = 2f;

    // Índice interno de fase: 0 = vida1; 1 = vida2; 2 = vida3; 3 = terminado
    private int currentPhase = 0;
    // Referencia a la coroutine activa (para poder detenerla)
    private Coroutine phaseCoroutine = null;

    private void Start()
    {
        // Validar referencias
        if (columnWrapper == null)
        {
            Debug.LogError("Fase2Vida: ColumnWrapOnInput no asignado.");
            enabled = false;
            return;
        }
        if (snakeAttack == null)
        {
            Debug.LogError("Fase2Vida: SnakeColumnWrapOnInput no asignado.");
            enabled = false;
            return;
        }
        if (tipoColorController == null)
        {
            Debug.LogError("Fase2Vida: TipoColorController no asignado.");
            enabled = false;
            return;
        }

        // Iniciar en la primera columna
        currentPhase = 0;
        columnWrapper.columna = columna1;
        // Arrancar la secuencia de la fase actual
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    private void Update()
    {
        // Verificar si debemos cambiar de fase según las vidas
        switch (currentPhase)
        {
            case 0:
                if (vida1 <= 0)
                {
                    // Cambiar a fase 1 (segunda vida)
                    TransitionToPhase(1, columna2);
                }
                break;
            case 1:
                if (vida2 <= 0)
                {
                    // Cambiar a fase 2 (tercera vida)
                    TransitionToPhase(2, columna3);
                }
                break;
            case 2:
                if (vida3 <= 0)
                {
                    // Fase 3 terminada, no hay más columnas
                    TransitionToPhase(3, null);
                }
                break;
            default:
                // currentPhase >= 3: nada que hacer
                break;
        }
    }

    /// <summary>
    /// Detiene la coroutine de fase actual y detiene cualquier ataque activo en snakeAttack.
    /// Luego arranca la siguiente fase, asignando la nueva columna.
    /// </summary>
    /// <param name="nextPhase">Índice de la próxima fase (0,1,2,3)</param>
    /// <param name="nextColumn">Transform de la columna para la siguiente fase (puede ser null si es fase final)</param>
    private void TransitionToPhase(int nextPhase, Transform nextColumn)
    {
        currentPhase = nextPhase;

        // Detener cualquier coroutine de fase en curso
        if (phaseCoroutine != null)
        {
            StopCoroutine(phaseCoroutine);
            phaseCoroutine = null;
        }

        // Detener cualquier ataque o sacudida en SnakeColumnWrapOnInput
        if (snakeAttack != null)
        {
            snakeAttack.enabled = false;
            snakeAttack.StopAllCoroutines();
        }

        // Si nextColumn es null, significa que no hay más fases (vida 0 en la última vida)
        if (nextColumn == null)
        {
            Debug.Log("Fase2Vida: Todas las vidas agotadas. Secuencia finalizada.");

            // Desactivar el ColumnWrapOnInput para que deje de mantener la serpiente alrededor de la columna
            if (columnWrapper != null)
            {
                columnWrapper.enabled = false;
                columnWrapper.StopAllCoroutines();
            }

            // Desactivar SnakeColumnWrapOnInput (por si aún quedaba alguna rutina interna)
            if (snakeAttack != null)
            {
                snakeAttack.enabled = false;
                snakeAttack.StopAllCoroutines();
            }

            // Habilitar el SnakeController para que la serpiente pase a moverse libremente
            SnakeController snakeCtrl = GetComponent<SnakeController>();
            if (snakeCtrl != null)
            {
                // Apuntar al objeto "Player" en lugar de a la columna
                GameObject playerObj = GameObject.FindWithTag("Player");
                if (playerObj != null)
                {
                    snakeCtrl.jugador = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning("Fase2Vida: No se encontró ningún GameObject con tag 'Player'.");
                }

                snakeCtrl.enabled = true;
            }
            else
            {
                Debug.LogWarning("Fase2Vida: No se encontró SnakeController para habilitar movimiento libre.");
            }

            // Iniciar coroutine que, tras 1 segundo, destruye y activa los scripts correspondientes
            StartCoroutine(HandleScriptArraysAfterDelay());

            return;
        }

        // Si hay siguiente columna, actualizar la columna en el wrapper y reiniciar la secuencia
        columnWrapper.columna = nextColumn;
        phaseCoroutine = StartCoroutine(ExecutePhase2Sequence());
    }

    /// <summary>
    /// Espera 1 segundo y luego destruye los scripts en scriptsToRemove y activa los de scriptsToActivate.
    /// </summary>
    private IEnumerator HandleScriptArraysAfterDelay()
    {
        yield return new WaitForSeconds(1f);

        // Eliminar scripts
        if (scriptsToRemove != null)
        {
            foreach (MonoBehaviour script in scriptsToRemove)
            {
                if (script != null)
                {
                    Destroy(script);
                }
            }
        }

        // Activar scripts
        if (scriptsToActivate != null)
        {
            foreach (MonoBehaviour script in scriptsToActivate)
            {
                if (script != null)
                {
                    script.enabled = true;
                }
            }
        }
    }

    /// <summary>
    /// Coroutine principal que envuelve la columna y luego entra en ciclo de ataques.
    /// </summary>
    private IEnumerator ExecutePhase2Sequence()
    {
        // 1) Iniciar comportamiento de enrollar en columna
        if (columnWrapper == null)
        {
            Debug.LogError("Fase2Vida: ColumnWrapOnInput no asignado.");
            yield break;
        }

        // Disparar el wrap sin esperar input
        columnWrapper.TriggerWrap();

        // Esperar a que termine el enrollado
        float wrapDuration = columnWrapper.velocidadEnrollado;
        yield return new WaitForSeconds(wrapDuration + 0.1f);

        // Añadir retraso antes del primer ataque para que no sea inmediato
        yield return new WaitForSeconds(initialAttackDelay);

        // 2) Repetir ciclo de ataque indefinidamente
        if (snakeAttack == null)
        {
            Debug.LogError("Fase2Vida: SnakeColumnWrapOnInput no asignado.");
            yield break;
        }

        // Asegurarse de que el componente de ataque esté habilitado
        snakeAttack.enabled = true;

        // Obtener la instancia de SnakeController para asignar segmentos
        SnakeController snakeCtrl = snakeAttack.GetComponent<SnakeController>();
        if (snakeCtrl == null)
        {
            Debug.LogError("Fase2Vida: SnakeController no encontrado en SnakeColumnWrapOnInput.");
            yield break;
        }

        // Reflection para acceso a campo privado "segments" dentro de SnakeColumnWrapOnInput
        FieldInfo segmentsField = typeof(SnakeColumnWrapOnInput)
            .GetField("segments", BindingFlags.NonPublic | BindingFlags.Instance);

        if (segmentsField == null)
        {
            Debug.LogError("Fase2Vida: No se encontró el campo 'segments' en SnakeColumnWrapOnInput.");
            yield break;
        }

        while (true)
        {
            // Antes de invocar PreAttackShake, asignar la lista de segmentos actual
            List<Transform> currentSegments = snakeCtrl.Segmentos;
            segmentsField.SetValue(snakeAttack, currentSegments);

            // Reflection para obtener el método privado PreAttackShake
            MethodInfo preShakeMethod = typeof(SnakeColumnWrapOnInput)
                .GetMethod("PreAttackShake", BindingFlags.NonPublic | BindingFlags.Instance);

            if (preShakeMethod != null)
            {
                // Iniciar la coroutine de sacudido y ataque
                IEnumerator preShakeCoroutine = (IEnumerator)preShakeMethod.Invoke(snakeAttack, null);
                StartCoroutine(preShakeCoroutine);

                // Calcular tiempo total que dura: sacudido + pausa + ataque
                float shakeDur = snakeAttack.shakeDuration;
                float delay = snakeAttack.shakeToAttackDelay;
                float atkDur = snakeAttack.attackDuration;
                float totalAttackTime = shakeDur + delay + atkDur;

                // Esperar a que termine la secuencia antes de repetir
                yield return new WaitForSeconds(totalAttackTime + attackInterval);
            }
            else
            {
                Debug.LogError("Fase2Vida: No se encontró PreAttackShake() en SnakeColumnWrapOnInput.");
                yield break;
            }
        }
    }

    /// <summary>
    /// Método público para recibir daño de una BalaPlayer.
    /// Verifica si el tipo de bala NO coincide con el tipo actual del jefe,
    /// aplica un valor de daño según el tipo de bala y luego dispara parpadeo.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        // Convertir TipoBala a TipoEnemigo para comparar contra el tipo actual
        TipoColorController.TipoEnemigo balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );

        // Aplicar daño solo si el tipo de bala NO coincide con el tipo actual
        if (tipoColorController.CurrentTipo != balaComoEnemigo)
        {
            // Disparar parpadeo en el controlador de color
            tipoColorController.RecibirDanio(0f);

            // Determinar cuánto daño aplicar según tipo de bala
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

            // Reducir la vida según la fase actual usando el valor de daño calculado
            switch (currentPhase)
            {
                case 0:
                    vida1 = Mathf.Max(vida1 - danioAplicado, 0);
                    break;
                case 1:
                    vida2 = Mathf.Max(vida2 - danioAplicado, 0);
                    break;
                case 2:
                    vida3 = Mathf.Max(vida3 - danioAplicado, 0);
                    break;
                default:
                    // Fase final: ignorar
                    break;
            }
        }
        else
        {
            Debug.Log("Bala de tipo correcto: no se aplica daño.");
        }
    }
}



