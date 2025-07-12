// SnakeColumnWrapOnInput.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
[RequireComponent(typeof(ColumnWrapOnInput))]
[RequireComponent(typeof(AnimacionAnticipacionCola))]
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
    [Tooltip("Duracion en segundos para el wrap y el retorno fluido de la cabeza")]
    public float returnAndWrapDuration = 1f;

    private SnakeController snake;
    private ColumnWrapOnInput columnWrapper;
    private AnimacionAnticipacionCola anticipacionCola;
    private List<Transform> segments;

    // Estados de ataque
    private bool isShaking = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    // Posicion objetivo fija para el ataque
    private Vector3 attackTargetPosition;

    // Para activar/desactivar efectos
    private ActivadorEfectos efectosActivator;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        columnWrapper = GetComponent<ColumnWrapOnInput>();
        anticipacionCola = GetComponent<AnimacionAnticipacionCola>();
        returnAndWrapDuration = columnWrapper.velocidadEnrollado;
        efectosActivator = FindObjectOfType<ActivadorEfectos>();
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

        // Anticipacion visual: recarga cola
        if (anticipacionCola != null)
            yield return StartCoroutine(anticipacionCola.RutinaAnticipacion());

        // Sacudida de cola
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
        attackTimer = 0f;
        isAttacking = true;
        GameObject playerObj = GameObject.FindWithTag("Player");
        if (playerObj != null)
            attackTargetPosition = playerObj.transform.position;
        else
            attackTargetPosition = segments[segments.Count - numAttackSegments - 1].position + transform.forward * attackDistance;
    }

    void ApplyAttack()
    {
        attackTimer += Time.deltaTime;
        float t = Mathf.Clamp01(attackTimer / attackDuration);

        int wrapStartIndex = segments.Count - numAttackSegments;
        float lerpFactor = (attackDistance / attackDuration) * Time.deltaTime;

        Transform lineOrigin = segments[wrapStartIndex - 1];
        Vector3 dirToTarget = (attackTargetPosition - lineOrigin.position).normalized;
        float distToTarget = Vector3.Distance(attackTargetPosition, lineOrigin.position);
        float maxTotal = Mathf.Min(distToTarget, attackDistance * numAttackSegments);
        float step = maxTotal / numAttackSegments;

        // Movimiento segmento a segmento antes del ataque
        for (int i = 0; i < wrapStartIndex; i++)
        {
            Transform curr = segments[i];
            Transform prev = (i == 0) ? segments[0] : segments[i - 1];
            Vector3 dir = (prev.position - curr.position).normalized;
            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
        }

        // Ataque con objetivo fijo
        for (int i = wrapStartIndex; i < segments.Count; i++)
        {
            Transform curr = segments[i];
            int rel = i - wrapStartIndex + 1;
            Vector3 tgt = lineOrigin.position + dirToTarget * (step * rel);
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);

            if (i == segments.Count - 1)
                curr.forward = -dirToTarget;
            else if ((tgt - curr.position).sqrMagnitude > 1e-4f)
                curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation((tgt - curr.position).normalized), lerpFactor);
        }

        // Orientar cabeza hacia la posicion guardada
        if (segments.Count > 0)
        {
            Transform head = segments[0];
            Vector3 lookPos = new Vector3(attackTargetPosition.x, head.position.y, attackTargetPosition.z);
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
        if (efectosActivator != null)
            efectosActivator.activar = false;

        int segCount = segments.Count;
        List<Vector3> wrapStart = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            wrapStart.Add(segments[i].position);

        float alturaTotal =
            snake.distanciaCabezaCuerpo +
            snake.separacionSegmentos * (segCount - 2) +
            snake.separacionCola;
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







//// SnakeColumnWrapOnInput.cs
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

//    // Posición objetivo fija para el ataque
//    private Vector3 attackTargetPosition;

//    // Para activar/desactivar efectos
//    private ActivadorEfectos efectosActivator;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        columnWrapper = GetComponent<ColumnWrapOnInput>();
//        returnAndWrapDuration = columnWrapper.velocidadEnrollado;

//        // Referencia al ActivadorEfectos
//        efectosActivator = FindObjectOfType<ActivadorEfectos>();
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
//        attackTimer = 0f;
//        isAttacking = true;
//        GameObject playerObj = GameObject.FindWithTag("Player");
//        if (playerObj != null)
//            attackTargetPosition = playerObj.transform.position;
//        else
//            attackTargetPosition = segments[segments.Count - numAttackSegments - 1].position + transform.forward * attackDistance;
//    }

//    void ApplyAttack()
//    {
//        attackTimer += Time.deltaTime;
//        float t = Mathf.Clamp01(attackTimer / attackDuration);

//        int wrapStartIndex = segments.Count - numAttackSegments;
//        float lerpFactor = (attackDistance / attackDuration) * Time.deltaTime;

//        Transform lineOrigin = segments[wrapStartIndex - 1];
//        Vector3 dirToTarget = (attackTargetPosition - lineOrigin.position).normalized;
//        float distToTarget = Vector3.Distance(attackTargetPosition, lineOrigin.position);
//        float maxTotal = Mathf.Min(distToTarget, attackDistance * numAttackSegments);
//        float step = maxTotal / numAttackSegments;

//        // Movimiento segmento a segmento antes del ataque
//        for (int i = 0; i < wrapStartIndex; i++)
//        {
//            Transform curr = segments[i];
//            Transform prev = (i == 0) ? segments[0] : segments[i - 1];
//            Vector3 dir = (prev.position - curr.position).normalized;
//            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
//            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
//            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
//        }

//        // Ataque con objetivo fijo
//        for (int i = wrapStartIndex; i < segments.Count; i++)
//        {
//            Transform curr = segments[i];
//            int rel = i - wrapStartIndex + 1;
//            Vector3 tgt = lineOrigin.position + dirToTarget * (step * rel);
//            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);

//            if (i == segments.Count - 1)
//            {
//                curr.forward = -dirToTarget;
//            }
//            else if ((tgt - curr.position).sqrMagnitude > 1e-4f)
//            {
//                curr.rotation = Quaternion.Slerp(
//                    curr.rotation,
//                    Quaternion.LookRotation((tgt - curr.position).normalized),
//                    lerpFactor
//                );
//            }
//        }

//        // Orientar cabeza hacia la posición guardada
//        if (segments.Count > 0)
//        {
//            Transform head = segments[0];
//            Vector3 lookPos = new Vector3(attackTargetPosition.x, head.position.y, attackTargetPosition.z);
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
//        // Al comenzar el retorno (“subir”): desactivar efectos
//        if (efectosActivator != null)
//            efectosActivator.activar = false;

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
//        float headOff = columnWrapper.headOffset;
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

//        // Ajuste final
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



































