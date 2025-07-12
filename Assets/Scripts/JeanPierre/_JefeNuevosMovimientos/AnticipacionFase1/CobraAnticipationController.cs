// CobraAnticipationController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CobraPoseController))]
public class CobraAnticipationController : MonoBehaviour
{
    [Header("Anticipación antes de ataque")]
    [Tooltip("Duración total de la fase de anticipación")]
    public float duracionAnticipacion = 0.7f;

    [Tooltip("Desplazamiento máximo hacia atrás de cada segmento")]
    public float headOffset = 0.5f;

    [Tooltip("Factor de offset para los segmentos del cuello (multiplica headOffset)")]
    [Range(0f, 1f)]
    public float neckOffsetFactor = 0.7f;

    [Tooltip("Multiplicador global del offset en anticipación")]
    public float offsetMultiplier = 1f;

    [Tooltip("Curva que define el easing principal (0–1)")]
    public AnimationCurve mainCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Tooltip("Curva secundaria de rebote al final")]
    public AnimationCurve bounceCurve = new AnimationCurve(
        new Keyframe(0.0f, 0.0f),
        new Keyframe(0.8f, 1.0f),
        new Keyframe(1.0f, 0.0f)
    );

    [Tooltip("Intensidad de “noise” lateral para dar vida")]
    public float noiseAmplitude = 0.05f;
    [Tooltip("Velocidad del noise")]
    public float noiseSpeed = 3f;

    [Tooltip("Inclinación máxima (grados) al final")]
    public float maxTilt = 15f;

    private CobraPoseController poseCtrl;
    private float noiseSeed;

    void Awake()
    {
        poseCtrl = GetComponent<CobraPoseController>();
        noiseSeed = Random.value * 100f;
    }

    public IEnumerator RunAnticipation()
    {
        poseCtrl.AplicarPose();
        int lastIdx = Mathf.Clamp(poseCtrl.segmentosCuello, 1, poseCtrl.Segmentos.Count - 1);
        int total = lastIdx + 1;

        // Captura posiciones originales
        List<Vector3> originals = new List<Vector3>(total);
        for (int i = 0; i <= lastIdx; i++)
            originals.Add(poseCtrl.Segmentos[i].position);

        // Dirección hacia atrás
        Vector3 backDir = -(poseCtrl.Player.position - originals[0]).normalized;
        backDir.y = 0;

        float elapsed = 0f;
        while (elapsed < duracionAnticipacion)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracionAnticipacion);

            // Evaluación principal y rebote
            float baseT = mainCurve.Evaluate(t);
            float bounceT = bounceCurve.Evaluate(t);

            for (int i = 0; i <= lastIdx; i++)
            {
                // Desfase progresivo
                float segDelay = (lastIdx - i) / (float)total * 0.4f;
                float segT = Mathf.Clamp01((t - segDelay) / (1f - segDelay));
                float eval = mainCurve.Evaluate(segT);

                // Offset + rebote
                float factor = (i == 0) ? 1f : neckOffsetFactor;
                float totalOff = headOffset * factor * offsetMultiplier;
                float move = eval * totalOff + bounceT * (totalOff * 0.2f);

                // Agregar noise lateral
                float lateralNoise = (Mathf.PerlinNoise(noiseSeed + i * 0.3f, Time.time * noiseSpeed) - 0.5f)
                                     * noiseAmplitude;
                Vector3 sideDir = Vector3.Cross(backDir, Vector3.up);

                // Aplicar posición
                Vector3 target = originals[i] + backDir * move + sideDir * lateralNoise;
                poseCtrl.Segmentos[i].position = target;

                // Rotación de inclinación suave según posición del segmento
                float tiltFactor = (i / (float)lastIdx);
                Quaternion targetRot = Quaternion.Euler(
                    Mathf.Lerp(0, maxTilt, eval) * (1f - tiltFactor),
                    poseCtrl.Segmentos[i].rotation.eulerAngles.y,
                    0
                );
                poseCtrl.Segmentos[i].rotation = Quaternion.Slerp(
                    poseCtrl.Segmentos[i].rotation,
                    targetRot,
                    Time.deltaTime * 8f
                );
            }

            yield return null;
        }

        poseCtrl.IniciarAtaque();
    }
}



//// CobraAnticipationController.cs
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(CobraPoseController))]
//public class CobraAnticipationController : MonoBehaviour
//{
//    [Header("Anticipación antes de ataque")]
//    [Tooltip("Duración total de la fase de anticipación")]
//    public float duracionAnticipacion = 0.7f;

//    [Tooltip("Desplazamiento máximo hacia atrás de cada segmento")]
//    public float headOffset = 0.5f;

//    [Tooltip("Factor de offset para los segmentos del cuello (multiplica headOffset)")]
//    [Range(0f, 1f)]
//    public float neckOffsetFactor = 0.7f;

//    [Tooltip("Multiplicador global del offset en anticipación")]
//    public float offsetMultiplier = 1f;

//    private CobraPoseController poseCtrl;

//    void Awake()
//    {
//        poseCtrl = GetComponent<CobraPoseController>();
//    }

//    /// <summary>
//    /// Ejecuta la anticipación en forma de onda:
//    /// primero la base del cuello, luego sucesivamente hasta la cabeza.
//    /// Solo traslada posiciones, NO rota ningún segmento.
//    /// </summary>
//    public IEnumerator RunAnticipation()
//    {
//        // Asegurar que la pose ya esté aplicada
//        poseCtrl.AplicarPose();

//        int lastIdx = Mathf.Clamp(poseCtrl.segmentosCuello, 1, poseCtrl.Segmentos.Count - 1);
//        int totalSegs = lastIdx + 1;  // incluye cabeza

//        // Capturar posiciones originales tras la pose
//        List<Vector3> originalPositions = new List<Vector3>(totalSegs);
//        for (int i = 0; i <= lastIdx; i++)
//            originalPositions.Add(poseCtrl.Segmentos[i].position);

//        // Dirección hacia atrás respecto al punto de ataque
//        Vector3 backDir = -(poseCtrl.Player.position - originalPositions[0]).normalized;
//        backDir.y = 0;

//        float elapsed = 0f;
//        while (elapsed < duracionAnticipacion)
//        {
//            elapsed += Time.deltaTime;
//            float tNorm = Mathf.Clamp01(elapsed / duracionAnticipacion);

//            for (int i = 0; i <= lastIdx; i++)
//            {
//                float segDelayNorm = (lastIdx - i) / (float)totalSegs;
//                float segDuration = 1f / totalSegs;
//                float localT = Mathf.Clamp01((tNorm - segDelayNorm) / segDuration);
//                float easeT = Mathf.SmoothStep(0f, 1f, localT);

//                float factor = (i == 0) ? 1f : neckOffsetFactor;
//                float totalOffset = headOffset * factor * offsetMultiplier;

//                // Solo translación, sin rotación
//                Vector3 targetPos = originalPositions[i] + backDir * totalOffset * easeT;
//                poseCtrl.Segmentos[i].position = targetPos;
//            }

//            yield return null;
//        }

//        // Tras la anticipación, lanzar el ataque
//        poseCtrl.IniciarAtaque();
//    }
//}













