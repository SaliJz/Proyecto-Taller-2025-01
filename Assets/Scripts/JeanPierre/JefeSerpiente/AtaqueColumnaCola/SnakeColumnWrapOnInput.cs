// SnakeColumnWrapOnInput.cs
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
    public float shakeDuration = 0.5f;
    public float shakeAmplitude = 30f;
    public float shakeFrequency = 10f;
    public float shakeToAttackDelay = 0.2f;

    [Header("Attack Settings")]
    public int numAttackSegments = 5;
    public float attackDistance = 1f;
    public float attackDuration = 1f;

    [Header("Return & Wrap Settings")]
    [Tooltip("Duración en segundos para el wrap y el retorno fluido de la cabeza")]
    public float returnAndWrapDuration = 1f;

    private SnakeController snake;
    private ColumnWrapOnInput columnWrapper;
    private List<Transform> segments;

    // Estados de ataque
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
        returnAndWrapDuration = columnWrapper.velocidadEnrollado;
    }

    void Update()
    {
        if (Input.GetKeyDown(attackKey) && !isShaking && !isAttacking)
        {
            segments = snake.Segmentos;
            if (segments.Count < 2) return;
            snake.enabled = false;
            StartCoroutine(PreAttackShake());
        }

        if (isAttacking)
            ApplyAttack();
    }

    private IEnumerator PreAttackShake()
    {
        isShaking = true;
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
        tail.localRotation = originalRot;
        isShaking = false;
        yield return new WaitForSeconds(shakeToAttackDelay);
        StartAttack();
    }

    void StartAttack()
    {
        numAttackSegments = Mathf.Clamp(numAttackSegments, 1, segments.Count - 1);
        var playerObj = GameObject.FindWithTag("Player");
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
        float t = Mathf.Clamp01(attackTimer / attackDuration);

        int wrapStartIndex = segments.Count - numAttackSegments;
        float lerpFactor = followLerpSpeed * Time.deltaTime;

        // Segmentos previos
        for (int i = 0; i < wrapStartIndex; i++)
        {
            Transform curr = segments[i];
            Transform prev = (i == 0) ? segments[0] : segments[i - 1];
            Vector3 dir = (prev.position - curr.position).normalized;
            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
        }

        // Segmentos de ataque y cola
        Transform lineOrigin = segments[wrapStartIndex - 1];
        var playerT = GameObject.FindWithTag("Player")?.transform;
        float distToPlayer = playerT != null
            ? Vector3.Distance(playerT.position, lineOrigin.position)
            : attackDistance * numAttackSegments;
        float maxTotal = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
        float step = maxTotal / numAttackSegments;

        for (int i = wrapStartIndex; i < segments.Count; i++)
        {
            Transform curr = segments[i];
            int rel = i - wrapStartIndex + 1;
            Vector3 tgt = lineOrigin.position + attackDir * (step * rel);
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);

            if (i == segments.Count - 1 && playerT != null)
            {
                // Cola: su atrás local (−Z) apuntará al jugador
                Vector3 dirToPlayer = (playerT.position - curr.position).normalized;
                curr.forward = -dirToPlayer;
            }
            else if ((tgt - curr.position).sqrMagnitude > 1e-4f)
            {
                curr.rotation = Quaternion.Slerp(
                    curr.rotation,
                    Quaternion.LookRotation((tgt - curr.position).normalized),
                    lerpFactor
                );
            }
        }

        // Cabeza dirigida al jugador
        if (segments.Count > 0 && playerT != null)
        {
            Transform head = segments[0];
            Vector3 lookPos = new Vector3(playerT.position.x, head.position.y, playerT.position.z);
            head.LookAt(lookPos);
        }

        if (t >= 1f)
            EndAttack();
    }

    void EndAttack()
    {
        isAttacking = false;
        StartCoroutine(ReturnAndWrap());
    }

    private IEnumerator ReturnAndWrap()
    {
        int segCount = segments.Count;
        List<Vector3> wrapStart = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            wrapStart.Add(segments[i].position);

        float alturaTotal = snake.distanciaCabezaCuerpo
                          + snake.separacionSegmentos * (segCount - 2)
                          + snake.separacionCola;
        float vueltas = columnWrapper.vueltasCompletas;
        float offset = columnWrapper.offsetRadio;
        float radio = columnWrapper.GetColumnRadius();
        float headOff = columnWrapper.headOffset;
        Transform col = columnWrapper.columna;

        List<Vector3> wrapTarget = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
        {
            float tt = 1f - (i / (float)(segCount - 1));
            float ang = tt * vueltas * 2 * Mathf.PI;
            float alt = tt * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            float extra = (i == 0) ? headOff : 0f;
            wrapTarget.Add(col.position + dir * (radio + offset + extra) + Vector3.up * alt);
        }

        float elapsed = 0f;
        while (elapsed < returnAndWrapDuration)
        {
            float frac = Mathf.SmoothStep(0f, 1f, elapsed / returnAndWrapDuration);
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = segments[i];
                seg.position = Vector3.Lerp(wrapStart[i], wrapTarget[i], frac);
                if (i == 0)
                {
                    var playerT2 = GameObject.FindWithTag("Player")?.transform;
                    if (playerT2 != null)
                    {
                        Vector3 lookPos = new Vector3(playerT2.position.x, seg.position.y, playerT2.position.z);
                        seg.LookAt(lookPos);
                    }
                }
                else if (i <= columnWrapper.segmentosCuello)
                    seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
                else
                {
                    Vector3 next = (i < segCount - 1)
                        ? wrapTarget[i + 1]
                        : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
                    seg.LookAt(next);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = segments[i];
            seg.position = wrapTarget[i];
            if (i == 0)
            {
                var playerT3 = GameObject.FindWithTag("Player")?.transform;
                if (playerT3 != null)
                {
                    Vector3 lookPos = new Vector3(playerT3.position.x, seg.position.y, playerT3.position.z);
                    seg.LookAt(lookPos);
                }
            }
            else if (i <= columnWrapper.segmentosCuello)
                seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
            else
            {
                Vector3 next = (i < segCount - 1)
                    ? wrapTarget[i + 1]
                    : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
                seg.LookAt(next);
            }
        }

        snake.ResetPositionHistory();
        snake.enabled = false;
    }
}

































//// SnakeColumnWrapOnInput.cs (con headOffset aplicado también en el retorno)
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//[RequireComponent(typeof(ColumnWrapOnInput))]
//public class SnakeColumnWrapOnInput : MonoBehaviour
//{
//    [Header("Input")]
//    public KeyCode attackKey = KeyCode.K;

//    [Header("Pre-Attack Feedback")]
//    public float shakeDuration = 0.5f;
//    public float shakeAmplitude = 30f;
//    public float shakeFrequency = 10f;
//    public float shakeToAttackDelay = 0.2f;

//    [Header("Attack Settings")]
//    public int numAttackSegments = 5;
//    public float attackDistance = 1f;
//    public float attackDuration = 1f;

//    [Header("Return & Wrap Settings")]
//    [Tooltip("Duración en segundos para el wrap y el retorno fluido de la cabeza")]
//    public float returnAndWrapDuration = 1f;

//    private SnakeController snake;
//    private ColumnWrapOnInput columnWrapper;
//    private List<Transform> segments;

//    // Estados de ataque
//    private bool isShaking = false;
//    private bool isAttacking = false;
//    private float attackTimer = 0f;
//    private Vector3 attackDir;
//    private float followLerpSpeed;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        columnWrapper = GetComponent<ColumnWrapOnInput>();
//        followLerpSpeed = attackDistance / attackDuration;
//        returnAndWrapDuration = columnWrapper.velocidadEnrollado;
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(attackKey) && !isShaking && !isAttacking)
//        {
//            segments = snake.Segmentos;
//            if (segments.Count < 2) return;
//            snake.enabled = false;
//            StartCoroutine(PreAttackShake());
//        }

//        if (isAttacking)
//            ApplyAttack();
//    }

//    private IEnumerator PreAttackShake()
//    {
//        isShaking = true;
//        Transform tail = segments[segments.Count - 1];
//        Quaternion originalRot = tail.localRotation;
//        float elapsed = 0f;
//        while (elapsed < shakeDuration)
//        {
//            float angle = Mathf.Sin(elapsed * shakeFrequency * 2f * Mathf.PI) * shakeAmplitude;
//            tail.localRotation = originalRot * Quaternion.Euler(0f, 0f, angle);
//            elapsed += Time.deltaTime;
//            yield return null;
//        }
//        tail.localRotation = originalRot;
//        isShaking = false;
//        yield return new WaitForSeconds(shakeToAttackDelay);
//        StartAttack();
//    }

//    void StartAttack()
//    {
//        numAttackSegments = Mathf.Clamp(numAttackSegments, 1, segments.Count - 1);
//        var playerObj = GameObject.FindWithTag("Player");
//        if (playerObj == null)
//        {
//            Debug.LogError("Player no encontrado.");
//            snake.enabled = true;
//            return;
//        }

//        int startIndex = segments.Count - numAttackSegments;
//        attackDir = (playerObj.transform.position - segments[startIndex].position).normalized;
//        attackTimer = 0f;
//        isAttacking = true;
//    }

//    void ApplyAttack()
//    {
//        attackTimer += Time.deltaTime;
//        float t = Mathf.Clamp01(attackTimer / attackDuration);

//        int wrapStartIndex = segments.Count - numAttackSegments;
//        float lerpFactor = followLerpSpeed * Time.deltaTime;

//        // Ajuste de segmentos previos
//        for (int i = 0; i < wrapStartIndex; i++)
//        {
//            Transform curr = segments[i];
//            Transform prev = (i == 0) ? segments[0] : segments[i - 1];
//            Vector3 dir = (prev.position - curr.position).normalized;
//            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
//            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
//            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
//        }

//        // Segmentos de ataque y cola
//        Transform lineOrigin = segments[wrapStartIndex - 1];
//        var playerT = GameObject.FindWithTag("Player")?.transform;
//        float distToPlayer = playerT != null
//            ? Vector3.Distance(playerT.position, lineOrigin.position)
//            : attackDistance * numAttackSegments;
//        float maxTotal = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
//        float step = maxTotal / numAttackSegments;

//        for (int i = wrapStartIndex; i < segments.Count; i++)
//        {
//            Transform curr = segments[i];
//            int rel = i - wrapStartIndex + 1;
//            Vector3 tgt = lineOrigin.position + attackDir * (step * rel);
//            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
//            if ((tgt - curr.position).sqrMagnitude > 1e-4f)
//                curr.rotation = Quaternion.Slerp(
//                    curr.rotation,
//                    Quaternion.LookRotation((tgt - curr.position).normalized),
//                    lerpFactor
//                );
//        }

//        // Orientar la cabeza al jugador mientras ataca
//        if (segments.Count > 0 && playerT != null)
//        {
//            Transform head = segments[0];
//            Vector3 lookPos = new Vector3(playerT.position.x, head.position.y, playerT.position.z);
//            head.LookAt(lookPos);
//        }

//        if (t >= 1f)
//            EndAttack();
//    }

//    void EndAttack()
//    {
//        isAttacking = false;
//        StartCoroutine(ReturnAndWrap());
//    }

//    private IEnumerator ReturnAndWrap()
//    {
//        int segCount = segments.Count;
//        List<Vector3> wrapStart = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//            wrapStart.Add(segments[i].position);

//        float alturaTotal = snake.distanciaCabezaCuerpo
//                          + snake.separacionSegmentos * (segCount - 2)
//                          + snake.separacionCola;
//        float vueltas = columnWrapper.vueltasCompletas;
//        float offset = columnWrapper.offsetRadio;
//        float radio = columnWrapper.GetColumnRadius();
//        float headOff = columnWrapper.headOffset;  // Aplicar headOffset
//        Transform col = columnWrapper.columna;

//        List<Vector3> wrapTarget = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//        {
//            float tt = 1f - (i / (float)(segCount - 1));
//            float ang = tt * vueltas * 2 * Mathf.PI;
//            float alt = tt * alturaTotal;
//            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
//            float extra = (i == 0) ? headOff : 0f;
//            wrapTarget.Add(col.position + dir * (radio + offset + extra) + Vector3.up * alt);
//        }

//        float elapsed = 0f;
//        while (elapsed < returnAndWrapDuration)
//        {
//            float frac = Mathf.SmoothStep(0f, 1f, elapsed / returnAndWrapDuration);

//            for (int i = 0; i < segCount; i++)
//            {
//                Transform seg = segments[i];
//                seg.position = Vector3.Lerp(wrapStart[i], wrapTarget[i], frac);

//                if (i == 0)
//                {
//                    var playerT2 = GameObject.FindWithTag("Player")?.transform;
//                    if (playerT2 != null)
//                    {
//                        Vector3 lookPos = new Vector3(playerT2.position.x, seg.position.y, playerT2.position.z);
//                        seg.LookAt(lookPos);
//                    }
//                }
//                else if (i <= columnWrapper.segmentosCuello)
//                    seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
//                else
//                {
//                    Vector3 next = (i < segCount - 1)
//                        ? wrapTarget[i + 1]
//                        : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
//                    seg.LookAt(next);
//                }
//            }

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        for (int i = 0; i < segCount; i++)
//        {
//            Transform seg = segments[i];
//            seg.position = wrapTarget[i];
//            if (i == 0)
//            {
//                var playerT3 = GameObject.FindWithTag("Player")?.transform;
//                if (playerT3 != null)
//                {
//                    Vector3 lookPos = new Vector3(playerT3.position.x, seg.position.y, playerT3.position.z);
//                    seg.LookAt(lookPos);
//                }
//            }
//            else if (i <= columnWrapper.segmentosCuello)
//                seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
//            else
//            {
//                Vector3 next = (i < segCount - 1)
//                    ? wrapTarget[i + 1]
//                    : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
//                seg.LookAt(next);
//            }
//        }

//        snake.ResetPositionHistory();
//        snake.enabled = false;
//    }
//}















