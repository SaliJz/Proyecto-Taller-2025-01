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


//// SnakeAttackAnticipation.cs
////
//// Animación de anticipación en “U” vertical antes de cada ataque,
//// con movimiento de onda fluido y continuidad entre segmentos.

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//[RequireComponent(typeof(ColumnWrapOnInput))]
//public class SnakeAttackAnticipation : MonoBehaviour
//{
//    [HideInInspector] public SnakeController snake;
//    [HideInInspector] public ColumnWrapOnInput columnWrapper;

//    [Header("Anticipación en U (onda)")]
//    [Tooltip("Número de segmentos iniciales (incluida la cabeza) que quedan estáticos.")]
//    public int staticSegmentsCount = 2;
//    [Tooltip("Número de segmentos posteriores que formarán la U.")]
//    public int movingSegmentsCount = 5;
//    [Tooltip("Duración (s) de la animación de cada segmento.")]
//    public float anticipationDuration = 0.6f;
//    [Tooltip("Radio horizontal base de la U.")]
//    public float uRadius = 1f;
//    [Tooltip("Altura vertical máxima de la U.")]
//    public float uHeight = 1f;
//    [Tooltip("Sobreimpulso máximo para efecto de rebote.")]
//    public float overshoot = 0.2f;
//    [Tooltip("Invierte la dirección de la U: false = hacia atrás, true = hacia adelante.")]
//    public bool invertWaveDirection = false;
//    [Tooltip("Potencia de la curva para suavizar la U (valores >1 hacen la U menos pronunciada al inicio).")]
//    public float curvePower = 2f;

//    /// <summary>
//    /// Llamar antes de arrancar la rutina para asignar referencias.
//    /// </summary>
//    public void Initialize(SnakeController sc, ColumnWrapOnInput cw)
//    {
//        snake = sc;
//        columnWrapper = cw;
//    }

//    /// <summary>
//    /// Ejecuta la animación en U con onda, manteniendo las distancias originales
//    /// y propagando la animación de forma fluida desde el segmento más cercano
//    /// hasta la cola.
//    /// </summary>
//    public IEnumerator AnticipationRoutine()
//    {
//        if (snake == null || columnWrapper == null)
//        {
//            Debug.LogError("AnticipationRoutine: referencias no inicializadas.");
//            yield break;
//        }

//        var segments = snake.Segmentos;
//        int totalSegs = segments.Count;
//        if (totalSegs <= staticSegmentsCount) yield break;

//        int available = totalSegs - staticSegmentsCount;
//        int count = Mathf.Min(movingSegmentsCount, available);
//        int startIndex = staticSegmentsCount;
//        Transform anchor = segments[startIndex - 1];

//        // Guardar posiciones iniciales y distancias originales
//        Vector3[] startPos = new Vector3[count];
//        float[] origDist = new float[count];
//        for (int i = 0; i < count; i++)
//        {
//            startPos[i] = segments[startIndex + i].position;
//            origDist[i] = (i == 0)
//                ? Vector3.Distance(anchor.position, startPos[i])
//                : Vector3.Distance(startPos[i], startPos[i - 1]);
//        }

//        // Calcular la curva en semicírculo vertical (“U”), suavizada por curvePower
//        Vector3 center = anchor.position;
//        Vector3 baseDir = (startPos[0] - center).normalized;
//        Vector3 waveDir = invertWaveDirection ? -baseDir : baseDir;
//        Vector3 up = Vector3.up;

//        Vector3[] rawU = new Vector3[count];
//        for (int i = 0; i < count; i++)
//        {
//            // tNorm va de 0 (cerca de la cabeza) a 1 (cola)
//            float tNorm = (count > 1) ? i / (float)(count - 1) : 0.5f;
//            // aplicamos potencia para reducir la curvatura al inicio
//            float blend = Mathf.Pow(tNorm, curvePower);
//            float angle = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, blend);
//            rawU[i] = center
//                     + waveDir * (Mathf.Cos(angle) * uRadius)
//                     + up * (Mathf.Sin(angle) * uHeight);
//        }

//        // Ajustar rawU para preservar distancias originales
//        Vector3[] targetPos = new Vector3[count];
//        for (int i = 0; i < count; i++)
//        {
//            if (i == 0)
//            {
//                Vector3 dir = (rawU[0] - center).normalized;
//                targetPos[0] = center + dir * origDist[0];
//            }
//            else
//            {
//                Vector3 dir = (rawU[i] - rawU[i - 1]).normalized;
//                targetPos[i] = targetPos[i - 1] + dir * origDist[i];
//            }
//        }

//        columnWrapper.enabled = false;

//        float elapsed = 0f;
//        float delayPerSegment = anticipationDuration / count;
//        float totalTime = anticipationDuration + delayPerSegment * (count - 1);

//        while (elapsed < totalTime)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                float localTime = elapsed - delayPerSegment * i;
//                if (localTime <= 0f) continue;

//                float frac = Mathf.Clamp01(localTime / anticipationDuration);
//                frac = Mathf.SmoothStep(0f, 1f, frac);
//                float pulse = Mathf.Sin(frac * Mathf.PI) * overshoot;

//                Transform seg = segments[startIndex + i];
//                Vector3 basePosition = Vector3.Lerp(startPos[i], targetPos[i], frac);
//                Vector3 moveDir = (targetPos[i] - startPos[i]).normalized;
//                seg.position = basePosition + moveDir * pulse;

//                // Continuidad de rotación: cada segmento mantiene la orientación
//                // del anterior hasta que la curvatura actúa gradualmente.
//                Vector3 lookTarget = (i == 0)
//                    ? anchor.position
//                    : segments[startIndex + i - 1].position;
//                seg.LookAt(lookTarget);
//            }

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        // Ajuste final de posición y orientación
//        for (int i = 0; i < count; i++)
//        {
//            Transform seg = segments[startIndex + i];
//            seg.position = targetPos[i];
//            Vector3 lookTarget = (i == 0)
//                ? anchor.position
//                : segments[startIndex + i - 1].position;
//            seg.LookAt(lookTarget);
//        }

//        columnWrapper.enabled = true;
//    }
//}




































////// Los mas bueno
//// SnakeAttackAnticipation.cs
////
//// Animación de anticipación en “U” vertical antes de cada ataque,
//// con movimiento de onda fluido y continuidad entre segmentos.

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//[RequireComponent(typeof(ColumnWrapOnInput))]
//public class SnakeAttackAnticipation : MonoBehaviour
//{
//    [HideInInspector] public SnakeController snake;
//    [HideInInspector] public ColumnWrapOnInput columnWrapper;

//    [Header("Anticipación en U (onda)")]
//    [Tooltip("Número de segmentos iniciales (incluida la cabeza) que quedan estáticos.")]
//    public int staticSegmentsCount = 2;
//    [Tooltip("Número de segmentos posteriores que formarán la U.")]
//    public int movingSegmentsCount = 5;
//    [Tooltip("Duración (s) de la animación de cada segmento.")]
//    public float anticipationDuration = 0.6f;
//    [Tooltip("Radio horizontal base de la U.")]
//    public float uRadius = 1f;
//    [Tooltip("Altura vertical máxima de la U.")]
//    public float uHeight = 1f;
//    [Tooltip("Sobreimpulso máximo para efecto de rebote.")]
//    public float overshoot = 0.2f;
//    [Tooltip("Invierte la dirección de la U: false = hacia atrás, true = hacia adelante.")]
//    public bool invertWaveDirection = false;

//    /// <summary>
//    /// Llamar antes de arrancar la rutina para asignar referencias.
//    /// </summary>
//    public void Initialize(SnakeController sc, ColumnWrapOnInput cw)
//    {
//        snake = sc;
//        columnWrapper = cw;
//    }

//    /// <summary>
//    /// Ejecuta la animación en U con onda, manteniendo las distancias originales
//    /// y propagando la animación de forma fluida desde el segmento más cercano
//    /// hasta la cola.
//    /// </summary>
//    public IEnumerator AnticipationRoutine()
//    {
//        if (snake == null || columnWrapper == null)
//        {
//            Debug.LogError("AnticipationRoutine: referencias no inicializadas.");
//            yield break;
//        }

//        var segments = snake.Segmentos;
//        int totalSegs = segments.Count;
//        if (totalSegs <= staticSegmentsCount) yield break;

//        int available = totalSegs - staticSegmentsCount;
//        int count = Mathf.Min(movingSegmentsCount, available);
//        int startIndex = staticSegmentsCount;
//        Transform anchor = segments[startIndex - 1];

//        // Guardar posiciones iniciales y distancias originales
//        Vector3[] startPos = new Vector3[count];
//        float[] origDist = new float[count];
//        for (int i = 0; i < count; i++)
//        {
//            startPos[i] = segments[startIndex + i].position;
//            origDist[i] = (i == 0)
//                ? Vector3.Distance(anchor.position, startPos[i])
//                : Vector3.Distance(startPos[i], startPos[i - 1]);
//        }

//        // Calcular la curva en semicírculo vertical (“U”)
//        Vector3 center = anchor.position;
//        Vector3 baseDir = (startPos[0] - center).normalized;
//        Vector3 waveDir = invertWaveDirection ? -baseDir : baseDir;
//        Vector3 up = Vector3.up;
//        Vector3[] rawU = new Vector3[count];
//        for (int i = 0; i < count; i++)
//        {
//            float t = (count > 1) ? i / (float)(count - 1) : 0.5f;
//            float angle = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, t);
//            rawU[i] = center
//                    + waveDir * (Mathf.Cos(angle) * uRadius)
//                    + up * (Mathf.Sin(angle) * uHeight);
//        }

//        // Ajustar rawU para preservar distancias originales
//        Vector3[] targetPos = new Vector3[count];
//        for (int i = 0; i < count; i++)
//        {
//            if (i == 0)
//            {
//                Vector3 dir = (rawU[0] - center).normalized;
//                targetPos[0] = center + dir * origDist[0];
//            }
//            else
//            {
//                Vector3 dir = (rawU[i] - rawU[i - 1]).normalized;
//                targetPos[i] = targetPos[i - 1] + dir * origDist[i];
//            }
//        }

//        columnWrapper.enabled = false;

//        float elapsed = 0f;
//        // Definimos un retardo entre segmentos para la propagación de la onda
//        float delayPerSegment = anticipationDuration / count;
//        // Tiempo total de animación hasta que el último segmento finaliza
//        float totalTime = anticipationDuration + delayPerSegment * (count - 1);

//        while (elapsed < totalTime)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                // Calculamos el tiempo local para este segmento
//                float localTime = elapsed - delayPerSegment * i;
//                if (localTime <= 0f) continue;

//                // Normalizamos y suavizamos la progresión
//                float frac = Mathf.Clamp01(localTime / anticipationDuration);
//                frac = Mathf.SmoothStep(0f, 1f, frac);
//                float pulse = Mathf.Sin(frac * Mathf.PI) * overshoot;

//                Transform seg = segments[startIndex + i];
//                Vector3 basePosition = Vector3.Lerp(startPos[i], targetPos[i], frac);
//                Vector3 moveDir = (targetPos[i] - startPos[i]).normalized;
//                seg.position = basePosition + moveDir * pulse;

//                // Cada segmento mira al anterior para continuidad
//                Vector3 lookTarget = (i == 0)
//                    ? anchor.position
//                    : segments[startIndex + i - 1].position;
//                seg.LookAt(lookTarget);
//            }

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        // Ajuste final de posición y orientación
//        for (int i = 0; i < count; i++)
//        {
//            Transform seg = segments[startIndex + i];
//            seg.position = targetPos[i];
//            Vector3 lookTarget = (i == 0)
//                ? anchor.position
//                : segments[startIndex + i - 1].position;
//            seg.LookAt(lookTarget);
//        }

//        columnWrapper.enabled = true;
//    }
//}


























//// SnakeAttackAnticipation.cs
////
//// Animación de anticipación en “U” vertical antes de cada ataque,
//// con movimiento de “onda” fluido y continuidad entre segmentos.

//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeColumnWrapOnInput))]
//[RequireComponent(typeof(ColumnWrapOnInput))]
//public class SnakeAttackAnticipation : MonoBehaviour
//{
//    [HideInInspector] public SnakeController snake;
//    [HideInInspector] public ColumnWrapOnInput columnWrapper;

//    [Header("Anticipación en U (onda)")]
//    [Tooltip("Número de segmentos iniciales (incluida la cabeza) que quedan estáticos.")]
//    public int staticSegmentsCount = 2;
//    [Tooltip("Número de segmentos posteriores que formarán la U.")]
//    public int movingSegmentsCount = 5;
//    [Tooltip("Duración total (s) de la animación de anticipación.")]
//    public float anticipationDuration = 0.6f;
//    [Tooltip("Radio horizontal base de la U.")]
//    public float uRadius = 1f;
//    [Tooltip("Altura vertical máxima de la U.")]
//    public float uHeight = 1f;
//    [Tooltip("Magnitud extra de sobreimpulso para fluidez.")]
//    public float overshoot = 0.2f;

//    /// <summary>
//    /// Llamar antes de arrancar la rutina para asignar referencias.
//    /// </summary>
//    public void Initialize(SnakeController sc, ColumnWrapOnInput cw)
//    {
//        snake = sc;
//        columnWrapper = cw;
//    }

//    /// <summary>
//    /// Realiza la animación de onda en U,
//    /// conectada al último segmento estático,
//    /// con suave sobreimpulso y retrasos por segmento.
//    /// </summary>
//    public IEnumerator AnticipationRoutine()
//    {
//        if (snake == null || columnWrapper == null)
//        {
//            Debug.LogError("AnticipationRoutine: referencias no inicializadas.");
//            yield break;
//        }

//        List<Transform> segments = snake.Segmentos;
//        int total = segments.Count;
//        if (total <= staticSegmentsCount) yield break;

//        int available = total - staticSegmentsCount;
//        int count = Mathf.Min(movingSegmentsCount, available);
//        int startIndex = staticSegmentsCount;
//        Transform anchor = segments[staticSegmentsCount - 1];

//        // Posiciones iniciales
//        Vector3[] startPos = new Vector3[count];
//        for (int i = 0; i < count; i++)
//            startPos[i] = segments[startIndex + i].position;

//        // Precalculamos posiciones base de U hacia atrás
//        Vector3 center = anchor.position;
//        Vector3 back = -columnWrapper.columna.forward;
//        Vector3 up = Vector3.up;
//        Vector3[] targetPos = new Vector3[count];
//        for (int i = 0; i < count; i++)
//        {
//            float t = (count > 1) ? i / (float)(count - 1) : 0.5f;
//            float angle = Mathf.Lerp(-Mathf.PI / 2f, Mathf.PI / 2f, t);
//            Vector3 offset = back * (Mathf.Cos(angle) * uRadius)
//                           + up * (Mathf.Sin(angle) * uHeight);
//            targetPos[i] = center + offset;
//        }

//        // Desactivamos wrap para que no interfiera
//        columnWrapper.enabled = false;

//        float elapsed = 0f;
//        float delayPer = anticipationDuration / (count + 1);

//        // Animación de onda con sobreimpulso
//        while (elapsed < anticipationDuration)
//        {
//            for (int i = 0; i < count; i++)
//            {
//                float localT = Mathf.Clamp01((elapsed - i * delayPer) / anticipationDuration);
//                float eased = Mathf.SmoothStep(0f, 1f, localT);

//                // position + overshoot en mitad de la curva
//                Vector3 basePos = Vector3.Lerp(startPos[i], targetPos[i], eased);
//                float pulse = Mathf.Sin(eased * Mathf.PI) * overshoot;
//                Vector3 extra = back * pulse;
//                Transform seg = segments[startIndex + i];
//                seg.position = basePos + extra;

//                // Continuidad: cada móvil mira al anterior (estático o móvil)
//                Vector3 lookTarget = (i == 0)
//                    ? anchor.position
//                    : segments[startIndex + i - 1].position;
//                seg.LookAt(lookTarget);
//            }

//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        // Ajuste final exacto
//        for (int i = 0; i < count; i++)
//        {
//            Transform seg = segments[startIndex + i];
//            seg.position = targetPos[i];
//            Vector3 lookTarget = (i == 0)
//                ? anchor.position
//                : segments[startIndex + i - 1].position;
//            seg.LookAt(lookTarget);
//        }

//        columnWrapper.enabled = true;
//    }
//}
