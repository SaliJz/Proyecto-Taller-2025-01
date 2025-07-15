// CobraPoseController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class CobraPoseController : MonoBehaviour
{
    public enum Estado { Inactivo, Pose, Anticipacion, Atacando, Retornando }

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
    [Tooltip("Prefab que se genera al llegar al objetivo de ataque")]
    public GameObject prefabAlLlegar;
    [Tooltip("Desfase en Y para instanciar el prefab un poco más abajo")]
    public float instanciaYOffset = 0.5f;
    [Tooltip("Prefab de feedback que indica dónde atacará la cobra")]
    public GameObject prefabEnPosicionJugador;

    [Header("Rotación de cabeza en ataque")]
    public float tiltX = 40f;
    public float tiltSmoothSpeed = 8f;

    private SnakeController snake;
    private ActivadorEfectos efectosActivator;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform player;
    private bool sequenceRunning = false;
    private bool hasTriggered = false;

    private Vector3 headAttackStart;
    private Vector3 baseAttackStart;
    private Vector3 originalAttackTarget;
    private Vector3 attackTargetWithOffset;
    private float timer;
    private GameObject feedbackInstance;

    // Expuesto para anticipación
    public List<Transform> Segmentos => segmentos;
    public Transform Player => player;

    void Start()
    {
        snake = GetComponent<SnakeController>();
        efectosActivator = FindObjectOfType<ActivadorEfectos>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) Debug.LogError("No se encontró ningún GameObject con tag \"Player\".");
        else player = p.transform;

        if (snake != null && snake.Segmentos != null)
            segmentos = snake.Segmentos;

        if (poseOnStart)
            StartCoroutine(FullSequence());
    }

    void Update()
    {
        if (snake != null && snake.Segmentos != null)
            segmentos = snake.Segmentos;

        if (segmentos == null || segmentos.Count == 0 || player == null)
            return;

        float dist = Vector3.Distance(segmentos[0].position, player.position);
        if (dist <= distanciaActivacion && !hasTriggered && !sequenceRunning)
        {
            hasTriggered = true;
            StartCoroutine(FullSequence());
        }
        else if (dist > distanciaActivacion)
            hasTriggered = false;

        switch (estado)
        {
            case Estado.Pose:
                AplicarPose();
                break;
            case Estado.Anticipacion:
                // no rotaciones aquí
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

        estado = Estado.Anticipacion;
        var ant = GetComponent<CobraAnticipationController>();
        if (ant != null)
            yield return StartCoroutine(ant.RunAnticipation());
        else
            IniciarAtaque();

        // Esperar hasta que termine todo el ataque y retorno
        while (estado != Estado.Pose)
            yield return null;

        yield return new WaitForSeconds(delayAntesDeDesactivar);
        SalirPose();
        sequenceRunning = false;
        hasTriggered = false;
    }

    void EntrarPose()
    {
        segmentosCuello = Mathf.Clamp(segmentosCuello, 2, segmentos.Count - 1);
        estado = Estado.Pose;
        snake.enabled = false;
        if (efectosActivator != null)
            efectosActivator.activar = false;

        Vector3 headPos = segmentos[0].position;
        Vector3 dirRaw = (player.position - headPos).normalized;
        float distReal = Vector3.Distance(headPos, player.position);
        float distUsar = Mathf.Min(distReal, distanciaAtaqueMax);

        originalAttackTarget = headPos + dirRaw * distUsar;
        originalAttackTarget.y = player.position.y;

        attackTargetWithOffset = headPos + dirRaw * (distUsar + headOffset);
        attackTargetWithOffset.y = player.position.y;

        if (prefabEnPosicionJugador != null)
        {
            Vector3 feedbackPos = attackTargetWithOffset + Vector3.down * instanciaYOffset;
            feedbackInstance = Instantiate(prefabEnPosicionJugador, feedbackPos, Quaternion.identity);
        }
    }

    void SalirPose()
    {
        estado = Estado.Inactivo;
        if (efectosActivator != null)
            efectosActivator.activar = true;
    }

    public void AplicarPose()
    {
        Vector3 headFlat = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 targetFlat = new Vector3(originalAttackTarget.x, 0, originalAttackTarget.z);
        Vector3 dir = (targetFlat - headFlat).sqrMagnitude < 0.01f
            ? transform.forward
            : (targetFlat - headFlat).normalized;

        Vector3 targetHead = headFlat + Vector3.up * alturaMax;
        Transform cabeza = segmentos[0];
        Vector3 lookDir = targetFlat - new Vector3(cabeza.position.x, 0, cabeza.position.z);

        if (lookDir.sqrMagnitude > 0.001f)
            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, Quaternion.LookRotation(lookDir), poseSmoothSpeed * Time.deltaTime);

        FollowChain(targetHead, dir, poseSmoothSpeed);
    }

    public void IniciarAtaque()
    {
        headAttackStart = segmentos[0].position;
        baseAttackStart = segmentos[segmentosCuello].position;
        estado = Estado.Atacando;

        Transform cabeza = segmentos[0];
        Vector3 look = (attackTargetWithOffset - cabeza.position);
        look.y = 0;
        if (look.sqrMagnitude > 0.001f)
            cabeza.rotation = Quaternion.LookRotation(look);

        timer = 0f;
    }

    void UpdateAtaque()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / duracionAtaque);
        float arcHeight = Mathf.Sin(timer * Mathf.PI) * alturaMax * 0.7f;

        Vector3 headPos = Vector3.Lerp(headAttackStart, attackTargetWithOffset, timer);
        headPos.y += arcHeight;
        segmentos[0].position = headPos;

        Vector3 neckPos = Vector3.Lerp(headAttackStart, originalAttackTarget, timer);
        neckPos.y += arcHeight;
        MoveNeckCurved(neckPos, baseAttackStart, alturaMax * 0.5f);

        Transform cabeza = segmentos[0];
        Vector3 eul = cabeza.rotation.eulerAngles;
        cabeza.rotation = Quaternion.Slerp(cabeza.rotation, Quaternion.Euler(tiltX, eul.y, eul.z), tiltSmoothSpeed * Time.deltaTime);

        if (timer >= 1f)
        {
            if (feedbackInstance != null)
            {
                Destroy(feedbackInstance);
                feedbackInstance = null;
            }
            if (prefabAlLlegar != null)
            {
                Vector3 spawnPos = segmentos[0].position + Vector3.down * instanciaYOffset;
                var inst = Instantiate(prefabAlLlegar, spawnPos, Quaternion.identity);
                Destroy(inst, 2f);
            }
            timer = 0f;
            estado = Estado.Retornando;
        }
    }

    void UpdateRetorno()
    {
        timer = Mathf.Min(1f, timer + Time.deltaTime / duracionRetorno);
        Vector3 returnHead = baseAttackStart + Vector3.up * alturaMax;
        Vector3 headPos = Vector3.Lerp(attackTargetWithOffset, returnHead, timer);
        headPos.y = Mathf.Lerp(headPos.y, returnHead.y, timer);
        segmentos[0].position = headPos;

        Vector3 neckPos = Vector3.Lerp(originalAttackTarget, returnHead, timer);
        MoveNeckCurved(neckPos, baseAttackStart, alturaMax * 0.5f);

        if (timer >= 1f)
            estado = Estado.Pose;
    }

    public void MoveNeckCurved(Vector3 headPos, Vector3 basePos, float controlHeight)
    {
        for (int i = 1; i < segmentosCuello; i++)
        {
            float t = (float)i / (segmentosCuello - 1);
            Vector3 lin = Vector3.Lerp(headPos, basePos, t);
            float arc = Mathf.Sin(t * Mathf.PI);
            Vector3 curved = lin + Vector3.up * controlHeight * arc;
            segmentos[i].position = curved;

            Vector3 lookTgt = (i < segmentosCuello - 1) ? segmentos[i + 1].position : basePos;
            Vector3 lookDir = lookTgt - segmentos[i].position;
            lookDir.y = 0;
            if (lookDir.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(segmentos[i].rotation, Quaternion.LookRotation(lookDir), poseSmoothSpeed * Time.deltaTime);
        }
    }

    void FollowChain(Vector3 headT, Vector3 dirXZ, float speed)
    {
        float sep = snake.separacionSegmentos;
        float half = Mathf.PI * 0.5f;
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
                ? Mathf.Sin((1f - t) * half) * alturaMax
                : 0f;

            Vector3 bentXZ = new Vector3(bent.x, 0, bent.z).normalized;
            Vector3 tgt = prev - bentXZ * sep;
            tgt.y = yOff;
            segmentos[i].position = Vector3.Lerp(segmentos[i].position, tgt, speed * Time.deltaTime);

            Vector3 look = prev - segmentos[i].position;
            look.y = 0;
            if (look.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(segmentos[i].rotation, Quaternion.LookRotation(look), speed * Time.deltaTime);

            prev = segmentos[i].position;
        }
    }

    void OnDestroy()
    {
        if (feedbackInstance != null)
            Destroy(feedbackInstance);
    }
}























