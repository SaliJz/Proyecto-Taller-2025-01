using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovimientoDirectoNavMesh : MonoBehaviour
{
    [Header("Referencia al Jugador (tag 'Player')")]
    public Transform playerTransform;   

    [Header("Par�metros de Movimiento")]
    public float velocidadInicial = 5f;
    public float velocidadMaxima = 10f;
    public float aceleracion = 0.1f;
    public float distanciaMinima = 1f;

    private float velocidadActual;
    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;

    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        // Configuramos para que el NavMeshAgent maneje posici�n y rotaci�n,
        // de modo que pueda usar NavMeshLinks autom�ticamente
        agent.updatePosition = true;
        agent.updateRotation = true;

        velocidadActual = velocidadInicial;

        // Asegurar que el agente est� sobre la NavMesh
        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning("No se encontr� posici�n de NavMesh cercana.");
        }

        if (playerTransform == null)
        {
            var p = GameObject.Find("Player");
            if (p != null) playerTransform = p.transform;
            else Debug.LogError("No se encontr� objeto con tag 'Player'.");
        }
    }

    void Update()
    {
        velocidadActual = abilityReceiver.CurrentSpeed;

        if (abilityReceiver == null || abilityReceiver.CurrentTarget == null) return;
        if (!agent.enabled || !agent.isOnNavMesh) return;

        float distancia = Vector3.Distance(transform.position, abilityReceiver.CurrentTarget.position);

        if (distancia > distanciaMinima)
        {
            // Actualizamos velocidad con aceleraci�n y l�mite
            velocidadActual = Mathf.Min(velocidadActual + aceleracion * Time.deltaTime, velocidadMaxima);
            agent.speed = velocidadActual;

            // Indicamos la posici�n objetivo. El NavMeshAgent usar� NavMeshLink si es necesario.
            agent.SetDestination(abilityReceiver.CurrentTarget.position);
        }
        else
        {
            // Reiniciamos al estar cerca
            velocidadActual = velocidadInicial;
            agent.speed = velocidadActual;
            agent.ResetPath();
        }
    }
}












//using UnityEngine;
//using UnityEngine.AI;

//[RequireComponent(typeof(NavMeshAgent))]
//public class MovimientoZigzagNavMesh : MonoBehaviour
//{
//    [Header("Referencia al Jugador (tag 'Player')")]
//    public Transform playerTransform;

//    [Header("Par�metros de Movimiento")]
//    public float velocidadInicial = 5f;
//    public float velocidadMaxima = 10f;
//    public float aceleracion = 0.1f;
//    public float amplitud = 2f;
//    public float escalaRuido = 1f;
//    public float semillaRuido = 0f;
//    public float distanciaMinima = 1f;

//    private float velocidadActual;
//    private NavMeshAgent agent;
//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        agent = GetComponent<NavMeshAgent>();

//        // Asegurarnos de que el agente est� activo y sobre la NavMesh
//        agent.updatePosition = true;
//        agent.updateRotation = false;
//        velocidadActual = velocidadInicial;

//        // Si no est� sobre la NavMesh, mu�velo al punto m�s cercano
//        if (!agent.isOnNavMesh)
//        {
//            NavMeshHit hit;
//            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
//            {
//                agent.Warp(hit.position);
//            }
//            else
//            {
//                Debug.LogWarning("MovimientoZigzagNavMesh: no se encontr� posici�n de NavMesh cercana.");
//            }
//        }

//        if (playerTransform == null)
//        {
//            var p = GameObject.FindGameObjectWithTag("Player");
//            if (p != null) playerTransform = p.transform;
//            else Debug.LogError("No se encontr� objeto con tag 'Player'.");
//        }
//    }

//    void Update()
//    {
//        if (playerTransform == null) return;

//        // Si el agente est� deshabilitado o ya no est� en la NavMesh, salimos
//        if (!agent.enabled || !agent.isOnNavMesh)
//            return;

//        float distancia = Vector3.Distance(transform.position, playerTransform.position);

//        // Rotaci�n manual s�lo en Y
//        Vector3 dirLook = playerTransform.position - transform.position;
//        dirLook.y = 0f;
//        if (dirLook.sqrMagnitude > 0.001f)
//            transform.rotation = Quaternion.LookRotation(dirLook.normalized);

//        if (distancia > distanciaMinima)
//        {
//            // Zigzag: direcci�n base y perpendicular
//            Vector3 dirBase = (playerTransform.position - transform.position);
//            dirBase.y = 0f;
//            dirBase.Normalize();
//            Vector3 dirPerp = Vector3.Cross(dirBase, Vector3.up).normalized;

//            // Perlin Noise entre -1 y 1
//            float ruido = Mathf.PerlinNoise((Time.time + semillaRuido) * escalaRuido, 0f);
//            float zig = (ruido - 0.5f) * 2f;

//            Vector3 dirFinal = (dirBase + dirPerp * zig * amplitud).normalized;

//            // Acelera y limita
//            velocidadActual = Mathf.Min(velocidadActual + aceleracion * Time.deltaTime, velocidadMaxima);

//            // Mueve con NavMeshAgent para respetar la malla (s�lo si sigue habilitado y en NavMesh)
//            agent.Move(dirFinal * velocidadActual * Time.deltaTime);
//        }
//        else
//        {
//            // Cerca: reinicia velocidad
//            velocidadActual = velocidadInicial;
//        }
//    }
//}












