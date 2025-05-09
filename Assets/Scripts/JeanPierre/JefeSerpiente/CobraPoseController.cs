// CobraPoseController.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class CobraPoseController : MonoBehaviour
{
    [Header("Configuración de la pose de cobra vertical")]
    [Tooltip("¿Adoptar la pose al iniciar?")]
    public bool poseOnStart = false;

    [Tooltip("Altura máxima del cuello (cabeza)")]
    public float alturaMax = 2f;

    [Tooltip("Ángulo total de curvatura para el cuello (grados)")]
    [Range(0, 180)]
    public float anguloCurva = 90f;

    [Tooltip("Cuántos segmentos forman el cuello arqueado")]
    public int segmentosCuello = 8;

    [Tooltip("Velocidad de interpolación para la pose (más alto = más rápido)")]
    public float poseSmoothSpeed = 10f;

    private SnakeController snake;
    private List<Transform> segmentos;
    private bool inPose = false;

    // Altura fija del suelo
    private const float pisoY = 0f;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        if (poseOnStart) EnterCobraPose();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (inPose) ExitCobraPose();
            else EnterCobraPose();
        }

        if (inPose)
            ApplyPose();
    }

    private void EnterCobraPose()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count);
        inPose = true;
        snake.enabled = false;
    }

    private void ExitCobraPose()
    {
        inPose = false;
        snake.enabled = true;
    }

    private void ApplyPose()
    {
        if (segmentos == null || segmentos.Count == 0) return;
        // 1) Determinar "adelante" hacia el jugador en el plano horizontal
        Vector3 headWorld = segmentos[0].position;
        Vector3 headGround = new Vector3(headWorld.x, pisoY, headWorld.z);
        Vector3 playerGround = new Vector3(snake.jugador.position.x, pisoY, snake.jugador.position.z);
        Vector3 forwardXZ = (playerGround - headGround).normalized;
        if (forwardXZ.sqrMagnitude < 0.0001f)
            forwardXZ = transform.forward; // fallback

        Vector3 rightAxis = Vector3.Cross(forwardXZ, Vector3.up).normalized;
        float sep = snake.separacionSegmentos;
        float halfPi = Mathf.PI * 0.5f;

        // 2) Posicionar la cabeza
        float yOff0 = Mathf.Sin(halfPi) * alturaMax;  // = alturaMax
        Vector3 prevPos = headGround + Vector3.up * yOff0;
        segmentos[0].position = Vector3.Lerp(segmentos[0].position, prevPos, poseSmoothSpeed * Time.deltaTime);

        // 3) Cada segmento a partir del 1
        for (int i = 1; i < segmentos.Count; i++)
        {
            Transform seg = segmentos[i];
            Vector3 bentDir;
            float yOff = 0f;

            if (i < segmentosCuello)
            {
                // cuello arqueado
                float tN = (float)i / (segmentosCuello - 1);
                float bendA = anguloCurva * tN;
                bentDir = Quaternion.AngleAxis(bendA, rightAxis) * forwardXZ;
                yOff = Mathf.Sin((1f - tN) * halfPi) * alturaMax;
            }
            else
            {
                // resto del cuerpo, recto hacia atrás
                bentDir = forwardXZ;
            }

            // mantener solo componente XZ para espaciado
            Vector3 bentDirXZ = new Vector3(bentDir.x, 0, bentDir.z).normalized;
            Vector3 targetPos = prevPos - bentDirXZ * sep;
            targetPos.y = pisoY + yOff;

            seg.position = Vector3.Lerp(seg.position, targetPos, poseSmoothSpeed * Time.deltaTime);

            // orientación horizontal suave
            Vector3 look = (prevPos - seg.position);
            look.y = 0;
            if (look.sqrMagnitude > 0.0001f)
            {
                Quaternion rot = Quaternion.LookRotation(look);
                seg.rotation = Quaternion.Slerp(seg.rotation, rot, poseSmoothSpeed * Time.deltaTime);
            }

            prevPos = seg.position;
        }
    }
}


//// CobraPoseController.cs
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    [Header("Configuración de la pose de cobra vertical")]
//    [Tooltip("¿Adoptar la pose al iniciar?")]
//    public bool poseOnStart = false;

//    [Tooltip("Altura máxima de elevación (cabeza)")]
//    public float alturaMax = 2f;

//    [Tooltip("Ángulo total de la curva en grados")]
//    [Range(0, 180)]
//    public float anguloCurva = 90f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private bool inPose = false;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        segmentos = snake.Segmentos;

//        if (poseOnStart)
//            EnterCobraPose();
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.C))
//        {
//            if (inPose) ExitCobraPose();
//            else EnterCobraPose();
//        }
//    }

//    private void EnterCobraPose()
//    {
//        inPose = true;
//        snake.enabled = false;    // pausa el seguimiento
//        SetCobraPose();
//    }

//    private void ExitCobraPose()
//    {
//        inPose = false;
//        snake.enabled = true;     // reanuda el seguimiento
//    }

//    private void SetCobraPose()
//    {
//        if (segmentos == null || segmentos.Count == 0) return;

//        // Posición y dirección de la cabeza actuales
//        Vector3 headPos = segmentos[0].position;
//        Vector3 forward = segmentos[0].forward.normalized;

//        int total = segmentos.Count;
//        float angRad = anguloCurva * Mathf.Deg2Rad;

//        for (int i = 0; i < total; i++)
//        {
//            Transform seg = segmentos[i];
//            float t = (float)i / (total - 1);  // 0 = cabeza, 1 = cola

//            // Línea recta detrás de la cabeza
//            Vector3 basePos = headPos - forward * snake.separacionSegmentos * i;

//            // Curva vertical tipo 'S' en Y: sin de 0->angRad
//            float yOffset = Mathf.Sin(t * angRad) * alturaMax;

//            // Ajuste para que sólo la cabeza alcance alturaMax
//            if (i == total - 1)
//                yOffset = 0f;  // cola a nivel de base

//            seg.position = new Vector3(basePos.x, basePos.y + yOffset, basePos.z);

//            // Orientación: mirar hacia el segmento anterior (o mantener forward si i==0)
//            if (i > 0)
//                seg.LookAt(new Vector3(segmentos[i - 1].position.x, seg.position.y, segmentos[i - 1].position.z));
//        }
//    }
//}
