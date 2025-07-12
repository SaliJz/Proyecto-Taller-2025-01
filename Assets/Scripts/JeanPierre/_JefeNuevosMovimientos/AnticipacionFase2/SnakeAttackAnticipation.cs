// SnakeAttackAnticipation.cs
//
// Ejecuta la fase de anticipación en “U” justo antes del ataque,
// y tras ella llama a PreAttackShake y deja que el ataque continúe fluidamente.

using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

[RequireComponent(typeof(SnakeColumnWrapOnInput))]
[RequireComponent(typeof(ColumnWrapOnInput))]
public class SnakeAttackAnticipation : MonoBehaviour
{
    [Header("Referencias (auto-asignadas si faltan)")]
    public SnakeColumnWrapOnInput snakeAttack;
    public ColumnWrapOnInput columnWrapper;

    [Header("Anticipación en U")]
    [Tooltip("Cuántos de los últimos segmentos saldrán del enrollado.")]
    public int anticipationSegmentsCount = 5;
    [Tooltip("Duración (s) de la animación de anticipación.")]
    public float anticipationDuration = 0.5f;
    [Tooltip("Radio horizontal de la U.")]
    public float uRadius = 1f;
    [Tooltip("Altura vertical de la U.")]
    public float uHeight = 1f;

    private SnakeController snake;
    private List<Transform> segments;
    private bool hasAnticipated = false;

    // Reflexión para miembros privados de SnakeColumnWrapOnInput
    private MethodInfo preShakeMethod;
    private FieldInfo isShakingField;
    private FieldInfo isAttackingField;

    void Awake()
    {
        snake = GetComponent<SnakeController>();

        if (snakeAttack == null)
            snakeAttack = GetComponent<SnakeColumnWrapOnInput>();
        if (columnWrapper == null)
            columnWrapper = GetComponent<ColumnWrapOnInput>();

        var t = typeof(SnakeColumnWrapOnInput);
        preShakeMethod = t.GetMethod("PreAttackShake", BindingFlags.Instance | BindingFlags.NonPublic);
        isShakingField = t.GetField("isShaking", BindingFlags.Instance | BindingFlags.NonPublic);
        isAttackingField = t.GetField("isAttacking", BindingFlags.Instance | BindingFlags.NonPublic);
    }

    void OnEnable()
    {
        hasAnticipated = false;
        StartCoroutine(WatchForAttackStart());
    }

    private IEnumerator WatchForAttackStart()
    {
        // Espera un frame a que snakeAttack inicie su ciclo
        yield return null;

        while (!hasAnticipated)
        {
            bool shaking = (bool)isShakingField.GetValue(snakeAttack);
            bool attacking = (bool)isAttackingField.GetValue(snakeAttack);

            // Cuando snakeAttack está listo pero aún no ha temblado ni atacado, anticipamos
            if (snakeAttack.enabled && !shaking && !attacking)
            {
                yield return StartCoroutine(AnticipationAndAttackFlow());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator AnticipationAndAttackFlow()
    {
        hasAnticipated = true;

        // Obtener todos los segmentos
        segments = snake.Segmentos;
        if (segments.Count == 0) yield break;

        // Ajustar el conteo de segmentos a mover
        int total = segments.Count;
        int startIndex = Mathf.Max(0, total - anticipationSegmentsCount);
        anticipationSegmentsCount = total - startIndex;

        // Guardar posiciones iniciales
        Vector3[] startPos = new Vector3[anticipationSegmentsCount];
        for (int i = 0; i < anticipationSegmentsCount; i++)
            startPos[i] = segments[startIndex + i].position;

        // Calcular posiciones en U vertical
        Vector3 center = columnWrapper.columna.position;
        Vector3 right = columnWrapper.columna.right;
        Vector3 up = Vector3.up;
        Vector3[] targetPos = new Vector3[anticipationSegmentsCount];
        for (int i = 0; i < anticipationSegmentsCount; i++)
        {
            float t = anticipationSegmentsCount > 1
                      ? i / (float)(anticipationSegmentsCount - 1)
                      : 0.5f;
            float angle = Mathf.Lerp(-Mathf.PI / 2, Mathf.PI / 2, t);
            Vector3 offset = right * (Mathf.Cos(angle) * uRadius)
                           + up * (Mathf.Sin(angle) * uHeight);
            targetPos[i] = center + offset;
        }

        // Desactivar ColumnWrapOnInput para liberar control de posición
        columnWrapper.enabled = false;

        // Animar transición a U
        float elapsed = 0f;
        while (elapsed < anticipationDuration)
        {
            float frac = Mathf.SmoothStep(0f, 1f, elapsed / anticipationDuration);
            for (int i = 0; i < anticipationSegmentsCount; i++)
            {
                Transform seg = segments[startIndex + i];
                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);
                Vector3 look = (i < anticipationSegmentsCount - 1)
                    ? targetPos[i + 1]
                    : center + up * (uHeight * 1.2f);
                seg.LookAt(look);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }
        // Posición final de anticipación
        for (int i = 0; i < anticipationSegmentsCount; i++)
            segments[startIndex + i].position = targetPos[i];

        // Reactivar ColumnWrap y SnakeAttack
        columnWrapper.enabled = true;
        snakeAttack.enabled = true;

        // Invocar y esperar PreAttackShake
        var shakeCoro = (IEnumerator)preShakeMethod.Invoke(snakeAttack, null);
        yield return StartCoroutine(shakeCoro);

        // Después de PreAttackShake, SnakeColumnWrapOnInput internamente
        // llama a StartAttack() y el ataque continúa fluidamente en Update().
    }
}
