using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class SnakeBreathingAnimationEnhanced : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Controlador de la serpiente")]
    public SnakeController snakeController;

    [Header("Parámetros globales")]
    [Tooltip("Velocidad de la oscilación (ciclos por segundo)")]
    public float frequency = 1f;
    [Tooltip("Desplazamiento de fase entre segmentos (radianes)")]
    public float phaseOffset = 0.5f;
    [Tooltip("Amplitud máxima de la respiración")]
    public float globalAmplitude = 0.1f;
    [Tooltip("Curva que modula la amplitud a lo largo del cuerpo\n(eje X=0 cola → X=1 cabeza, Y=0…1 multiplicador)")]
    public AnimationCurve amplitudeCurve = AnimationCurve.EaseInOut(0, 0.5f, 1, 0.2f);

    [Header("Rampa de inicio")]
    [Tooltip("Duración (segundos) para que la respiración alcance su amplitud completa")]
    public float rampDuration = 2f;

    [Header("Ejes de escala")]
    [Tooltip("¿Aplicar efecto sólo en X/Z (grosor) y no en Y (alto)?")]
    public bool scaleOnlyXZ = true;

    private List<Transform> segmentos;
    private List<Vector3> originalScales;
    private float startTime;
    private bool initialized = false;

    IEnumerator Start()
    {
        if (snakeController == null)
            snakeController = GetComponent<SnakeController>();

        // Espera a que SnakeController instancie sus segmentos
        yield return new WaitUntil(() => snakeController.Segmentos != null && snakeController.Segmentos.Count > 0);

        segmentos = snakeController.Segmentos;
        originalScales = new List<Vector3>(segmentos.Count);
        foreach (var seg in segmentos)
            originalScales.Add(seg.localScale);

        startTime = Time.time;
        initialized = true;
    }

    void Update()
    {
        if (!initialized) return;

        int count = segmentos.Count;
        float elapsed = Time.time - startTime;
        // Factor de rampa 0→1
        float ramp = (rampDuration > 0f) ? Mathf.Clamp01(elapsed / rampDuration) : 1f;

        float timePhase = elapsed * frequency * Mathf.PI * 2f;

        for (int i = 0; i < count; i++)
        {
            float tNorm = (float)i / (count - 1);
            float curveMul = amplitudeCurve.Evaluate(tNorm);

            float phase = timePhase + i * phaseOffset;
            float sin = Mathf.Sin(phase);

            // Aplicamos rampa al globalAmplitude
            float amp = globalAmplitude * ramp * curveMul;
            float scaleFactor = 1f + amp * sin;

            Vector3 orig = originalScales[i];
            Vector3 targetScale = scaleOnlyXZ
                ? new Vector3(orig.x * scaleFactor, orig.y, orig.z * scaleFactor)
                : orig * scaleFactor;

            segmentos[i].localScale = targetScale;
        }
    }
}




















//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class SnakeBreathingAnimation : MonoBehaviour
//{
//    [Header("Referencias")]
//    [Tooltip("Controlador de la serpiente")]
//    public SnakeController snakeController;

//    [Header("Parámetros de respiración (cuerpo)")]
//    [Tooltip("Amplitud del cambio de escala para los segmentos del cuerpo (p. ej. 0.1 para ±10%)")]
//    public float amplitude = 0.1f;
//    [Tooltip("Velocidad de la oscilación (ciclos por segundo)")]
//    public float frequency = 1f;
//    [Tooltip("Retraso de fase entre segmentos (en radianes)")]
//    public float phaseOffset = 0.5f;

//    [Header("Parámetros de respiración (cabeza)")]
//    [Tooltip("Amplitud del cambio de escala para la cabeza (mucho más pequeña que el cuerpo)")]
//    public float headAmplitude = 0.02f;

//    [Header("Multiplicador global")]
//    [Tooltip("Escala global para ajustar el tamaño final de todos los segmentos")]
//    public float scaleMultiplier = 1f;

//    private List<Transform> segmentos;

//    void Awake()
//    {
//        if (snakeController == null)
//            snakeController = GetComponent<SnakeController>();

//        segmentos = snakeController.Segmentos;
//        if (segmentos == null || segmentos.Count == 0)
//            Debug.LogWarning("SnakeBreathingAnimation: No se encontraron segmentos en el SnakeController.");
//    }

//    void Update()
//    {
//        if (segmentos == null || segmentos.Count == 0)
//            return;

//        int count = segmentos.Count;

//        for (int i = 0; i < count; i++)
//        {
//            // Invertimos el índice: 0 = cola ... count-1 = cabeza
//            int invertedIndex = count - 1 - i;
//            Transform seg = segmentos[invertedIndex];

//            // Calculamos la fase común
//            float phase = Time.time * Mathf.PI * 2f * frequency + i * phaseOffset;

//            // Si es la cabeza (invertedIndex == 0), usamos headAmplitude
//            if (invertedIndex == 0)
//            {
//                float headScaleFactor = 1f + headAmplitude * Mathf.Sin(phase);
//                seg.localScale = Vector3.one * headScaleFactor * scaleMultiplier;
//            }
//            else
//            {
//                float bodyScaleFactor = 1f + amplitude * Mathf.Sin(phase);
//                seg.localScale = Vector3.one * bodyScaleFactor * scaleMultiplier;
//            }
//        }
//    }
//}
