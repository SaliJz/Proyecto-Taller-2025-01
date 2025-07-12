using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovementAndAttackWithNavMeshController : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Transform del jugador")]
    public Transform playerTransform;

    [Tooltip("Rango para iniciar la animación de ataque")]
    public float attackRange = 2f;

    [Tooltip("Daño que se aplica al chocar con el jugador")]
    public int damageOnCollision = 10;

    [Tooltip("Tiempo entre ataques")]
    public float attackCooldown = 1f;

    [Header("Componentes")]
    private NavMeshAgent agent;
    private Animator animator;

    private float lastAttackTime;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null)
                playerTransform = p.transform;
        }

        lastAttackTime = -attackCooldown;
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > attackRange)
        {
            // Persigue al jugador usando NavMeshAgent
            if (agent.isStopped)
                agent.isStopped = false;

            agent.SetDestination(playerTransform.position);

            // Podrías añadir: animator.SetBool("isMoving", true);
        }
        else
        {
            // Detén al agente y lanza el trigger de ataque si ha pasado el cooldown
            if (!agent.isStopped)
                agent.isStopped = true;

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                animator.SetTrigger("Attack");
                lastAttackTime = Time.time;
            }
        }
    }

    // Este método asume que PlayerHealth.TakeDamage ahora requiere (int damage, Vector3 attackerPosition)
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            var health = collision.gameObject.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // Pasamos la posición de este enemigo como attackerPosition
                health.TakeDamage(damageOnCollision, transform.position);
            }
            else
            {
                Debug.LogWarning("PlayerHealth no encontrado en el jugador.");
            }
        }
    }
}









//using System.Collections;
//using UnityEngine;

//public class MovimientoConAtaque : MonoBehaviour
//{
//    public Transform playerTransform;

//    public float velocidadPersecucion = 5f;
//    public float velocidadAtaque = 10f;
//    public float velocidadRetirada = 5f;

//    public float distanciaAtaqueMin = 2f;
//    public float distanciaAtaqueMax = 5f;
//    private float distanciaAtaqueActual;

//    public int minAtaques = 3;
//    public int maxAtaques = 5;

//    public GameObject prefabColiderAtaque;
//    public float duracionColider = 0.5f;
//    public float tiempoEntreColiders = 0.3f;

//    public float tiempoRetirada = 1f;
//    public float velocidadRotacion = 5f;

//    enum Estado { Persecucion, Ataque, Retirada }
//    Estado estadoActual = Estado.Persecucion;

//    Vector3 posicionObjetivoAtaque;
//    GameObject coliderActual;

//    Animator animator;

//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        animator = GetComponentInChildren<Animator>();

//        if (playerTransform == null)
//        {
//            GameObject p = GameObject.FindGameObjectWithTag("Player");
//            if (p != null)
//                playerTransform = p.transform;
//        }

//        AsignarNuevaDistanciaAtaque();
//    }

//    void Update()
//    {
//        if (playerTransform == null) return;

//        RotarHaciaJugador();

//        switch (estadoActual)
//        {
//            case Estado.Persecucion:
//                Perseguir();
//                Debug.Log("Activando trigger Attack");

//                break;
//            case Estado.Ataque:

//                MoverAtaque();
//                break;
//        }
//    }

//    void RotarHaciaJugador()
//    {
//        Vector3 direccion = PosicionJugadorSinY() - transform.position;
//        if (direccion != Vector3.zero)
//        {
//            Quaternion rotacionDeseada = Quaternion.LookRotation(direccion);
//            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
//        }
//    }

//    Vector3 PosicionJugadorSinY()
//    {
//        return new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
//    }

//    void AsignarNuevaDistanciaAtaque()
//    {
//        distanciaAtaqueActual = Random.Range(distanciaAtaqueMin, distanciaAtaqueMax);
//    }

//    void Perseguir()
//    {
//        Vector3 posJugador = PosicionJugadorSinY();
//        Vector3 dir = (posJugador - transform.position).normalized;
//        float velocidadActual = abilityReceiver.CurrentSpeed;
//        transform.position += dir * velocidadPersecucion * Time.deltaTime;

//        if (Vector3.Distance(transform.position, posJugador) <= distanciaAtaqueActual)
//        {
//            posicionObjetivoAtaque = posJugador;
//            estadoActual = Estado.Ataque;
//            if (animator != null)
//            {

//                animator.SetTrigger("Attack");
//            }
//        }
//    }

//    void MoverAtaque()
//    {
//        Vector3 delta = posicionObjetivoAtaque - transform.position;
//        if (delta.magnitude > 0.1f)
//        {
//            transform.position += delta.normalized * velocidadAtaque * Time.deltaTime;
//        }
//        else
//        {
//            estadoActual = Estado.Retirada;
//            StartCoroutine(SecuenciaAtaques());
//        }
//    }

//    IEnumerator SecuenciaAtaques()
//    {

//        int numAtaques = Random.Range(minAtaques, maxAtaques + 1);

//        for (int i = 0; i < numAtaques; i++)
//        {



//            posicionObjetivoAtaque = PosicionJugadorSinY();

//            while ((transform.position - posicionObjetivoAtaque).magnitude > 0.1f)
//            {
//                Vector3 dir = (posicionObjetivoAtaque - transform.position).normalized;
//                transform.position += dir * velocidadAtaque * Time.deltaTime;
//                yield return null;
//            }

//            CrearColiderEnObjetivo();
//            yield return new WaitForSeconds(duracionColider);
//            DestruirColiderActual();

//            yield return new WaitForSeconds(tiempoEntreColiders);
//            CrearColiderEnObjetivo();
//            yield return new WaitForSeconds(duracionColider);
//            DestruirColiderActual();

//            if (i < numAtaques - 1)
//                yield return new WaitForSeconds(tiempoEntreColiders);
//        }

//        Vector3 dirRetirada = -transform.forward;
//        float timer = 0f;
//        while (timer < tiempoRetirada)
//        {
//            transform.position += dirRetirada * velocidadRetirada * Time.deltaTime;
//            timer += Time.deltaTime;
//            yield return null;
//        }

//        AsignarNuevaDistanciaAtaque();
//        estadoActual = Estado.Persecucion;
//    }

//    void CrearColiderEnObjetivo()
//    {
//        coliderActual = Instantiate(prefabColiderAtaque, posicionObjetivoAtaque, transform.rotation);
//    }

//    void DestruirColiderActual()
//    {
//        if (coliderActual != null)
//        {
//            Destroy(coliderActual);
//            coliderActual = null;
//        }
//    }

//    // Este método se llama automáticamente cuando el GameObject que contiene este script es destruido.
//    void OnDestroy()
//    {
//        DestruirColiderActual();
//    }
//}



