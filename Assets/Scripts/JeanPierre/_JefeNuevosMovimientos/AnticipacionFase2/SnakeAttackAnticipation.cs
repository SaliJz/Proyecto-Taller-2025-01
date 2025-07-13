
// SnakeAttackAnticipation.cs
//
// Animación de anticipación en “U” vertical antes de cada ataque,
// con movimiento de onda fluido y continuidad entre segmentos,
// y separación constante para que no haya huecos.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeColumnWrapOnInput))]
[RequireComponent(typeof(ColumnWrapOnInput))]
public class SnakeAttackAnticipation : MonoBehaviour
{
    [HideInInspector] public SnakeController snake;
    [HideInInspector] public ColumnWrapOnInput columnWrapper;

    [Header("Anticipación en U (onda)")]
    public int staticSegmentsCount = 2;
    public int movingSegmentsCount = 5;
    public float anticipationDuration = 0.6f;
    [Tooltip("Radio horizontal base de la U.")]
    public float uRadius = 1f;
    [Tooltip("Altura vertical máxima de la U.")]
    public float uHeight = 0.1f;
    public float overshoot = 0.2f;
    public bool invertWaveDirection = false;
    public float curvePower = 2f;
    public float backAllowance = 0.5f;
    public float frontAllowance = 1f;
    public float centerOffset = 0f;
    public float sideOffset = 0f;

    public void Initialize(SnakeController sc, ColumnWrapOnInput cw)
    {
        snake = sc;
        columnWrapper = cw;
    }

    public IEnumerator AnticipationRoutine()
    {
        if (snake == null || columnWrapper == null) yield break;

        var segments = snake.Segmentos;
        int totalSegs = segments.Count;
        if (totalSegs <= staticSegmentsCount) yield break;

        int count = Mathf.Min(movingSegmentsCount, totalSegs - staticSegmentsCount);
        int startIdx = staticSegmentsCount;
        Transform anchor = segments[startIdx - 1];

        // Guardar posiciones iniciales
        Vector3[] startPos = new Vector3[count];
        for (int i = 0; i < count; i++)
            startPos[i] = segments[startIdx + i].position;

        // Distancia constante entre segmentos (asegura continuidad)
        float segSpacing = snake.separacionSegmentos;

        // Dirección de la onda y ejes
        Vector3 center = anchor.position;
        Vector3 baseDir = (startPos[0] - center).normalized;
        Vector3 waveDir = invertWaveDirection ? -baseDir : baseDir;
        Vector3 up = Vector3.up;
        Vector3 sideDir = Vector3.Cross(up, waveDir).normalized;

        // Desplaza el centro según configuración
        center += waveDir * centerOffset + sideDir * sideOffset;

        // Calcular rawU
        Vector3[] rawU = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            float t = count > 1 ? i / (float)(count - 1) : 0.5f;
            float blend = Mathf.Pow(t, curvePower);
            float angle = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, blend);

            float cosVal = Mathf.Cos(angle);
            float allowance = cosVal >= 0f ? frontAllowance : backAllowance;
            float effCos = cosVal * allowance;

            Vector3 horiz = waveDir * (effCos * uRadius);
            float height = Mathf.Sin(angle) * uHeight;
            rawU[i] = center + horiz + up * height;
        }

        // Construir posiciones finales con separación constante
        Vector3[] targetPos = new Vector3[count];
        for (int i = 0; i < count; i++)
        {
            if (i == 0)
            {
                Vector3 dir = (rawU[0] - center).normalized;
                targetPos[0] = center + dir * segSpacing;
            }
            else
            {
                Vector3 dir = (rawU[i] - rawU[i - 1]).normalized;
                targetPos[i] = targetPos[i - 1] + dir * segSpacing;
            }
        }

        columnWrapper.enabled = false;

        float elapsed = 0f;
        float delay = anticipationDuration / count;
        float total = anticipationDuration + delay * (count - 1);

        while (elapsed < total)
        {
            for (int i = 0; i < count; i++)
            {
                float lt = elapsed - delay * i;
                if (lt <= 0f) continue;

                float frac = Mathf.Clamp01(lt / anticipationDuration);
                frac = Mathf.SmoothStep(0f, 1f, frac);
                float pulse = Mathf.Sin(frac * Mathf.PI) * overshoot;

                Transform seg = segments[startIdx + i];
                Vector3 basePos = Vector3.Lerp(startPos[i], targetPos[i], frac);
                Vector3 moveDir = (targetPos[i] - startPos[i]).normalized;
                seg.position = basePos + moveDir * pulse;

                Vector3 lookT = i == 0
                    ? anchor.position
                    : segments[startIdx + i - 1].position;
                seg.LookAt(lookT);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final
        for (int i = 0; i < count; i++)
        {
            Transform seg = segments[startIdx + i];
            seg.position = targetPos[i];
            Vector3 lookT = i == 0
                ? anchor.position
                : segments[startIdx + i - 1].position;
            seg.LookAt(lookT);
        }

        columnWrapper.enabled = true;
    }
}

