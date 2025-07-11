// CobraAnticipationController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CobraPoseController))]
public class CobraAnticipationController : MonoBehaviour
{
    [Header("Anticipaci�n antes de ataque")]
    [Tooltip("Duraci�n total de la fase de anticipaci�n")]
    public float duracionAnticipacion = 0.7f;

    [Tooltip("Desplazamiento m�ximo hacia atr�s de cada segmento")]
    public float headOffset = 0.5f;

    [Tooltip("Factor de offset para los segmentos del cuello (multiplica headOffset)")]
    [Range(0f, 1f)]
    public float neckOffsetFactor = 0.7f;

    [Tooltip("Multiplicador global del offset en anticipaci�n")]
    public float offsetMultiplier = 1f;

    private CobraPoseController poseCtrl;

    void Awake()
    {
        poseCtrl = GetComponent<CobraPoseController>();
    }

    /// <summary>
    /// Ejecuta la anticipaci�n en forma de onda:
    /// primero la base del cuello, luego sucesivamente hasta la cabeza.
    /// Solo traslada posiciones, NO rota ning�n segmento.
    /// </summary>
    public IEnumerator RunAnticipation()
    {
        // Asegurar que la pose ya est� aplicada
        poseCtrl.AplicarPose();

        int lastIdx = Mathf.Clamp(poseCtrl.segmentosCuello, 1, poseCtrl.Segmentos.Count - 1);
        int totalSegs = lastIdx + 1;  // incluye cabeza

        // Capturar posiciones originales tras la pose
        List<Vector3> originalPositions = new List<Vector3>(totalSegs);
        for (int i = 0; i <= lastIdx; i++)
            originalPositions.Add(poseCtrl.Segmentos[i].position);

        // Direcci�n hacia atr�s respecto al punto de ataque
        Vector3 backDir = -(poseCtrl.Player.position - originalPositions[0]).normalized;
        backDir.y = 0;

        float elapsed = 0f;
        while (elapsed < duracionAnticipacion)
        {
            elapsed += Time.deltaTime;
            float tNorm = Mathf.Clamp01(elapsed / duracionAnticipacion);

            for (int i = 0; i <= lastIdx; i++)
            {
                float segDelayNorm = (lastIdx - i) / (float)totalSegs;
                float segDuration = 1f / totalSegs;
                float localT = Mathf.Clamp01((tNorm - segDelayNorm) / segDuration);
                float easeT = Mathf.SmoothStep(0f, 1f, localT);

                float factor = (i == 0) ? 1f : neckOffsetFactor;
                float totalOffset = headOffset * factor * offsetMultiplier;

                // Solo translaci�n, sin rotaci�n
                Vector3 targetPos = originalPositions[i] + backDir * totalOffset * easeT;
                poseCtrl.Segmentos[i].position = targetPos;
            }

            yield return null;
        }

        // Tras la anticipaci�n, lanzar el ataque
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
//    [Header("Anticipaci�n antes de ataque")]
//    [Tooltip("Duraci�n total de la fase de anticipaci�n")]
//    public float duracionAnticipacion = 0.7f;

//    [Tooltip("Desplazamiento m�ximo hacia atr�s de cada segmento")]
//    public float headOffset = 0.5f;

//    [Tooltip("Factor de offset para los segmentos del cuello (multiplica headOffset)")]
//    [Range(0f, 1f)]
//    public float neckOffsetFactor = 0.7f;

//    [Tooltip("Multiplicador global del offset en anticipaci�n")]
//    public float offsetMultiplier = 1f;

//    private CobraPoseController poseCtrl;

//    void Awake()
//    {
//        poseCtrl = GetComponent<CobraPoseController>();
//    }

//    /// <summary>
//    /// Ejecuta la anticipaci�n en forma de onda:
//    /// primero el segmento base del cuello, luego sucesivamente hasta la cabeza.
//    /// Solo traslada posiciones, NO rota ning�n segmento.
//    /// </summary>
//    public IEnumerator RunAnticipation()
//    {
//        int lastIdx = Mathf.Clamp(poseCtrl.segmentosCuello, 1, poseCtrl.Segmentos.Count - 1);
//        int totalSegs = lastIdx + 1;  // incluye cabeza

//        // Capturar posiciones originales
//        List<Vector3> originalPositions = new List<Vector3>(totalSegs);
//        for (int i = 0; i <= lastIdx; i++)
//            originalPositions.Add(poseCtrl.Segmentos[i].position);

//        // Direcci�n hacia atr�s respecto al punto de ataque
//        Vector3 backDir = -(poseCtrl.Player.position - originalPositions[0]).normalized;
//        backDir.y = 0;

//        float elapsed = 0f;
//        while (elapsed < duracionAnticipacion)
//        {
//            elapsed += Time.deltaTime;
//            float tNorm = Mathf.Clamp01(elapsed / duracionAnticipacion);

//            for (int i = 0; i <= lastIdx; i++)
//            {
//                // Delay secuencial para cada segmento
//                float segDelayNorm = (lastIdx - i) / (float)totalSegs;
//                float segDuration = 1f / totalSegs;
//                float localT = Mathf.Clamp01((tNorm - segDelayNorm) / segDuration);
//                float easeT = Mathf.SmoothStep(0f, 1f, localT);

//                // Offset escalado: cuello ligeramente menos que cabeza
//                float factor = (i == 0) ? 1f : neckOffsetFactor;
//                float totalOffset = headOffset * factor * offsetMultiplier;

//                // Solo trasladamos la posici�n
//                Vector3 targetPos = originalPositions[i] + backDir * totalOffset * easeT;
//                poseCtrl.Segmentos[i].position = targetPos;
//            }

//            yield return null;
//        }

//        // Tras la anticipaci�n, lanzamos el ataque
//        poseCtrl.IniciarAtaque();
//    }
//}
