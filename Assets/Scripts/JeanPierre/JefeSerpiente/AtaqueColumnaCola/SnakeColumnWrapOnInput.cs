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

    // Para movimiento de cabeza
    private Vector3 headAttackStartPos;
    // Para wrap
    private float storedHeadOffset;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        columnWrapper = GetComponent<ColumnWrapOnInput>();
        followLerpSpeed = attackDistance / attackDuration;
        // Sincronizamos duración por defecto con la velocidad de enrollado
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
        headAttackStartPos = segments[0].position;
        attackTimer = 0f;
        isAttacking = true;
    }

    void ApplyAttack()
    {
        attackTimer += Time.deltaTime;
        float t = Mathf.Clamp01(attackTimer / attackDuration);

        // Mover cabeza en ataque
        {
            var head = segments[0];
            Vector3 target = headAttackStartPos + attackDir * attackDistance;
            head.position = Vector3.Lerp(headAttackStartPos, target, t);
            head.rotation = Quaternion.Slerp(head.rotation, Quaternion.LookRotation(attackDir), t);
        }

        // Segments previos
        int wrapStartIndex = segments.Count - numAttackSegments;
        float lerpFactor = followLerpSpeed * Time.deltaTime;
        for (int i = 1; i < wrapStartIndex; i++)
        {
            Transform curr = segments[i], prev = segments[i - 1];
            Vector3 dir = (prev.position - curr.position).normalized;
            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
        }

        // Segmentos de ataque y cola
        Transform lineOrigin = segments[wrapStartIndex - 1];
        var playerObj2 = GameObject.FindWithTag("Player");
        Transform playerT = playerObj2 ? playerObj2.transform : null;
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
            if ((tgt - curr.position).sqrMagnitude > 1e-4f)
                curr.rotation = Quaternion.Slerp(curr.rotation,
                    Quaternion.LookRotation((tgt - curr.position).normalized),
                    lerpFactor);
        }

        if (t >= 1f)
            EndAttack();
    }

    void EndAttack()
    {
        isAttacking = false;
        // Iniciamos el retorno suave de la cabeza + wrap conjunto
        StartCoroutine(ReturnHeadAndWrap());
    }

    private IEnumerator ReturnHeadAndWrap()
    {
        // Preparamos datos de wrap
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
        Transform col = columnWrapper.columna;
        var playerT = GameObject.FindWithTag("Player")?.transform;

        // Objetivos de wrap
        List<Vector3> wrapTarget = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
        {
            float tt = 1f - (i / (float)(segCount - 1));
            float ang = tt * vueltas * 2 * Mathf.PI;
            float alt = tt * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            float extra = (i == 0) ? storedHeadOffset : 0f;
            wrapTarget.Add(col.position + dir * (radio + offset + extra) + Vector3.up * alt);
        }

        // Corremos el bucle unificado de retorno de cabeza + wrap
        float elapsed = 0f;
        while (elapsed < returnAndWrapDuration)
        {
            float frac = Mathf.SmoothStep(0f, 1f, elapsed / returnAndWrapDuration);

            for (int i = 0; i < segCount; i++)
            {
                Transform seg = segments[i];
                if (i == 0)
                {
                    // cabeza: va desde su lugar al inicio del ataque → headAttackStartPos
                    seg.position = Vector3.Lerp(wrapStart[i], headAttackStartPos, frac);
                    // sigue mirando al jugador durante el retorno
                    if (playerT != null)
                        seg.LookAt(new Vector3(playerT.position.x, seg.position.y, playerT.position.z));
                }
                else
                {
                    // resto de segmentos: del start al wrapTarget
                    seg.position = Vector3.Lerp(wrapStart[i], wrapTarget[i], frac);

                    // orientaciones según posición en la columna
                    if (i <= columnWrapper.segmentosCuello)
                        seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
                    else
                    {
                        Vector3 next = (i < segCount - 1)
                            ? wrapTarget[i + 1]
                            : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
                        seg.LookAt(next);
                    }
                }
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final: posición exacta
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = segments[i];
            if (i == 0)
            {
                seg.position = headAttackStartPos;
                if (playerT != null)
                    seg.LookAt(new Vector3(playerT.position.x, seg.position.y, playerT.position.z));
            }
            else
            {
                seg.position = wrapTarget[i];
                if (i <= columnWrapper.segmentosCuello)
                    seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
                else
                {
                    Vector3 next = (i < segCount - 1)
                        ? wrapTarget[i + 1]
                        : col.position + Vector3.up * (wrapTarget[i].y + 0.1f);
                    seg.LookAt(next);
                }
            }
        }

        snake.enabled = false;
    }
}



//// Ataque con cola limitada y transición fluida al wrap al finalizar,
//// con pre-ataque de cascabel, cabeza que ataca desde su posición original
//// y luego vuelve a la posición correcta en la columna.

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

//    private SnakeController snake;
//    private ColumnWrapOnInput columnWrapper;
//    private List<Transform> segments;

//    // Estados
//    private bool isShaking = false;
//    private bool isAttacking = false;
//    private float attackTimer = 0f;
//    private Vector3 attackDir;
//    private float followLerpSpeed;

//    // Para movimiento de cabeza
//    private Vector3 headAttackStartPos;

//    // Para wrap
//    private float storedHeadOffset;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        columnWrapper = GetComponent<ColumnWrapOnInput>();
//        followLerpSpeed = attackDistance / attackDuration;
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

//        GameObject playerObj = GameObject.FindWithTag("Player");
//        if (playerObj == null)
//        {
//            Debug.LogError("Player no encontrado.");
//            snake.enabled = true;
//            return;
//        }

//        int startIndex = segments.Count - numAttackSegments;
//        attackDir = (playerObj.transform.position - segments[startIndex].position).normalized;

//        // Guardamos posición inicial de la cabeza
//        headAttackStartPos = segments[0].position;

//        attackTimer = 0f;
//        isAttacking = true;
//    }

//    void ApplyAttack()
//    {
//        attackTimer += Time.deltaTime;
//        float t = Mathf.Clamp01(attackTimer / attackDuration);

//        // 1) Cabeza: del startPos hasta startPos + attackDir * attackDistance
//        {
//            Transform head = segments[0];
//            Vector3 target = headAttackStartPos + attackDir * attackDistance;
//            head.position = Vector3.Lerp(headAttackStartPos, target, t);
//            head.rotation = Quaternion.Slerp(
//                head.rotation,
//                Quaternion.LookRotation(attackDir),
//                t
//            );
//        }

//        // 2) Segmentos previos: igual que antes, siguiendo a su anterior
//        int startIndex = segments.Count - numAttackSegments;
//        float lerpFactor = followLerpSpeed * Time.deltaTime;

//        for (int i = 1; i < startIndex; i++)
//        {
//            Transform curr = segments[i];
//            Transform prev = segments[i - 1];
//            Vector3 dir = (prev.position - curr.position).normalized;
//            Vector3 target = prev.position - dir * snake.separacionSegmentos;
//            curr.position = Vector3.Lerp(curr.position, target, lerpFactor);
//            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
//        }

//        // 3) Segmentos de ataque y cola
//        Transform lineOrigin = segments[startIndex - 1];
//        GameObject playerObj2 = GameObject.FindWithTag("Player");
//        Transform playerT = playerObj2 ? playerObj2.transform : null;
//        float distToPlayer = playerT != null
//            ? Vector3.Distance(playerT.position, lineOrigin.position)
//            : attackDistance * numAttackSegments;
//        float maxTotal = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
//        float step = maxTotal / numAttackSegments;

//        for (int i = startIndex; i < segments.Count; i++)
//        {
//            Transform curr = segments[i];
//            int rel = i - startIndex + 1;
//            Vector3 targetPos = lineOrigin.position + attackDir * (step * rel);
//            curr.position = Vector3.Lerp(curr.position, targetPos, lerpFactor);
//            if ((targetPos - curr.position).sqrMagnitude > 1e-4f)
//                curr.rotation = Quaternion.Slerp(
//                    curr.rotation,
//                    Quaternion.LookRotation((targetPos - curr.position).normalized),
//                    lerpFactor
//                );
//        }

//        if (t >= 1f)
//            EndAttack();
//    }

//    void EndAttack()
//    {
//        isAttacking = false;
//        StartCoroutine(InitWrapWithStoredOffset());
//    }

//    // Antes de wrap, guardamos la separación radial original de la cabeza
//    private IEnumerator InitWrapWithStoredOffset()
//    {
//        Transform head = segments[0];
//        Vector3 colPos = columnWrapper.columna.position;
//        Vector3 headXZ = new Vector3(head.position.x, colPos.y, head.position.z);
//        float radialDist = Vector3.Distance(headXZ, colPos);
//        storedHeadOffset = radialDist - (columnWrapper.GetColumnRadius() + columnWrapper.offsetRadio);

//        yield return StartCoroutine(TransitionToWrap());
//    }

//    private IEnumerator TransitionToWrap()
//    {
//        int segCount = segments.Count;
//        List<Vector3> startPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//            startPos.Add(segments[i].position);

//        float alturaTotal = snake.distanciaCabezaCuerpo
//                          + snake.separacionSegmentos * (segCount - 2)
//                          + snake.separacionCola;
//        float vueltas = columnWrapper.vueltasCompletas;
//        float offset = columnWrapper.offsetRadio;
//        float wrapDur = columnWrapper.velocidadEnrollado;
//        float radio = columnWrapper.GetColumnRadius();
//        Transform col = columnWrapper.columna;
//        Transform player = GameObject.FindWithTag("Player")?.transform;

//        // Objetivos
//        List<Vector3> targetPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//        {
//            float tt = 1f - ((float)i / (segCount - 1));
//            float ang = tt * vueltas * 2 * Mathf.PI;
//            float alt = tt * alturaTotal;
//            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
//            float extra = (i == 0) ? storedHeadOffset : 0f;
//            Vector3 p = col.position + dir * (radio + offset + extra) + Vector3.up * alt;
//            targetPos.Add(p);
//        }

//        float elapsed = 0f;
//        while (elapsed < wrapDur)
//        {
//            float frac = Mathf.SmoothStep(0f, 1f, elapsed / wrapDur);
//            for (int i = 0; i < segCount; i++)
//            {
//                Transform seg = segments[i];
//                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

//                if (i == 0 && player != null)
//                {
//                    Vector3 look = new Vector3(player.position.x, seg.position.y, player.position.z);
//                    seg.LookAt(look);
//                }
//                else if (i <= columnWrapper.segmentosCuello)
//                {
//                    Vector3 colLook = new Vector3(col.position.x, seg.position.y, col.position.z);
//                    seg.LookAt(colLook);
//                }
//                else
//                {
//                    Vector3 next = (i < segCount - 1)
//                        ? targetPos[i + 1]
//                        : col.position + Vector3.up * (targetPos[i].y + 0.1f);
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
//            seg.position = targetPos[i];
//            if (i == 0 && player != null)
//            {
//                Vector3 look = new Vector3(player.position.x, seg.position.y, player.position.z);
//                seg.LookAt(look);
//            }
//            else if (i <= columnWrapper.segmentosCuello)
//            {
//                Vector3 colLook = new Vector3(col.position.x, seg.position.y, col.position.z);
//                seg.LookAt(colLook);
//            }
//            else
//            {
//                Vector3 next = (i < segCount - 1)
//                    ? targetPos[i + 1]
//                    : col.position + Vector3.up * (targetPos[i].y + 0.1f);
//                seg.LookAt(next);
//            }
//        }

//        snake.enabled = false;
//    }
//}


























//// SnakeColumnWrapOnInput.cs
//// Ataque con cola limitada y transición fluida al wrap al finalizar,
//// con pre-ataque de cascabel para la cola y pausa antes del ataque,
//// + headOffset dinámico que respeta la posición final de la cabeza tras el ataque.

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

//    private SnakeController snake;
//    private ColumnWrapOnInput columnWrapper;
//    private List<Transform> segments;
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
//        GameObject playerObj = GameObject.FindWithTag("Player");
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
//        float lerpFactor = followLerpSpeed * Time.deltaTime;
//        int startIndex = segments.Count - numAttackSegments;
//        Vector3 lineOrigin = segments[startIndex - 1].position;
//        Transform playerT = GameObject.FindWithTag("Player").transform;
//        float distToPlayer = Vector3.Distance(playerT.position, lineOrigin);
//        float maxTotal = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
//        float step = maxTotal / numAttackSegments;

//        // Mueve segmentos previos
//        for (int i = 1; i < startIndex; i++)
//        {
//            Transform curr = segments[i];
//            Transform prev = segments[i - 1];
//            Vector3 dir = (prev.position - curr.position).normalized;
//            Vector3 target = prev.position - dir * snake.separacionSegmentos;
//            curr.position = Vector3.Lerp(curr.position, target, lerpFactor);
//            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
//        }

//        // Mueve segmentos de ataque y cola
//        for (int i = startIndex; i < segments.Count; i++)
//        {
//            Transform curr = segments[i];
//            int rel = i - startIndex + 1;
//            Vector3 targetPos = lineOrigin + attackDir * (step * rel);
//            curr.position = Vector3.Lerp(curr.position, targetPos, lerpFactor);
//            if ((targetPos - curr.position).sqrMagnitude > 1e-4f)
//                curr.rotation = Quaternion.Slerp(curr.rotation,
//                    Quaternion.LookRotation((targetPos - curr.position).normalized),
//                    lerpFactor);
//        }

//        if (attackTimer >= attackDuration)
//            EndAttack();
//    }

//    void EndAttack()
//    {
//        isAttacking = false;
//        StartCoroutine(TransitionToWrap());
//    }

//    private IEnumerator TransitionToWrap()
//    {
//        int segCount = segments.Count;
//        List<Vector3> startPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//            startPos.Add(segments[i].position);

//        // Calculamos la altura total y datos de la columna
//        float alturaTotal = snake.distanciaCabezaCuerpo
//                          + snake.separacionSegmentos * (segCount - 2)
//                          + snake.separacionCola;
//        float vueltas = columnWrapper.vueltasCompletas;
//        float offset = columnWrapper.offsetRadio;
//        float wrapDur = columnWrapper.velocidadEnrollado;
//        float radio = columnWrapper.GetColumnRadius();
//        Transform col = columnWrapper.columna;
//        Transform player = GameObject.FindWithTag("Player")?.transform;

//        // --- NUEVO: calculamos dinámicamente cuánto sobresale la cabeza al terminar el ataque ---
//        Vector3 headPos = segments[0].position;
//        Vector3 headXZ = new Vector3(headPos.x, col.position.y, headPos.z);
//        float radialDist = Vector3.Distance(headXZ, col.position);
//        float dynamicOffset = radialDist - (radio + offset);
//        // ----------------------------------------------------------------------------

//        // Calculamos posiciones objetivo usando dynamicOffset para la cabeza
//        List<Vector3> targetPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//        {
//            float t = 1f - ((float)i / (segCount - 1));
//            float ang = t * vueltas * 2 * Mathf.PI;
//            float alt = t * alturaTotal;
//            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));

//            float extra = (i == 0) ? dynamicOffset : 0f;
//            Vector3 p = col.position + dir * (radio + offset + extra) + Vector3.up * alt;
//            targetPos.Add(p);
//        }

//        // Animación de interpolación y orientación
//        float elapsed = 0f;
//        while (elapsed < wrapDur)
//        {
//            float frac = Mathf.SmoothStep(0f, 1f, elapsed / wrapDur);
//            for (int i = 0; i < segCount; i++)
//            {
//                Transform seg = segments[i];
//                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

//                if (i == 0 && player != null)
//                {
//                    Vector3 look = new Vector3(player.position.x, seg.position.y, player.position.z);
//                    seg.LookAt(look);
//                }
//                else if (i <= columnWrapper.segmentosCuello)
//                {
//                    Vector3 colLook = new Vector3(col.position.x, seg.position.y, col.position.z);
//                    seg.LookAt(colLook);
//                }
//                else
//                {
//                    Vector3 next = (i < segCount - 1)
//                        ? targetPos[i + 1]
//                        : col.position + Vector3.up * (targetPos[i].y + 0.1f);
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
//            seg.position = targetPos[i];

//            if (i == 0 && player != null)
//            {
//                Vector3 look = new Vector3(player.position.x, seg.position.y, player.position.z);
//                seg.LookAt(look);
//            }
//            else if (i <= columnWrapper.segmentosCuello)
//            {
//                Vector3 colLook = new Vector3(col.position.x, seg.position.y, col.position.z);
//                seg.LookAt(colLook);
//            }
//            else
//            {
//                Vector3 next = (i < segCount - 1)
//                    ? targetPos[i + 1]
//                    : col.position + Vector3.up * (targetPos[i].y + 0.1f);
//                seg.LookAt(next);
//            }
//        }

//        snake.enabled = false;
//    }
//}

























//// SnakeColumnWrapOnInput.cs
//// Ataque con cola limitada y transición fluida al wrap al finalizar,
//// con pre-ataque de cascabel para la cola y pausa antes del ataque

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
//    [Tooltip("Duración del sacudido de cola antes del ataque")]
//    public float shakeDuration = 0.5f;
//    [Tooltip("Amplitud en grados del sacudido de cola")]
//    public float shakeAmplitude = 30f;
//    [Tooltip("Frecuencia (oscilaciones por segundo) del sacudido de cola")]
//    public float shakeFrequency = 10f;
//    [Tooltip("Retardo entre fin del sacudido y comienzo del ataque")]
//    public float shakeToAttackDelay = 0.2f;

//    [Header("Attack Settings")]
//    [Tooltip("Número de segmentos finales (excluyendo cabeza) que participarán en el ataque")]
//    public int numAttackSegments = 5;
//    [Tooltip("Distancia extra entre segmentos durante el ataque")]
//    public float attackDistance = 1f;
//    [Tooltip("Duración total del ataque en segundos")]
//    public float attackDuration = 1f;

//    private SnakeController snake;
//    private ColumnWrapOnInput columnWrapper;
//    private List<Transform> segments;
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
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(attackKey) && !isShaking && !isAttacking)
//        {
//            // Inicia sacudido de cola antes del ataque
//            segments = snake.Segmentos;
//            if (segments.Count < 2)
//                return;

//            snake.enabled = false;
//            StartCoroutine(PreAttackShake());
//        }

//        if (isAttacking)
//            ApplyAttack();
//    }

//    private IEnumerator PreAttackShake()
//    {
//        isShaking = true;

//        // Obtiene el segmento de la cola
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

//        // Restaurar rotación original
//        tail.localRotation = originalRot;
//        isShaking = false;

//        // Pausa antes de iniciar el ataque
//        yield return new WaitForSeconds(shakeToAttackDelay);

//        // Tras sacudido y pausa, inicia ataque
//        StartAttack();
//    }

//    void StartAttack()
//    {
//        numAttackSegments = Mathf.Clamp(numAttackSegments, 1, segments.Count - 1);

//        GameObject playerObj = GameObject.FindWithTag("Player");
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
//        float lerpFactor = followLerpSpeed * Time.deltaTime;

//        int startIndex = segments.Count - numAttackSegments;
//        Vector3 lineOrigin = segments[startIndex - 1].position;

//        // Distancia real hasta el jugador desde el punto de origen
//        Transform playerT = GameObject.FindWithTag("Player").transform;
//        float distToPlayer = Vector3.Distance(playerT.position, lineOrigin);
//        // La distancia máxima total que cubrirán todos los segmentos
//        float maxTotalDist = Mathf.Min(distToPlayer, attackDistance * numAttackSegments);
//        // Paso equidistante
//        float stepDist = maxTotalDist / numAttackSegments;

//        // Mueve segmentos previos
//        for (int i = 1; i < startIndex; i++)
//        {
//            Transform curr = segments[i];
//            Transform prev = segments[i - 1];
//            Vector3 dir = (prev.position - curr.position).normalized;
//            Vector3 target = prev.position - dir * snake.separacionSegmentos;
//            curr.position = Vector3.Lerp(curr.position, target, lerpFactor);
//            curr.rotation = Quaternion.Slerp(
//                curr.rotation,
//                Quaternion.LookRotation(dir),
//                lerpFactor
//            );
//        }

//        // Mueve segmentos de ataque y cola equidistantes
//        for (int i = startIndex; i < segments.Count; i++)
//        {
//            Transform curr = segments[i];
//            int relIdx = i - startIndex + 1;
//            Vector3 targetPos = lineOrigin + attackDir * (stepDist * relIdx);

//            curr.position = Vector3.Lerp(curr.position, targetPos, lerpFactor);
//            if ((targetPos - curr.position).sqrMagnitude > 0.0001f)
//            {
//                curr.rotation = Quaternion.Slerp(
//                    curr.rotation,
//                    Quaternion.LookRotation((targetPos - curr.position).normalized),
//                    lerpFactor
//                );
//            }
//        }

//        if (attackTimer >= attackDuration)
//            EndAttack();
//    }

//    void EndAttack()
//    {
//        isAttacking = false;
//        StartCoroutine(TransitionToWrap());
//    }

//    private IEnumerator TransitionToWrap()
//    {
//        int segCount = segments.Count;
//        List<Vector3> startPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//            startPos.Add(segments[i].position);

//        float alturaTotal = snake.distanciaCabezaCuerpo
//                          + snake.separacionSegmentos * (segCount - 2)
//                          + snake.separacionCola;

//        float vueltas = columnWrapper.vueltasCompletas;
//        float offset = columnWrapper.offsetRadio;
//        float wrapDur = columnWrapper.velocidadEnrollado;
//        float radio = columnWrapper.GetColumnRadius();
//        Transform col = columnWrapper.columna;

//        List<Vector3> targetPos = new List<Vector3>(segCount);
//        for (int i = 0; i < segCount; i++)
//        {
//            float t = 1f - ((float)i / (segCount - 1));
//            float ang = t * vueltas * 2 * Mathf.PI;
//            float alt = t * alturaTotal;
//            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
//            Vector3 p = col.position + dir * (radio + offset) + Vector3.up * alt;
//            targetPos.Add(p);
//        }

//        float elapsed = 0f;
//        while (elapsed < wrapDur)
//        {
//            float frac = Mathf.SmoothStep(0f, 1f, elapsed / wrapDur);
//            for (int i = 0; i < segCount; i++)
//            {
//                Transform seg = segments[i];
//                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

//                if (i <= columnWrapper.segmentosCuello)
//                    seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
//                else
//                {
//                    Vector3 lookT = (i < segCount - 1)
//                        ? targetPos[i + 1]
//                        : col.position + Vector3.up * (targetPos[i].y + 0.1f);
//                    seg.LookAt(lookT);
//                }
//            }
//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        for (int i = 0; i < segCount; i++)
//        {
//            Transform seg = segments[i];
//            seg.position = targetPos[i];
//            if (i <= columnWrapper.segmentosCuello)
//                seg.LookAt(new Vector3(col.position.x, seg.position.y, col.position.z));
//            else
//            {
//                Vector3 lookT = (i < segCount - 1)
//                    ? targetPos[i + 1]
//                    : col.position + Vector3.up * (targetPos[i].y + 0.1f);
//                seg.LookAt(lookT);
//            }
//        }

//        snake.enabled = false;
//    }
//}




























