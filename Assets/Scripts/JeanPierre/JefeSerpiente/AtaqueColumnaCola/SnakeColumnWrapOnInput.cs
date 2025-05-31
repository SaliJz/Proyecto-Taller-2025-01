// SnakeColumnWrapOnInput.cs
// Ataque con cola limitada y transición fluida al wrap al finalizar,
// con pre-ataque de cascabel para la cola y pausa antes del ataque

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
[RequireComponent(typeof(ColumnWrapOnInput))]
public class SnakeColumnWrapOnInput : MonoBehaviour
{
    [Header("Input")]
    public KeyCode attackKey = KeyCode.K;

    [Header("Pre-Attack Feedback")]
    [Tooltip("Duración del sacudido de cola antes del ataque")]
    public float shakeDuration = 0.5f;
    [Tooltip("Amplitud en grados del sacudido de cola")]
    public float shakeAmplitude = 30f;
    [Tooltip("Frecuencia (oscilaciones por segundo) del sacudido de cola")]
    public float shakeFrequency = 10f;
    [Tooltip("Retardo entre fin del sacudido y comienzo del ataque")]
    public float shakeToAttackDelay = 0.2f;

    [Header("Attack Settings")]
    [Tooltip("Número de segmentos finales (excluyendo cabeza) que participarán en el ataque")]
    public int numAttackSegments = 5;
    [Tooltip("Distancia extra entre segmentos durante el ataque")]
    public float attackDistance = 1f;
    [Tooltip("Duración total del ataque en segundos")]
    public float attackDuration = 1f;

    private SnakeController snake;
    private ColumnWrapOnInput columnWrapper;
    private List<Transform> segments;
    private bool isShaking = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;
    private Vector3 attackDir;
    private float followLerpSpeed;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        columnWrapper = GetComponent<ColumnWrapOnInput>();
        followLerpSpeed = attackDistance / attackDuration;
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && !isShaking && !isAttacking)
        {
            // Inicia sacudido de cola antes del ataque
            segments = snake.Segmentos;
            if (segments.Count < 2)
                return;

            snake.enabled = false;
            StartCoroutine(PreAttackShake());
        }

        if (isAttacking)
            ApplyAttack();
    }

    private IEnumerator PreAttackShake()
    {
        isShaking = true;

        // Obtiene el segmento de la cola
        Transform tail = segments[segments.Count - 1];
        Quaternion originalRot = tail.localRotation;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float angle = Mathf.Sin(elapsed * shakeFrequency * 2f * Mathf.PI) * shakeAmplitude;
            tail.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restaurar rotación original
        tail.localRotation = originalRot;
        isShaking = false;

        // Pausa antes de iniciar el ataque
        yield return new WaitForSeconds(shakeToAttackDelay);

        // Tras sacudido y pausa, inicia ataque
        StartAttack();
    }

    void StartAttack()
    {
        numAttackSegments = Mathf.Clamp(numAttackSegments, 1, segments.Count - 1);

        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("Player no encontrado.");
            snake.enabled = true;
            return;
        }

        int startIndex = segments.Count - numAttackSegments;
        attackDir = (playerObj.transform.position - segments[startIndex].position).normalized;

        attackTimer = 0f;
        isAttacking = true;
    }

    void ApplyAttack()
    {
        attackTimer += Time.deltaTime;
        float lerpFactor = followLerpSpeed * Time.deltaTime;

        int startIndex = segments.Count - numAttackSegments;
        Vector3 lineOrigin = segments[startIndex - 1].position;

        // Distancia real hasta el jugador desde el punto de origen
        Transform playerT = GameObject.FindWithTag("Player").transform;
        float distToPlayer = Vector3.Distance(playerT.position, lineOrigin);
        // La distancia máxima total que cubrirán todos los segmentos
        float maxTotalDist = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
        // Paso equidistante
        float stepDist = maxTotalDist / numAttackSegments;

        // Mueve segmentos previos
        for (int i = 1; i < startIndex; i++)
        {
            Transform curr = segments[i];
            Transform prev = segments[i - 1];
            Vector3 dir = (prev.position - curr.position).normalized;
            Vector3 target = prev.position - dir * snake.separacionSegmentos;
            curr.position = Vector3.Lerp(curr.position, target, lerpFactor);
            curr.rotation = Quaternion.Slerp(
                curr.rotation,
                Quaternion.LookRotation(dir),
                lerpFactor
            );
        }

        // Mueve segmentos de ataque y cola equidistantes
        for (int i = startIndex; i < segments.Count; i++)
        {
            Transform curr = segments[i];
            int relIdx = i - startIndex + 1;
            Vector3 targetPos = lineOrigin + attackDir * (stepDist * relIdx);

            curr.position = Vector3.Lerp(curr.position, targetPos, lerpFactor);
            if ((targetPos - curr.position).sqrMagnitude > 0.0001f)
            {
                curr.rotation = Quaternion.Slerp(
                    curr.rotation,
                    Quaternion.LookRotation((targetPos - curr.position).normalized),
                    lerpFactor
                );
            }
        }

        if (attackTimer >= attackDuration)
            EndAttack();
    }

    void EndAttack()
    {
        isAttacking = false;
        StartCoroutine(TransitionToWrap());
    }

    private IEnumerator TransitionToWrap()
    {
        int segCount = segments.Count;
        List<Vector3> startPos = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            startPos.Add(segments[i].position);

        float alturaTotal = snake.distanciaCabezaCuerpo
                          + snake.separacionSegmentos * (segCount - 2)
                          + snake.separacionCola;

        float vueltas = columnWrapper.vueltasCompletas;
        float offset = columnWrapper.offsetRadio;
        float wrapDur = columnWrapper.velocidadEnrollado;
        float radio = columnWrapper.GetColumnRadius();
        Transform col = columnWrapper.columna;

        List<Vector3> targetPos = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
        {
            float t = 1f - ((float)i / (segCount - 1));
            float ang = t * vueltas * 2 * Mathf.PI;
            float alt = t * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 p = col.position + dir * (radio + offset) + Vector3.up * alt;
            targetPos.Add(p);
        }

        float elapsed = 0f;
        while (elapsed < wrapDur)
        {
            float frac = Mathf.SmoothStep(0f, 1f, elapsed / wrapDur);
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = segments[i];
                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

                if (i <= columnWrapper.segmentosCuello)
                    seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
                else
                {
                    Vector3 lookT = (i < segCount - 1)
                        ? targetPos[i + 1]
                        : col.position + Vector3.up * (targetPos[i].y + 0.1f);
                    seg.LookAt(lookT);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        for (int i = 0; i < segCount; i++)
        {
            Transform seg = segments[i];
            seg.position = targetPos[i];
            if (i <= columnWrapper.segmentosCuello)
                seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
            else
            {
                Vector3 lookT = (i < segCount - 1)
                    ? targetPos[i + 1]
                    : col.position + Vector3.up * (targetPos[i].y + 0.1f);
                seg.LookAt(lookT);
            }
        }

        snake.enabled = false;
    }
}




























