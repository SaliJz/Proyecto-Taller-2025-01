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

    [Header("Ataque")]
    public float distanciaAtaqueMax = 8f;
    public float duracionAtaque = 0.5f;
    public float duracionRetorno = 0.7f;
    [Tooltip("Distancia extra que avanzará solo la cabeza durante el ataque")]
    public float headOffset = 0.5f;

    [Header("Rotación de cabeza en ataque")]
    [Tooltip("Ángulo en X al que se inclina la cabeza durante el ataque")]
    public float tiltX = 40f;
    [Tooltip("Velocidad de interpolación de la rotación X durante el ataque")]
    public float tiltSmoothSpeed = 8f;

    private SnakeController snake;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform player;
    private bool sequenceRunning = false;
    private bool hasTriggered = false;

    private Vector3 headAttackStart;
    private Vector3 baseAttackStart;
    private Vector3 originalAttackTarget;       // sin offset
    private Vector3 attackTargetWithOffset;     // con offset
    private float timer;

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
        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 playerFlat = new Vector3(player.position.x, 0, player.position.z);
        Vector3 dir = (playerFlat - headFlat).sqrMagnitude < 0.01f
            ? transform.forward
            : (playerFlat - headFlat).normalized;
        Vector3 targetHead = headFlat + Vector3.up * alturaMax;

        Transform cabeza = segmentos[0];
        Vector3 lookDirToPlayer = (playerFlat - cabeza.position);
        lookDirToPlayer.y = 0;
        if (lookDirToPlayer.sqrMagnitude > 0.001f)
            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, Quaternion.LookRotation(lookDirToPlayer), poseSmoothSpeed * Time.deltaTime);

        FollowChain(targetHead, dir, poseSmoothSpeed);
    }

    void IniciarAtaque()
    {
        segmentos = snake.Segmentos;
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);

        headAttackStart = segmentos[0].position;
        baseAttackStart = segmentos[segmentosCuello].position;

        Vector3 dirRaw = (player.position - headAttackStart).normalized;
        float distanciaReal = Vector3.Distance(headAttackStart, player.position);
        float distanciaUsar = Mathf.Min(distanciaReal, distanciaAtaqueMax);

        originalAttackTarget = headAttackStart + dirRaw * distanciaUsar;
        originalAttackTarget.y = player.position.y;

        attackTargetWithOffset = headAttackStart + dirRaw * (distanciaUsar + headOffset);
        attackTargetWithOffset.y = player.position.y;

        // orientar cabeza hacia target con offset
        Transform cabeza = segmentos[0];
        Vector3 lookDir = (attackTargetWithOffset - cabeza.position);
        lookDir.y = 0;
        if (lookDir.sqrMagnitude > 0.001f)
            cabeza.rotation = Quaternion.LookRotation(lookDir);

        timer = 0f;
        estado = Estado.Atacando;
    }

    void UpdateAtaque()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / duracionAtaque);
        float arcHeight = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.7f;

        // solo cabeza con offset
        Vector3 headPos = Vector3.Lerp(headAttackStart, attackTargetWithOffset, timer);
        headPos.y += arcHeight;
        segmentos[0].position = headPos;

        // cuello sin seguir offset
        Vector3 neckHeadPos = Vector3.Lerp(headAttackStart, originalAttackTarget, timer);
        neckHeadPos.y += arcHeight;
        MoveNeckCurved(neckHeadPos, baseAttackStart, alturaMax * 0.5f);

        // tilt solo cabeza
        Transform cabeza = segmentos[0];
        Vector3 eul = cabeza.rotation.eulerAngles;
        cabeza.rotation = Quaternion.Slerp(cabeza.rotation, Quaternion.Euler(tiltX, eul.y, eul.z), tiltSmoothSpeed * Time.deltaTime);

        if (timer >= 1f)
        {
            timer = 0f;
            estado = Estado.Retornando;
        }
    }

    void UpdateRetorno()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / duracionRetorno);
        Vector3 targetReturnHead = baseAttackStart + Vector3.up * alturaMax;
        Vector3 headPos = Vector3.Lerp(attackTargetWithOffset, targetReturnHead, timer);
        headPos.y = Mathf.Lerp(headPos.y, targetReturnHead.y, timer);
        segmentos[0].position = headPos;

        Vector3 neckHeadPos = Vector3.Lerp(originalAttackTarget, targetReturnHead, timer);
        MoveNeckCurved(neckHeadPos, baseAttackStart, alturaMax * 0.5f);

        if (timer >= 1f)
            estado = Estado.Pose;
    }

    void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float controlHeight)
    {
        // sólo mueve segmentos 1..segmentosCuello-1
        for (int i = 1; i < segmentosCuello; i++)
        {
            float t = (float)i / (segmentosCuello - 1);
            Vector3 linearPos = Vector3.Lerp(headPos, basePos, t);
            float arcFactor = Mathf.Sin(t * Mathf.PI);
            Vector3 curvedPos = linearPos + Vector3.up * controlHeight * arcFactor;
            segmentos[i].position = curvedPos;

            Vector3 lookTarget = (i < segmentosCuello - 1)
                ? segmentos[i + 1].position
                : basePos;
            Vector3 lookDir = lookTarget - segmentos[i].position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(lookDir),
                    poseSmoothSpeed * Time.deltaTime
                );
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

            Vector3 look = prev - segmentos[i].position;
            look.y = 0;
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

























































































