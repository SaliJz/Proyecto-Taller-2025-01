using UnityEngine;
using UnityEngine.AI;

public class EnemigoExplosion : MonoBehaviour
{
    [Header("Movimiento Continuo")]
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;

    [Header("Referencias")]
    public GameObject toggleObject;

    private Transform playerTransform;
    private EnemyAbilityReceiver abilityReceiver;
    private NavMeshAgent agent;
    private bool canMoveContinuously = false;

    void Awake()
    {
        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        // Permitir al agente usar la NavMesh completa (incluyendo NavMeshLink)
        agent.updatePosition = true;
        agent.updateRotation = false;  // controlamos rotación manualmente
        agent.speed = moveSpeed;
    }

    void OnEnable()
    {
        // Empezar movimiento continuo inmediatamente
        canMoveContinuously = true;
    }

    void OnDisable()
    {
        canMoveContinuously = false;
    }

    void Update()
    {
        if (abilityReceiver == null || abilityReceiver.CurrentTarget == null || !canMoveContinuously) return;

        // Rotación manual hacia el jugador
        Vector3 dir = (abilityReceiver.CurrentTarget.position - transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

        // Solo llamar SetDestination si el agente está en la NavMesh
        if (agent.isOnNavMesh)
        {
            agent.SetDestination(abilityReceiver.CurrentTarget.position);
            // Sin zigzag: posicionarse directamente en agent.nextPosition
            transform.position = agent.nextPosition;
        }
    }
}








//using UnityEngine;
//using UnityEngine.AI;

//public class EnemigoExplosion : MonoBehaviour
//{
//    [Header("Movimiento Continuo")]
//    public float moveSpeed = 3f;
//    public float rotationSpeed = 180f;

//    [Header("Referencias")]
//    public GameObject toggleObject;

//    private Transform playerTransform;
//    private EnemyAbilityReceiver abilityReceiver;
//    private NavMeshAgent agent;
//    private bool canMoveContinuously = false;

//    void Awake()
//    {
//        playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        agent = GetComponent<NavMeshAgent>();

//        // Permitir al agente usar la NavMesh completa (incluyendo NavMeshLink)
//        agent.updatePosition = true;
//        agent.updateRotation = false;  // controlamos rotación manualmente
//        agent.speed = moveSpeed;
//    }

//    void OnEnable()
//    {
//        // Empezar movimiento continuo inmediatamente
//        canMoveContinuously = true;
//    }

//    void OnDisable()
//    {
//        canMoveContinuously = false;
//    }

//    void Update()
//    {
//        if (playerTransform == null || !canMoveContinuously)
//            return;

//        // Rotación manual hacia el jugador
//        Vector3 dir = (playerTransform.position - transform.position).normalized;
//        Quaternion targetRot = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);

//        // Dejar que el agente avance por la NavMesh (incluyendo NavMeshLink)
//        agent.SetDestination(playerTransform.position);

//        // Sin zigzag: posicionarse directamente en agent.nextPosition
//        transform.position = agent.nextPosition;
//    }
//}









