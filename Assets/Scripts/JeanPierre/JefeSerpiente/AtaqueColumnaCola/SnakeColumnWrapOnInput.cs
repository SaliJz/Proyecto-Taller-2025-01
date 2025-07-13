// SnakeColumnWrapOnInput.cs
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public float returnAndWrapDuration = 1f;

    private SnakeController snake;
    private ColumnWrapOnInput columnWrapper;
    private List<Transform> segments;

    private bool isShaking = false;
    private bool isAttacking = false;
    private float attackTimer = 0f;

    private ActivadorEfectos efectosActivator;

    // Campos para almacenar la pose perfecta post-wrap
    private List<Vector3> initialWrapPositions;
    private List<Quaternion> initialWrapRotations;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        columnWrapper = GetComponent<ColumnWrapOnInput>();
        returnAndWrapDuration = columnWrapper.velocidadEnrollado;
        efectosActivator = FindObjectOfType<ActivadorEfectos>();
        // Suscribir captura post-wrap cuando termine el enrollado
        columnWrapper.OnWrapComplete += CapturePostWrapPose;
    }

    void OnDestroy()
    {
        if (columnWrapper != null)
            columnWrapper.OnWrapComplete -= CapturePostWrapPose;
    }

    // Captura posiciones y rotaciones justas al completar el wrap
    private void CapturePostWrapPose()
    {
        if (snake != null && snake.Segmentos != null)
        {
            initialWrapPositions = snake.Segmentos.Select(s => s.position).ToList();
            initialWrapRotations = snake.Segmentos.Select(s => s.rotation).ToList();
        }
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
        // La pose post-wrap ya está capturada
        numAttackSegments = Mathf.Clamp(numAttackSegments, 1, segments.Count - 1);
        attackTimer = 0f;
        isAttacking = true;
    }

    void ApplyAttack()
    {
        attackTimer += Time.deltaTime;
        float t = Mathf.Clamp01(attackTimer / attackDuration);

        int wrapStartIndex = segments.Count - numAttackSegments;
        float lerpFactor = (attackDistance / attackDuration) * Time.deltaTime;
        Transform lineOrigin = segments[wrapStartIndex - 1];

        GameObject playerObj = GameObject.FindWithTag("Player");
        Vector3 currentTarget = playerObj != null
            ? playerObj.transform.position
            : lineOrigin.position + transform.forward * attackDistance;
        Vector3 dirToTarget = (currentTarget - lineOrigin.position).normalized;

        float distToTarget = Vector3.Distance(currentTarget, lineOrigin.position);
        float maxTotal = Mathf.Min(distToTarget, attackDistance * numAttackSegments);
        float step = maxTotal / numAttackSegments;

        // Enrollar segmentos anteriores
        for (int i = 0; i < wrapStartIndex; i++)
        {
            Transform curr = segments[i];
            Transform prev = (i == 0) ? segments[0] : segments[i - 1];
            Vector3 dir = (prev.position - curr.position).normalized;
            Vector3 tgt = prev.position - dir * snake.separacionSegmentos;
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);
            curr.rotation = Quaternion.Slerp(curr.rotation, Quaternion.LookRotation(dir), lerpFactor);
        }

        // Mover segmentos de ataque
        for (int i = wrapStartIndex; i < segments.Count; i++)
        {
            Transform curr = segments[i];
            int rel = i - wrapStartIndex + 1;
            Vector3 tgt = lineOrigin.position + dirToTarget * (step * rel);
            curr.position = Vector3.Lerp(curr.position, tgt, lerpFactor);

            if (i == segments.Count - 1)
                curr.forward = -dirToTarget;
            else if ((tgt - curr.position).sqrMagnitude > 1e-4f)
                curr.rotation = Quaternion.Slerp(
                    curr.rotation,
                    Quaternion.LookRotation((tgt - curr.position).normalized),
                    lerpFactor
                );
        }

        // La cabeza sigue al jugador
        if (segments.Count > 0 && playerObj != null)
        {
            Transform head = segments[0];
            Vector3 lookPos = new Vector3(currentTarget.x, head.position.y, currentTarget.z);
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
        List<Vector3> wrapStart = segments.Select(s => s.position).ToList();

        // Usar la pose perfecta post-wrap capturada
        List<Vector3> wrapTarget = new List<Vector3>(initialWrapPositions);
        List<Quaternion> wrapRotTarget = new List<Quaternion>(initialWrapRotations);

        float elapsed = 0f;
        while (elapsed < returnAndWrapDuration)
        {
            float frac = Mathf.SmoothStep(0f, 1f, elapsed / returnAndWrapDuration);
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = segments[i];
                seg.position = Vector3.Lerp(wrapStart[i], wrapTarget[i], frac);
                seg.rotation = Quaternion.Slerp(seg.rotation, wrapRotTarget[i], frac);
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final exacto
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = segments[i];
            seg.position = wrapTarget[i];
            seg.rotation = wrapRotTarget[i];
        }

        snake.ResetPositionHistory();
        snake.enabled = false;
    }
}


