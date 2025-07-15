// AnimacionMuerteCobraEpic.cs
//
// Maneja la secuencia épica de muerte de la cobra y, al terminar,
// carga la escena de Créditos.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(SnakeController))]
public class AnimacionMuerteCobraEpic : MonoBehaviour
{
    #region Pose Settings
    [Header("Configuración de Pose")]
    [Tooltip("Altura máxima de elevación de la cabeza")]
    public float alturaMax = 2f;
    [Tooltip("Grado de curvatura en los segmentos del cuello")]
    [Range(0, 180)]
    public float anguloCurva = 90f;
    [Tooltip("Número de segmentos que participan en la curva del cuello (mín. 2)")]
    [Min(2)]
    public int segmentosCuello = 8;
    [Tooltip("Velocidad base de la pose")]
    public float poseSpeed = 3f;
    [Tooltip("Curva de interpolación para la pose (0=start, 1=end)")]
    public AnimationCurve poseEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    #endregion

    #region Collapse Settings
    [Header("Configuración de Colapso")]
    [Tooltip("Altura extra desde la que se desploman los segmentos")]
    public float alturaCaida = 1.5f;
    [Tooltip("Curva de caída para simular agotamiento")]
    public AnimationCurve caidaEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Inclinación máxima hacia adelante al colapsar")]
    [Range(0, 90)]
    public float tiltForward = 45f;
    #endregion

    #region Dizzy Settings
    [Header("Configuración de Mareo")]
    [Tooltip("Duración total del mareo antes de la muerte")]
    public float duracionMareo = 1.2f;
    [Tooltip("Grado máximo de inclinación lateral en grados")]
    public float intensidadMareo = 15f;
    [Tooltip("Frecuencia de oscilación lateral por segundo")]
    public float frecuenciaMareo = 3f;
    [Tooltip("Eje local alrededor del cual inclinarse para el mareo")]
    public Vector3 ejeMareo = new Vector3(0, 0, 1);
    #endregion

    #region Activation
    [Header("Activación")]
    [Tooltip("Marca para iniciar la secuencia épica")]
    public bool activarEpic = false;
    #endregion

    private SnakeController snake;
    private List<Transform> segmentos;
    private Transform player;
    private bool running = false;

    void Awake()
    {
        snake = GetComponent<SnakeController>();
        segmentos = snake?.Segmentos;
        var p = GameObject.FindWithTag("Player");
        if (p) player = p.transform;
        enabled = false; // Se habilita desde Fase2Vida
    }

    void Update()
    {
        if (activarEpic && !running && segmentos != null && player != null)
        {
            StartCoroutine(RunEpicSequence());
        }
    }

    private IEnumerator RunEpicSequence()
    {
        running = true;
        activarEpic = false;
        snake.enabled = false;

        // 1. Pose épica hacia el jugador
        yield return PosePhase();

        // 2. Pequeña pausa
        yield return new WaitForSeconds(0.1f);

        // 3. Mareo lateral antes de colapsar
        yield return MareoPhase();

        // 4. Pequeña pausa antes de la muerte
        yield return new WaitForSeconds(0.1f);

        // 5. Colapso final (animación de muerte)
        yield return CollapsePhase();

        // 6. Al terminar, carga la escena de Créditos
        SceneManager.LoadScene("Creditos");
    }

    private IEnumerator PosePhase()
    {
        float duration = alturaMax / poseSpeed * 0.6f + 0.5f;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = poseEase.Evaluate(Mathf.Clamp01(t));
            AplicarPoseHaciaPlayer(ease);
            yield return null;
        }
    }

    private IEnumerator MareoPhase()
    {
        float elapsed = 0f;
        Quaternion origHeadRot = segmentos[0].rotation;
        Vector3 axis = ejeMareo.normalized;
        while (elapsed < duracionMareo)
        {
            elapsed += Time.deltaTime;
            float angle = Mathf.Sin(elapsed * frecuenciaMareo * Mathf.PI * 2f) * intensidadMareo;
            segmentos[0].rotation = origHeadRot * Quaternion.AngleAxis(angle, axis);
            yield return null;
        }
        segmentos[0].rotation = origHeadRot;
    }

    private IEnumerator CollapsePhase()
    {
        float duration = alturaCaida / poseSpeed * 0.8f + 0.7f;
        float t = 0f;
        Vector3[] origPos = new Vector3[segmentos.Count];
        Quaternion[] origRot = new Quaternion[segmentos.Count];
        for (int i = 0; i < segmentos.Count; i++)
        {
            origPos[i] = segmentos[i].position;
            origRot[i] = segmentos[i].rotation;
        }
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float ease = caidaEase.Evaluate(Mathf.Clamp01(t));
            for (int i = 0; i < segmentos.Count; i++)
                ApplyCollapseToSegment(i, origPos, origRot, ease);
            yield return null;
        }
    }

    private void AplicarPoseHaciaPlayer(float ease)
    {
        Vector3 headPos = segmentos[0].position;
        Vector3 planeHead = new Vector3(headPos.x, 0, headPos.z);
        Vector3 planePlayer = new Vector3(player.position.x, 0, player.position.z);
        Vector3 dir = (planePlayer - planeHead).normalized;
        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
        Vector3 targetHead = planeHead + Vector3.up * (alturaMax * ease);

        segmentos[0].rotation = Quaternion.Slerp(
            segmentos[0].rotation,
            Quaternion.LookRotation(dir),
            ease
        );

        FollowChain(targetHead, dir, ease);
    }

    private void ApplyCollapseToSegment(int i, Vector3[] origPos, Quaternion[] origRot, float ease)
    {
        Vector3 target = new Vector3(
            origPos[i].x,
            Mathf.Lerp(origPos[i].y + alturaCaida, 0, ease),
            origPos[i].z
        );
        segmentos[i].position = Vector3.Lerp(origPos[i], target, ease);

        if (i < segmentosCuello)
        {
            Vector3 forward = player.position - origPos[0];
            forward.y = 0;
            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
            Quaternion lean = Quaternion.LookRotation(forward) * Quaternion.Euler(tiltForward * ease, 0, 0);
            segmentos[i].rotation = Quaternion.Slerp(origRot[i], lean, ease);
        }
        else
        {
            segmentos[i].rotation = origRot[i];
        }
    }

    private void FollowChain(Vector3 headTarget, Vector3 dirXZ, float ease)
    {
        float sep = snake.separacionSegmentos;
        float half = Mathf.PI * 0.5f;
        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;

        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headTarget, ease);
        Vector3 prev = segmentos[0].position;

        for (int i = 1; i < segmentos.Count; i++)
        {
            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
            Vector3 bent = (i < segmentosCuello)
                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
                : dirXZ;
            float yOff = (i < segmentosCuello)
                ? Mathf.Sin((1f - t) * half) * alturaMax * ease
                : 0f;

            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
            Vector3 tgt = prev - bentXZ * sep;
            tgt.y = yOff;

            segmentos[i].position = Vector3.Lerp(segmentos[i].position, tgt, ease);

            Vector3 look = prev - segmentos[i].position;
            look.y = 0;
            if (look.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(look),
                    ease
                );

            prev = segmentos[i].position;
        }
    }
}







//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class AnimacionMuerteCobraEpic : MonoBehaviour
//{
//    #region Pose Settings
//    [Header("Configuración de Pose")]
//    [Tooltip("Altura máxima de elevación de la cabeza")]
//    public float alturaMax = 2f;

//    [Tooltip("Grado de curvatura en los segmentos del cuello")]
//    [Range(0, 180)]
//    public float anguloCurva = 90f;

//    [Tooltip("Número de segmentos que participan en la curva del cuello (mín. 2)")]
//    [Min(2)]
//    public int segmentosCuello = 8;

//    [Tooltip("Velocidad base de la pose")]
//    public float poseSpeed = 3f;

//    [Tooltip("Curva de interpolación para la pose (0=start, 1=end)")]
//    public AnimationCurve poseEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
//    #endregion

//    #region Collapse Settings
//    [Header("Configuración de Colapso")]
//    [Tooltip("Altura extra desde la que se desploman los segmentos")]
//    public float alturaCaida = 1.5f;

//    [Tooltip("Curva de caída para simular agotamiento")]
//    public AnimationCurve caidaEase = AnimationCurve.EaseInOut(0, 0, 1, 1);

//    [Tooltip("Inclinación máxima hacia adelante al colapsar")]
//    [Range(0, 90)]
//    public float tiltForward = 45f;
//    #endregion

//    #region Dizzy Settings
//    [Header("Configuración de Mareo")]
//    [Tooltip("Duración total del mareo antes de la muerte")]
//    public float duracionMareo = 1.2f;

//    [Tooltip("Grado máximo de inclinación lateral en grados")]
//    public float intensidadMareo = 15f;

//    [Tooltip("Frecuencia de oscilación lateral por segundo")]
//    public float frecuenciaMareo = 3f;

//    [Tooltip("Eje local alrededor del cual inclinarse para el mareo")]
//    public Vector3 ejeMareo = new Vector3(0, 0, 1);
//    #endregion

//    #region Activation
//    [Header("Activación")]
//    [Tooltip("Marca para iniciar la secuencia épica")]
//    public bool activarEpic = false;
//    #endregion

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Transform player;
//    private bool running = false;

//    void Awake()
//    {
//        snake = GetComponent<SnakeController>();
//        segmentos = snake?.Segmentos;
//        var p = GameObject.FindWithTag("Player");
//        if (p) player = p.transform;
//    }

//    void Update()
//    {
//        if (activarEpic && !running && segmentos != null && player != null)
//        {
//            StartCoroutine(RunEpicSequence());
//        }
//    }

//    private IEnumerator RunEpicSequence()
//    {
//        running = true;
//        activarEpic = false;
//        snake.enabled = false;

//        // 1. Pose épica hacia el jugador
//        yield return PosePhase();

//        // 2. Pequeña pausa
//        yield return new WaitForSeconds(0.1f);

//        // 3. Mareo lateral antes de colapsar
//        yield return MareoPhase();

//        // 4. Pequeña pausa antes de la muerte
//        yield return new WaitForSeconds(0.1f);

//        // 5. Colapso final (animación de muerte)
//        yield return CollapsePhase();

//        running = false;
//    }

//    private IEnumerator PosePhase()
//    {
//        float duration = alturaMax / poseSpeed * 0.6f + 0.5f;
//        float t = 0f;
//        while (t < 1f)
//        {
//            t += Time.deltaTime / duration;
//            float ease = poseEase.Evaluate(Mathf.Clamp01(t));
//            AplicarPoseHaciaPlayer(ease);
//            yield return null;
//        }
//    }

//    private IEnumerator MareoPhase()
//    {
//        float elapsed = 0f;
//        Quaternion origHeadRot = segmentos[0].rotation;
//        Vector3 axis = ejeMareo.normalized;

//        while (elapsed < duracionMareo)
//        {
//            elapsed += Time.deltaTime;
//            // Oscilación lateral: inclinación alrededor del ejeMareo
//            float angle = Mathf.Sin(elapsed * frecuenciaMareo * Mathf.PI * 2f) * intensidadMareo;
//            Quaternion roll = Quaternion.AngleAxis(angle, axis);
//            segmentos[0].rotation = origHeadRot * roll;
//            yield return null;
//        }

//        // Restaurar rotación original antes del colapso
//        segmentos[0].rotation = origHeadRot;
//    }

//    private IEnumerator CollapsePhase()
//    {
//        float duration = alturaCaida / poseSpeed * 0.8f + 0.7f;
//        float t = 0f;
//        Vector3[] origPos = new Vector3[segmentos.Count];
//        Quaternion[] origRot = new Quaternion[segmentos.Count];
//        for (int i = 0; i < segmentos.Count; i++)
//        {
//            origPos[i] = segmentos[i].position;
//            origRot[i] = segmentos[i].rotation;
//        }

//        while (t < 1f)
//        {
//            t += Time.deltaTime / duration;
//            float ease = caidaEase.Evaluate(Mathf.Clamp01(t));
//            for (int i = 0; i < segmentos.Count; i++)
//                ApplyCollapseToSegment(i, origPos, origRot, ease);
//            yield return null;
//        }
//    }

//    private void AplicarPoseHaciaPlayer(float ease)
//    {
//        Vector3 headPos = segmentos[0].position;
//        Vector3 planeHead = new Vector3(headPos.x, 0, headPos.z);
//        Vector3 planePlayer = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (planePlayer - planeHead).normalized;
//        if (dir.sqrMagnitude < 0.001f) dir = transform.forward;
//        Vector3 targetHead = planeHead + Vector3.up * (alturaMax * ease);

//        segmentos[0].rotation = Quaternion.Slerp(
//            segmentos[0].rotation,
//            Quaternion.LookRotation(dir),
//            ease
//        );

//        FollowChain(targetHead, dir, ease);
//    }

//    private void ApplyCollapseToSegment(int i, Vector3[] origPos, Quaternion[] origRot, float ease)
//    {
//        Vector3 target = new Vector3(
//            origPos[i].x,
//            Mathf.Lerp(origPos[i].y + alturaCaida, 0, ease),
//            origPos[i].z
//        );
//        segmentos[i].position = Vector3.Lerp(origPos[i], target, ease);

//        if (i < segmentosCuello)
//        {
//            Vector3 forward = player.position - origPos[0];
//            forward.y = 0;
//            if (forward.sqrMagnitude < 0.01f) forward = transform.forward;
//            Quaternion lean = Quaternion.LookRotation(forward) * Quaternion.Euler(tiltForward * ease, 0, 0);
//            segmentos[i].rotation = Quaternion.Slerp(origRot[i], lean, ease);
//        }
//        else
//        {
//            segmentos[i].rotation = origRot[i];
//        }
//    }

//    private void FollowChain(Vector3 headTarget, Vector3 dirXZ, float ease)
//    {
//        float sep = snake.separacionSegmentos;
//        float half = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;

//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headTarget, ease);
//        Vector3 prev = segmentos[0].position;

//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * half) * alturaMax * ease
//                : 0f;

//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 tgt = prev - bentXZ * sep;
//            tgt.y = yOff;

//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, tgt, ease);

//            Vector3 look = prev - segmentos[i].position;
//            look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    ease
//                );

//            prev = segmentos[i].position;
//        }
//    }
//}
























