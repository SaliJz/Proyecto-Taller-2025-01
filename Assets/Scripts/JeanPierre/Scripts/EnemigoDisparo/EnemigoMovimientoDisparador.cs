using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(EnemyAbilityReceiver))]
[RequireComponent(typeof(NavMeshAgent))]
public class EnemigoMovimientoDisparador : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    public Transform jugador;

    [Header("Rango para elegir distancia deseada")]
    public float distanciaMinima = 3f;
    public float distanciaMaxima = 7f;
    [Header("Margen alrededor de la distancia deseada")]
    public float margen = 0.5f;

    [Header("Circulación alrededor del jugador")]
    public float velocidadCirculo = 2f;
    public bool sentidoHorario = true;

    [Header("Disparo")]
    public GameObject balaPrefab;               // Prefab de la bala
    public Transform puntoDisparo;              // Punto exacto donde nace la bala
    public float cooldownDisparo = 1.5f;        // Tiempo entre disparos
    public float velocidadBala = 10f;           // Velocidad de la bala

    private EnemyAbilityReceiver abilityReceiver;
    private NavMeshAgent agent;
    private Animator animator;
    private float distanciaDeseada;
    private float timerDisparo;

    private void Awake()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (jugador == null)
        {
            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO != null) jugador = jugadorGO.transform;
            else Debug.LogWarning($"No se encontró Player en {name}");
        }

        distanciaDeseada = Random.Range(distanciaMinima, distanciaMaxima);
        agent.stoppingDistance = distanciaDeseada;
        agent.updateRotation = false;
        agent.updateUpAxis = true;

        timerDisparo = cooldownDisparo;
    }

    private void Start()
    {
        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogError($"[{name}] No hay NavMesh cercano para posicionar al agente.");
        }
    }

    private void Update()
    {
        if (jugador == null) return;

        // Movimiento
        agent.speed = abilityReceiver.CurrentSpeed;
        Vector3 delta = jugador.position - transform.position;
        delta.y = 0f;
        float dist = delta.magnitude;
        bool estaMoviendose = false;

        if (!agent.isOnNavMesh)
        {
            Start();
        }
        else if (dist > distanciaDeseada + margen)
        {
            agent.stoppingDistance = distanciaDeseada;
            agent.SetDestination(jugador.position);
            estaMoviendose = true;
        }
        else if (dist < distanciaDeseada - margen)
        {
            Vector3 dirOp = -delta.normalized;
            Vector3 target = transform.position + dirOp * (distanciaDeseada + 0.1f);
            agent.stoppingDistance = 0f;
            agent.SetDestination(target);
            estaMoviendose = true;
        }
        else
        {
            agent.ResetPath();
            Vector3 moveDir = delta.normalized;
            Vector3 perp = sentidoHorario
                ? Quaternion.Euler(0, 90f, 0) * moveDir
                : Quaternion.Euler(0, -90f, 0) * moveDir;
            transform.position += perp.normalized * velocidadCirculo * Time.deltaTime;
            estaMoviendose = true;
        }

        // Rotación
        Vector3 flatVel = agent.velocity;
        flatVel.y = 0f;
        if (flatVel.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(flatVel.normalized, Vector3.up);

        // Animación
        if (animator != null)
            animator.SetBool("isMoving", estaMoviendose);

        // Disparo
        timerDisparo -= Time.deltaTime;
        if (timerDisparo <= 0f && dist <= distanciaMaxima + 1f)
        {
            Disparar();
            timerDisparo = cooldownDisparo;
        }
    }

    private void Disparar()
    {
        if (balaPrefab == null || puntoDisparo == null || jugador == null) return;

        // Instancia la bala exactamente en puntoDisparo, sin offsets verticales
        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        if (bala.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (jugador.position - puntoDisparo.position).normalized;
            rb.velocity = dir * velocidadBala;
        }
    }

    /// <summary>
    /// Invierte el sentido de circulación alrededor del jugador.
    /// </summary>
    public void InvertirSentido()
    {
        sentidoHorario = !sentidoHorario;
    }
}







//using UnityEngine;
//using UnityEngine.AI;

//[RequireComponent(typeof(EnemyAbilityReceiver))]
//[RequireComponent(typeof(NavMeshAgent))]
//public class EnemigoMovimientoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Rango para elegir distancia deseada")]
//    public float distanciaMinima = 3f;
//    public float distanciaMaxima = 7f;
//    [Header("Margen alrededor de la distancia deseada")]
//    public float margen = 0.5f;

//    [Header("Circulación alrededor del jugador")]
//    public float velocidadCirculo = 2f;
//    public bool sentidoHorario = true;

//    private EnemyAbilityReceiver abilityReceiver;
//    private NavMeshAgent agent;
//    private float distanciaDeseada;
//    private Animator animator;

//    private void Awake()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponentInChildren<Animator>();

//        // Encuentra jugador si no está asignado
//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null) jugador = jugadorGO.transform;
//            else Debug.LogWarning($"No se encontró Player en {name}");
//        }

//        // Distancia deseada aleatoria
//        distanciaDeseada = Random.Range(distanciaMinima, distanciaMaxima);

//        // Configuraciones del agente
//        agent.stoppingDistance = distanciaDeseada;
//        agent.updateRotation = false;  // Desactivamos rotación automática
//        agent.updateUpAxis = true;
//    }

//    private void Start()
//    {
//        // Asegura que el agente esté sobre la NavMesh
//        if (!agent.isOnNavMesh)
//        {
//            if (NavMesh.SamplePosition(transform.position, out var hit, 5f, NavMesh.AllAreas))
//                agent.Warp(hit.position);
//            else
//                Debug.LogError($"[{name}] No hay NavMesh cercano para posicionar al agente.");
//        }
//    }

//    private void Update()
//    {
//        if (jugador == null) return;

//        // Actualiza velocidad del agente según efectos de habilidad
//        agent.speed = abilityReceiver.CurrentSpeed;

//        Vector3 delta = jugador.position - transform.position;
//        delta.y = 0f;
//        float dist = delta.magnitude;
//        bool estaMoviendose = false;

//        if (agent.isOnNavMesh)
//        {
//            if (dist > distanciaDeseada + margen)
//            {
//                // Acercarse
//                agent.stoppingDistance = distanciaDeseada;
//                agent.SetDestination(jugador.position);
//                estaMoviendose = true;
//            }
//            else if (dist < distanciaDeseada - margen)
//            {
//                // Alejarse
//                Vector3 dirOp = delta.normalized * -1f;
//                Vector3 target = transform.position + dirOp * (distanciaDeseada + 0.1f);
//                agent.stoppingDistance = 0f;
//                agent.SetDestination(target);
//                estaMoviendose = true;
//            }
//            else
//            {
//                // Circular alrededor
//                agent.ResetPath();
//                Vector3 moveDir = delta.normalized;
//                Vector3 perp = sentidoHorario
//                    ? Quaternion.Euler(0, 90f, 0) * moveDir
//                    : Quaternion.Euler(0, -90f, 0) * moveDir;
//                transform.position += perp.normalized * velocidadCirculo * Time.deltaTime;
//                estaMoviendose = true;
//            }
//        }
//        else
//        {
//            // Reintenta posicionar si se salió de la malla
//            Start();
//        }

//        // ROTACIÓN SOLO EN Y para evitar inclinarse
//        Vector3 flatVel = agent.velocity;
//        flatVel.y = 0f;
//        if (flatVel.sqrMagnitude > 0.01f)
//        {
//            transform.rotation = Quaternion.LookRotation(flatVel.normalized, Vector3.up);
//        }

//        // Animación de movimiento
//        if (animator != null)
//            animator.SetBool("isMoving", estaMoviendose);
//    }

//    /// <summary>
//    /// Invierte el sentido de circulación alrededor del jugador.
//    /// </summary>
//    public void InvertirSentido()
//    {
//        sentidoHorario = !sentidoHorario;
//    }
//}
















//using UnityEngine;

//[RequireComponent(typeof(EnemyAbilityReceiver))]
//public class EnemigoMovimientoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Rango para elegir distancia deseada")]
//    public float distanciaMinima = 3f;
//    public float distanciaMaxima = 7f;
//    [Header("Margen alrededor de la distancia deseada")]
//    public float margen = 0.5f;

//    [Header("Movimiento relativo al jugador")]
//    public float velocidadMovimiento = 3f;

//    [Header("Circulación alrededor del jugador")]
//    public float velocidadCirculo = 2f;
//    public bool sentidoHorario = true;

//    private EnemyAbilityReceiver abilityReceiver;
//    private float distanciaDeseada;

//    Animator animator;

//    private void Awake()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        animator = GetComponentInChildren<Animator>();

//        // Busca jugador si no está asignado
//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null)
//                jugador = jugadorGO.transform;
//            else
//                Debug.LogWarning($"No se encontró ningún GameObject con tag 'Player' en {name}");
//        }

//        // Elige aleatoriamente la distancia deseada dentro del rango
//        distanciaDeseada = Random.Range(distanciaMinima, distanciaMaxima);
//    }

//    private void Update()
//    {
//        if (jugador == null) return;
//        bool estaMoviendose = false;

//        float speed = abilityReceiver.CurrentSpeed;
//        Vector3 dir = jugador.position - transform.position;
//        dir.y = 0f;
//        float dist = dir.magnitude;
//        Vector3 moveDir = dir.normalized;

//        if (dist > distanciaDeseada + margen)
//        {
//            // Se acerca
//            transform.position = Vector3.MoveTowards(
//                transform.position,
//                transform.position + moveDir,
//                speed * Time.deltaTime
//            );
//            estaMoviendose = true;
//        }
//        else if (dist < distanciaDeseada - margen)
//        {
//            // Se aleja
//            transform.position = Vector3.MoveTowards(
//                transform.position,
//                transform.position - moveDir,
//                speed * Time.deltaTime
//            );
//            estaMoviendose = true;
//        }
//        else
//        {
//            // Circula alrededor
//            Vector3 perp = sentidoHorario
//                ? Quaternion.Euler(0, 90f, 0) * moveDir
//                : Quaternion.Euler(0, -90f, 0) * moveDir;

//            transform.position += perp.normalized * velocidadCirculo * Time.deltaTime;
//            estaMoviendose = true;
//        }
//        if (animator != null)
//        {
//            animator.SetBool("isMoving", estaMoviendose);
//        }

//    }

//    /// <summary>
//    /// Invierte el sentido de circulación alrededor del jugador.
//    /// </summary>
//    public void InvertirSentido()
//    {
//        sentidoHorario = !sentidoHorario;
//    }
//}

