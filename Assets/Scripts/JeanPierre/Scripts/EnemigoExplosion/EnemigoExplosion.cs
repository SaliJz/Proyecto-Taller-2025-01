using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemigoExplosion : MonoBehaviour
{
    [Header("Teletransportación")]
    public float minTeleportDistance = 1f;
    public float maxTeleportDistance = 3f;
    public int minTeleports = 3;
    public int maxTeleports = 4;
    public float zigzagTeleportAmplitude = 0.5f;
    public float lateralTeleportChance = 0.3f;

    [Header("Movimiento Continuo")]
    public float minAdvanceTime = 2f;
    public float maxAdvanceTime = 4f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;

    [Header("Movimiento Continuo - Zigzag")]
    public float continuousZigzagAmplitude = 1f;
    public float continuousZigzagFrequency = 2f;

    [Header("Transición de Opacidad (Efecto Espectral)")]
    public float disappearDuration = 0.1f;
    public float appearDuration = 0.1f;

    [Header("Referencias")]
    public GameObject toggleObject;

    private Transform playerTransform;
    private EnemyAbilityReceiver abilityReceiver;
    private NavMeshAgent agent;
    private bool canMoveContinuously = false;
    private bool toggleZigzag = false;
    private Vector3 previousLateralOffset = Vector3.zero;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        // Desactivamos la rotación automática para usar RotateTowards
        agent.updateRotation = false;
        agent.updatePosition = false;
        agent.speed = moveSpeed;
    }

    void OnEnable()
    {
        StartCoroutine(TeleportMovementRoutine());
    }

    void OnDisable()
    {
        StopAllCoroutines();
        canMoveContinuously = false;
        previousLateralOffset = Vector3.zero;
    }

    void Update()
    {
        if (playerTransform != null)
        {
            // Rotación hacia el jugador
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }

        if (canMoveContinuously)
        {
            // Cálculo del zigzag lateral
            float lateralOsc = continuousZigzagAmplitude * Mathf.Sin(Time.time * continuousZigzagFrequency * 2f * Mathf.PI);
            float speed = (abilityReceiver != null) ? abilityReceiver.CurrentSpeed : moveSpeed;
            agent.speed = speed;

            Vector3 lateralOffset = transform.right * lateralOsc;
            Vector3 forwardMove = transform.forward * speed * Time.deltaTime;
            Vector3 moveDelta = forwardMove + (lateralOffset - previousLateralOffset);

            // Aplicamos el movimiento con el agente
            agent.Move(moveDelta);
            transform.position = agent.nextPosition;  // sincronizar posición

            previousLateralOffset = lateralOffset;
        }
        else
        {
            // Si no se mueve, aseguramos que el agente esté quieto
            agent.nextPosition = transform.position;
        }
    }

    IEnumerator TeleportMovementRoutine()
    {
        while (true)
        {
            int teleportCount = Random.Range(minTeleports, maxTeleports + 1);
            for (int i = 0; i < teleportCount; i++)
                yield return StartCoroutine(DoTeleport());

            canMoveContinuously = true;
            yield return new WaitForSeconds(Random.Range(minAdvanceTime, maxAdvanceTime));
            canMoveContinuously = false;
        }
    }

    IEnumerator DoTeleport()
    {
        // Apagamos el objeto (por ejemplo, efectos de pantalla)
        if (toggleObject != null)
            toggleObject.SetActive(false);

        if (playerTransform != null)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            float teleportDistance = Random.Range(minTeleportDistance, maxTeleportDistance);
            Vector3 perp = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
            float lateral = (toggleZigzag ? 1f : -1f) * zigzagTeleportAmplitude;
            toggleZigzag = !toggleZigzag;
            Vector3 zigzagOffset = perp * lateral;

            Vector3 targetPos;
            if (Random.value < lateralTeleportChance)
                targetPos = transform.position + zigzagOffset;
            else
                targetPos = transform.position + dirToPlayer * teleportDistance + zigzagOffset;

            // Usamos NavMeshAgent.Warp para teletransportar manteniendo validez en la malla
            agent.Warp(targetPos);
            transform.position = agent.nextPosition;
        }

        // Volvemos a encender el objeto
        if (toggleObject != null)
            toggleObject.SetActive(true);

        yield return new WaitForSeconds(appearDuration);
    }
}











//// EnemigoExplosion.cs (sin cambios)
//using UnityEngine;
//using System.Collections;

//public class EnemigoExplosion : MonoBehaviour
//{
//    [Header("Teletransportación")]
//    public float minTeleportDistance = 1f;
//    public float maxTeleportDistance = 3f;
//    public int minTeleports = 3;
//    public int maxTeleports = 4;
//    public float zigzagTeleportAmplitude = 0.5f;
//    public float lateralTeleportChance = 0.3f;

//    [Header("Movimiento Continuo")]
//    public float minAdvanceTime = 2f;
//    public float maxAdvanceTime = 4f;
//    public float moveSpeed = 3f;
//    public float rotationSpeed = 180f;

//    [Header("Movimiento Continuo - Zigzag")]
//    public float continuousZigzagAmplitude = 1f;
//    public float continuousZigzagFrequency = 2f;

//    [Header("Transición de Opacidad (Efecto Espectral)")]
//    public float disappearDuration = 0.1f;
//    public float appearDuration = 0.1f;

//    [Header("Referencias")]
//    public GameObject toggleObject;

//    private Transform playerTransform;
//    private EnemyAbilityReceiver abilityReceiver;
//    private bool canMoveContinuously = false;
//    private bool toggleZigzag = false;
//    private Vector3 previousLateralOffset = Vector3.zero;

//    void Awake()
//    {
//        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//    }

//    void OnEnable()
//    {
//        StartCoroutine(TeleportMovementRoutine());
//    }

//    void OnDisable()
//    {
//        StopAllCoroutines();
//        canMoveContinuously = false;
//        previousLateralOffset = Vector3.zero;
//    }

//    void Update()
//    {
//        if (playerTransform != null)
//        {
//            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
//            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);
//            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
//        }

//        if (canMoveContinuously)
//        {
//            float lateralOsc = continuousZigzagAmplitude * Mathf.Sin(Time.time * continuousZigzagFrequency * 2f * Mathf.PI);
//            float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : moveSpeed;
//            Vector3 lateralOffset = transform.right * lateralOsc;
//            Vector3 forwardMove = transform.forward * speed * Time.deltaTime;
//            transform.position += forwardMove + (lateralOffset - previousLateralOffset);
//            previousLateralOffset = lateralOffset;
//        }
//    }

//    IEnumerator TeleportMovementRoutine()
//    {
//        while (true)
//        {
//            int teleportCount = Random.Range(minTeleports, maxTeleports + 1);
//            for (int i = 0; i < teleportCount; i++)
//                yield return StartCoroutine(DoTeleport());

//            canMoveContinuously = true;
//            yield return new WaitForSeconds(Random.Range(minAdvanceTime, maxAdvanceTime));
//            canMoveContinuously = false;
//        }
//    }

//    IEnumerator DoTeleport()
//    {
//        if (toggleObject != null)
//            toggleObject.SetActive(false);

//        if (playerTransform != null)
//        {
//            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
//            float teleportDistance = Random.Range(minTeleportDistance, maxTeleportDistance);
//            Vector3 perp = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
//            float lateral = (toggleZigzag ? 1f : -1f) * zigzagTeleportAmplitude;
//            toggleZigzag = !toggleZigzag;
//            Vector3 zigzagOffset = perp * lateral;

//            if (Random.value < lateralTeleportChance)
//                transform.position += zigzagOffset;
//            else
//                transform.position += dirToPlayer * teleportDistance + zigzagOffset;
//        }

//        if (toggleObject != null)
//            toggleObject.SetActive(true);

//        yield return new WaitForSeconds(appearDuration);
//    }
//}