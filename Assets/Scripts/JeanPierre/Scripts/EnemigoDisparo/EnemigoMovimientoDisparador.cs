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

    [Header("Circulaci�n alrededor del jugador")]
    public float velocidadCirculo = 2f;
    public bool sentidoHorario = true;

    [Header("Disparo")]
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float cooldownDisparo = 1.5f;
    public float velocidadBala = 10f;

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

        // Permitir cruce autom�tico de OffMeshLinks sin detenerse bruscamente
        agent.autoTraverseOffMeshLink = true;
        agent.autoBraking = false;

        if (jugador == null)
        {
            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO != null) jugador = jugadorGO.transform;
            else Debug.LogWarning($"No se encontr� Player en {name}");
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

        // Si est� cruzando un OffMeshLink, dejar que NavMeshAgent lo maneje
        if (agent.isOnOffMeshLink)
            return;

        // Ajustar velocidad seg�n ralentizaciones, hackeos, etc.
        agent.speed = abilityReceiver.CurrentSpeed;

        float dist = Vector3.Distance(transform.position, jugador.position);

        // Si est� lejos, acercarse
        if (dist > distanciaDeseada + margen)
        {
            agent.stoppingDistance = distanciaDeseada;
            agent.SetDestination(jugador.position);
        }
        // Si est� demasiado cerca, retroceder
        else if (dist < distanciaDeseada - margen)
        {
            Vector3 dirOp = (transform.position - jugador.position).normalized;
            agent.stoppingDistance = 0f;
            agent.SetDestination(transform.position + dirOp * (distanciaDeseada + 0.1f));
        }
        // Dentro de rango, circular
        else
        {
            agent.ResetPath();
            CirculaAlrededor();
        }

        RotacionYAnimacion();
        ManejaDisparo(dist);
    }

    private void CirculaAlrededor()
    {
        Vector3 delta = jugador.position - transform.position;
        Vector3 perp = sentidoHorario
            ? Quaternion.Euler(0, 90f, 0) * delta.normalized
            : Quaternion.Euler(0, -90f, 0) * delta.normalized;
        transform.position += perp * velocidadCirculo * Time.deltaTime;
    }

    private void RotacionYAnimacion()
    {
        Vector3 flatVel = agent.velocity;
        flatVel.y = 0f;
        if (flatVel.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(flatVel.normalized, Vector3.up);

        if (animator != null)
            animator.SetBool("isMoving", agent.hasPath);
    }

    private void ManejaDisparo(float dist)
    {
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

        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        if (bala.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 dir = (jugador.position - puntoDisparo.position).normalized;
            rb.linearVelocity = dir * velocidadBala;
        }
    }

    /// <summary>
    /// Invierte el sentido de circulaci�n alrededor del jugador.
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

//    [Header("Circulaci�n alrededor del jugador")]
//    public float velocidadCirculo = 2f;
//    public bool sentidoHorario = true;

//    [Header("Disparo")]
//    public GameObject balaPrefab;               // Prefab de la bala
//    public Transform puntoDisparo;              // Punto exacto donde nace la bala
//    public float cooldownDisparo = 1.5f;        // Tiempo entre disparos
//    public float velocidadBala = 10f;           // Velocidad de la bala

//    private EnemyAbilityReceiver abilityReceiver;
//    private NavMeshAgent agent;
//    private Animator animator;
//    private float distanciaDeseada;
//    private float timerDisparo;

//    private void Awake()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        agent = GetComponent<NavMeshAgent>();
//        animator = GetComponentInChildren<Animator>();

//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null) jugador = jugadorGO.transform;
//            else Debug.LogWarning($"No se encontr� Player en {name}");
//        }

//        distanciaDeseada = Random.Range(distanciaMinima, distanciaMaxima);
//        agent.stoppingDistance = distanciaDeseada;
//        agent.updateRotation = false;
//        agent.updateUpAxis = true;

//        timerDisparo = cooldownDisparo;
//    }

//    private void Start()
//    {
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

//        // Movimiento
//        agent.speed = abilityReceiver.CurrentSpeed;
//        Vector3 delta = jugador.position - transform.position;
//        delta.y = 0f;
//        float dist = delta.magnitude;
//        bool estaMoviendose = false;

//        if (!agent.isOnNavMesh)
//        {
//            Start();
//        }
//        else if (dist > distanciaDeseada + margen)
//        {
//            agent.stoppingDistance = distanciaDeseada;
//            agent.SetDestination(jugador.position);
//            estaMoviendose = true;
//        }
//        else if (dist < distanciaDeseada - margen)
//        {
//            Vector3 dirOp = -delta.normalized;
//            Vector3 target = transform.position + dirOp * (distanciaDeseada + 0.1f);
//            agent.stoppingDistance = 0f;
//            agent.SetDestination(target);
//            estaMoviendose = true;
//        }
//        else
//        {
//            agent.ResetPath();
//            Vector3 moveDir = delta.normalized;
//            Vector3 perp = sentidoHorario
//                ? Quaternion.Euler(0, 90f, 0) * moveDir
//                : Quaternion.Euler(0, -90f, 0) * moveDir;
//            transform.position += perp.normalized * velocidadCirculo * Time.deltaTime;
//            estaMoviendose = true;
//        }

//        // Rotaci�n
//        Vector3 flatVel = agent.velocity;
//        flatVel.y = 0f;
//        if (flatVel.sqrMagnitude > 0.01f)
//            transform.rotation = Quaternion.LookRotation(flatVel.normalized, Vector3.up);

//        // Animaci�n
//        if (animator != null)
//            animator.SetBool("isMoving", estaMoviendose);

//        // Disparo
//        timerDisparo -= Time.deltaTime;
//        if (timerDisparo <= 0f && dist <= distanciaMaxima + 1f)
//        {
//            Disparar();
//            timerDisparo = cooldownDisparo;
//        }
//    }

//    private void Disparar()
//    {
//        if (balaPrefab == null || puntoDisparo == null || jugador == null) return;

//        // Instancia la bala exactamente en puntoDisparo, sin offsets verticales
//        GameObject bala = Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
//        if (bala.TryGetComponent<Rigidbody>(out var rb))
//        {
//            Vector3 dir = (jugador.position - puntoDisparo.position).normalized;
//            rb.velocity = dir * velocidadBala;
//        }
//    }

//    /// <summary>
//    /// Invierte el sentido de circulaci�n alrededor del jugador.
//    /// </summary>
//    public void InvertirSentido()
//    {
//        sentidoHorario = !sentidoHorario;
//    }
//}

