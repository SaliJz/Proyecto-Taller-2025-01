using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class CobraPoseController : MonoBehaviour
{
    public enum Estado { Inactivo, Pose, Atacando, Retornando }

    [Header("Pose de cobra")]
    public bool poseOnStart = false;
    public float alturaMax = 2f;
    [Range(0, 180)] public float anguloCurva = 90f;
    [Min(2)] public int segmentosCuello = 8;
    public float poseSmoothSpeed = 10f;

    [Header("Secuencia automática")]
    public float delayAntesDePose = 1f;
    public float delayAntesDeAtaque = 1.5f;
    public float delayAntesDeDesactivar = 1f;
    public float distanciaActivacion = 10f;

    private SnakeController snake;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform player;
    private bool sequenceRunning = false;
    private bool hasTriggered = false;

    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
    private float timer;
    private float neckLength; // ← distancia máxima cabeza–base

    void Start()
    {
        snake = GetComponent<SnakeController>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
        else player = p.transform;

        if (poseOnStart) EntrarPose();
    }

    void Update()
    {
        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0) return;
        if (player == null) return;

        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);

        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
        {
            hasTriggered = true;
            StartCoroutine(FullSequence());
        }
        else if (dist > distanciaActivacion)
        {
            hasTriggered = false;
        }

        switch (estado)
        {
            case Estado.Pose:
                AplicarPose();
                break;
            case Estado.Atacando:
                UpdateAtaque();
                break;
            case Estado.Retornando:
                UpdateRetorno();
                break;
            case Estado.Inactivo:
                snake.enabled = true;
                break;
        }
    }

    private IEnumerator FullSequence()
    {
        sequenceRunning = true;
        yield return new WaitForSeconds(delayAntesDePose);
        EntrarPose();
        yield return new WaitForSeconds(delayAntesDeAtaque);
        IniciarAtaque();
        while (estado != Estado.Pose) yield return null;
        yield return new WaitForSeconds(delayAntesDeDesactivar);
        SalirPose();
        sequenceRunning = false;
        hasTriggered = false;
    }

    void EntrarPose()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
        estado = Estado.Pose;
        snake.enabled = false;
    }

    void SalirPose() => estado = Estado.Inactivo;

    void AplicarPose()
    {
        if (segmentos == null) return;
        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
            ? transform.forward
            : (playerFlat - headFlat).normalized;
        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
        FollowChain(targetHead, dir, poseSmoothSpeed);
    }

    void IniciarAtaque()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

        headAttackStart = segmentos[0].position;
        baseAttackStart = segmentos[segmentosCuello].position;

        // Calculamos la longitud máxima del cuello
        neckLength = Vector3.Distance(headAttackStart, baseAttackStart);

        // Obtenemos dirección y limitamos target para no sobrepasar neckLength
        Vector3 rawDir = (player.position - baseAttackStart).normalized;
        attackTarget = baseAttackStart + rawDir * neckLength;

        timer = 0f;
        estado = Estado.Atacando;
    }

    void UpdateAtaque()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeAtaque);

        // Lerp horizontal con arco vertical
        Vector3 flatLerp = Vector3.Lerp(headAttackStart, attackTarget, timer);
        float arcY = Mathf.Sin(timer * Mathf.PI) * alturaMax;
        Vector3 headPos = flatLerp + Vector3.up * arcY;

        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

        if (timer >= 1f)
        {
            headAttackEnd = segmentos[0].position;
            timer = 0f;
            estado = Estado.Retornando;
        }
    }

    void UpdateRetorno()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeDesactivar);
        Vector3 returnHead = baseAttackStart + Vector3.up * alturaMax;
        Vector3 headPos = Vector3.Lerp(headAttackEnd, returnHead, timer);
        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

        if (timer >= 1f) estado = Estado.Pose;
    }

    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float alturaControl)
    {
        Vector3 p0 = headPos, p2 = basePos;
        Vector3 p1 = (p0 + p2) * 0.5f + Vector3.up * alturaControl;
        for (int i = 0; i < segmentosCuello; i++)
        {
            float t = (float)i / (segmentosCuello - 1);
            Vector3 pos = Mathf.Pow(1 - t, 2) * p0
                        + 2 * (1 - t) * t * p1
                        + Mathf.Pow(t, 2) * p2;
            segmentos[i].position = pos;

            float tn = Mathf.Min(1f, t + 1f / (segmentosCuello - 1));
            Vector3 next = Mathf.Pow(1 - tn, 2) * p0
                         + 2 * (1 - tn) * tn * p1
                         + Mathf.Pow(tn, 2) * p2;
            Vector3 dir = next - pos; dir.y = 0;
            if (dir.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.LookRotation(dir);
        }
    }

    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
    {
        float sep = snake.separacionSegmentos;
        float halfPi = Mathf.PI * 0.5f;
        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
        Vector3 prev = segmentos[0].position;
        for (int i = 1; i < segmentos.Count; i++)
        {
            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
            Vector3 bent = (i < segmentosCuello)
                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
                : dirXZ;
            float yOff = (i < segmentosCuello)
                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
                : 0f;
            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
            Vector3 target = prev - bentXZ * sep;
            target.y = yOff;
            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
            Vector3 look = prev - segmentos[i].position; look.y = 0;
            if (look.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(look),
                    speed * Time.deltaTime
                );
            prev = segmentos[i].position;
        }
    } 
}




//// CobraPoseController.cs
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraPoseController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Atacando, Retornando }

//    [Header("Pose de cobra")]
//    public bool poseOnStart = false;
//    public float alturaMax = 2f;
//    [Range(0, 180)] public float anguloCurva = 90f;
//    [Min(2)] public int segmentosCuello = 8;
//    public float poseSmoothSpeed = 10f;

//    [Header("Secuencia automática")]
//    public float delayAntesDePose = 1f;
//    public float delayAntesDeAtaque = 1.5f;
//    public float delayAntesDeDesactivar = 1f;
//    public float distanciaActivacion = 10f;

//    private SnakeController snake;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform player;
//    private bool sequenceRunning = false;
//    private bool hasTriggered = false;

//    private Vector3 headAttackStart, baseAttackStart, headAttackEnd, attackTarget;
//    private float timer;

//    void Start()
//    {
//        snake = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
//        else player = p.transform;

//        if (poseOnStart) EntrarPose();
//    }

//    void Update()
//    {
//        // Evita errores si todavía no hay segmentos instanciados
//        if (snake == null || snake.Segmentos == null || snake.Segmentos.Count == 0)
//            return;

//        if (player == null) return;

//        // Medir distancia desde la cabeza móvil, no desde el pivot
//        float dist = Vector3.Distance(snake.Segmentos[0].position, player.position);
//        //Debug.Log($"[Cobra] dist={dist:F2}, umbral={distanciaActivacion:F2}");

//        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
//        {
//            hasTriggered = true;
//            StartCoroutine(FullSequence());
//        }
//        else if (dist > distanciaActivacion)
//        {
//            hasTriggered = false;
//        }

//        switch (estado)
//        {
//            case Estado.Pose: AplicarPose(); break;
//            case Estado.Atacando: UpdateAtaque(); break;
//            case Estado.Retornando: UpdateRetorno(); break;
//            case Estado.Inactivo: snake.enabled = true; break;
//        }
//    }

//    private IEnumerator FullSequence()
//    {
//        sequenceRunning = true;
//        yield return new WaitForSeconds(delayAntesDePose);
//        EntrarPose();
//        yield return new WaitForSeconds(delayAntesDeAtaque);
//        IniciarAtaque();
//        while (estado != Estado.Pose) yield return null;
//        yield return new WaitForSeconds(delayAntesDeDesactivar);
//        SalirPose();
//        sequenceRunning = false;
//        hasTriggered = false;
//    }

//    void EntrarPose()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        snake.enabled = false;
//    }

//    void SalirPose() => estado = Estado.Inactivo;

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
//        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (playerFlat - headFlat).normalized;
//        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
//        FollowChain(targetHead, dir, poseSmoothSpeed);
//    }

//    void IniciarAtaque()
//    {
//        segmentos = snake.Segmentos;
//        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
//        headAttackStart = segmentos[0].position;
//        baseAttackStart = segmentos[segmentosCuello].position;
//        attackTarget = player.position;
//        timer = 0f;
//        estado = Estado.Atacando;
//    }

//    void UpdateAtaque()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeAtaque);
//        Vector3 flatLerp = Vector3.Lerp(headAttackStart, attackTarget, timer);
//        float arcY = Mathf.Sin(timer * Mathf.PI) * alturaMax;
//        Vector3 headPos = flatLerp + Vector3.up * arcY;
//        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

//        if (timer >= 1f)
//        {
//            headAttackEnd = segmentos[0].position;
//            timer = 0f;
//            estado = Estado.Retornando;
//        }
//    }

//    void UpdateRetorno()
//    {
//        timer = Mathf.Min(1f, timer + Time.deltaTime / delayAntesDeDesactivar);
//        Vector3 returnHead = baseAttackStart + Vector3.up * alturaMax;
//        Vector3 headPos = Vector3.Lerp(headAttackEnd, returnHead, timer);
//        MoveNeckCurved(headPos, baseAttackStart, alturaMax);

//        if (timer >= 1f) estado = Estado.Pose;
//    }

//    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float alturaControl)
//    {
//        Vector3 p0 = headPos, p2 = basePos;
//        Vector3 p1 = (p0 + p2) * 0.5f + Vector3.up * alturaControl;
//        for (int i = 0; i < segmentosCuello; i++)
//        {
//            float t = (float)i / (segmentosCuello - 1);
//            Vector3 pos = Mathf.Pow(1 - t, 2) * p0
//                        + 2 * (1 - t) * t * p1
//                        + Mathf.Pow(t, 2) * p2;
//            segmentos[i].position = pos;

//            float tn = Mathf.Min(1f, t + 1f / (segmentosCuello - 1));
//            Vector3 next = Mathf.Pow(1 - tn, 2) * p0
//                         + 2 * (1 - tn) * tn * p1
//                         + Mathf.Pow(tn, 2) * p2;
//            Vector3 dir = next - pos; dir.y = 0;
//            if (dir.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.LookRotation(dir);
//        }
//    }

//    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
//    {
//        float sep = snake.separacionSegmentos;
//        float halfPi = Mathf.PI * 0.5f;
//        Vector3 right = Vector3.Cross(dirXZ, Vector3.up).normalized;
//        segmentos[0].position = Vector3.Lerp(segmentos[0].position, headT, speed * Time.deltaTime);
//        Vector3 prev = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < segmentosCuello) ? (float)i / (segmentosCuello - 1) : 1f;
//            Vector3 bent = (i < segmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurva * t, right) * dirXZ
//                : dirXZ;
//            float yOff = (i < segmentosCuello)
//                ? Mathf.Sin((1f - t) * halfPi) * alturaMax
//                : 0f;
//            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
//            Vector3 target = prev - bentXZ * sep;
//            target.y = yOff;
//            segmentos[i].position = Vector3.Lerp(segmentos[i].position, target, speed * Time.deltaTime);
//            Vector3 look = prev - segmentos[i].position; look.y = 0;
//            if (look.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(look),
//                    speed * Time.deltaTime
//                );
//            prev = segmentos[i].position;
//        }
//    }
//}




















































