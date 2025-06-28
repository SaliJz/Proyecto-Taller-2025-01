using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovimientoZigzagNavMesh : MonoBehaviour
{
    [Header("Referencia al Jugador (tag 'Player')")]
    public Transform playerTransform;

    [Header("Par�metros de Movimiento")]
    public float velocidadInicial = 5f;
    public float velocidadMaxima = 10f;
    public float aceleracion = 0.1f;
    public float amplitud = 2f;
    public float escalaRuido = 1f;
    public float semillaRuido = 0f;
    public float distanciaMinima = 1f;

    private float velocidadActual;
    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;

    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        // Mantenemos control autom�tico de posici�n, pero gestionamos rotaci�n a mano
        agent.updatePosition = true;
        agent.updateRotation = false;

        velocidadActual = velocidadInicial;

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else Debug.LogError("No se encontr� objeto con tag 'Player'.");
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Usamos siempre nuestra velocidadActual (con aceleraci�n) en lugar de override que daba cero
        float velocidad = velocidadActual;

        float distancia = Vector3.Distance(transform.position, playerTransform.position);

        // Rotaci�n manual s�lo en Y
        Vector3 dirLook = (playerTransform.position - transform.position);
        dirLook.y = 0f;
        if (dirLook.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dirLook.normalized);

        if (distancia > distanciaMinima)
        {
            // Zigzag: direcci�n base y perpendicular
            Vector3 dirBase = (playerTransform.position - transform.position);
            dirBase.y = 0f;
            dirBase.Normalize();
            Vector3 dirPerp = Vector3.Cross(dirBase, Vector3.up).normalized;

            // Perlin Noise entre -1 y 1
            float ruido = Mathf.PerlinNoise((Time.time + semillaRuido) * escalaRuido, 0f);
            float zig = (ruido - 0.5f) * 2f;

            Vector3 dirFinal = (dirBase + dirPerp * zig * amplitud).normalized;

            // Acelera y limita
            velocidadActual = Mathf.Min(velocidadActual + aceleracion * Time.deltaTime, velocidadMaxima);

            // Mueve con NavMeshAgent para respetar la malla
            agent.Move(dirFinal * velocidadActual * Time.deltaTime);

        }
        else
        {
            // Cerca: reinicia velocidad y det�n animaci�n
            velocidadActual = velocidadInicial;
        }
    }
}


//using UnityEngine;

//public class MovimientoZigzag : MonoBehaviour
//{
//    [Header("Referencia al Jugador (se buscar� por tag 'Player')")]
//    public Transform playerTransform;

//    [Header("Par�metros de Movimiento")]
//    // Velocidad inicial del objeto.
//    public float velocidadInicial = 5f;
//    // L�mite m�ximo de velocidad.
//    public float velocidadMaxima = 10f;
//    // Aceleraci�n a lo largo del tiempo.
//    public float aceleracion = 0.1f;
//    // Amplitud m�xima del movimiento lateral (zigzag).
//    public float amplitud = 2f;
//    // Escala de tiempo para el cambio de la oscilaci�n (cuanto mayor, m�s lento).
//    public float escalaRuido = 1f;
//    // Semilla para variar el Perlin Noise (opcional).
//    public float semillaRuido = 0f;
//    // Distancia m�nima para detener el movimiento.
//    public float distanciaMinima = 1f;

//    // Velocidad actual acumulada.
//    private float velocidadActual;

//    private EnemyAbilityReceiver abilityReceiver;

//    Animator animator;


//    void Start()
//    {
//        animator = GetComponentInChildren<Animator>();

//        if (abilityReceiver == null)
//        {
//            abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        }
//        else
//        {
//            Debug.LogWarning("No se encontr� el componente EnemyAbilityReceiver en " + gameObject.name);
//        }

//        velocidadActual = velocidadInicial;

//        // Se busca el objeto Player utilizando el tag "Player" si no se asign� manualmente.
//        if (playerTransform == null)
//        {
//            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//            if (playerObj != null)
//            {
//                playerTransform = playerObj.transform;
//            }
//            else
//            {
//                Debug.LogError("No se encontr� ning�n objeto con el tag 'Player'.");
//            }
//        }
//    }

//    void Update()
//    {
//        if (playerTransform == null)
//            return;

//        float velocidad = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadActual;

//        // Calcula la distancia entre este objeto y el jugador.
//        float distanciaAlPlayer = Vector3.Distance(transform.position, playerTransform.position);

//        // Siempre se ajusta la rotaci�n para que el frente (local forward) mire al jugador.
//        Vector3 direccionMirada = (playerTransform.position - transform.position).normalized;
//        // Para mantener el giro solo en el plano horizontal, descomenta la siguiente l�nea:
//        // direccionMirada.y = 0f;
//        if (direccionMirada.sqrMagnitude > 0.001f)
//        {
//            transform.rotation = Quaternion.LookRotation(direccionMirada);
//        }

//        // Si la distancia es mayor que la m�nima, se ejecuta el movimiento.
//        if (distanciaAlPlayer > distanciaMinima)
//        {
//            // Calcula la direcci�n base hacia el jugador.
//            Vector3 direccionBase = (playerTransform.position - transform.position).normalized;
//            // Se calcula una direcci�n lateral (perpendicular) usando Vector3.up.
//            Vector3 direccionPerpendicular = Vector3.Cross(direccionBase, Vector3.up).normalized;

//            // Se genera un factor lateral suave e irregular con Perlin Noise (valor entre -1 y 1).
//            float ruido = Mathf.PerlinNoise((Time.time + semillaRuido) * escalaRuido, 0f);
//            float factorZigzag = (ruido - 0.5f) * 2f;

//            // Combina la direcci�n base y la componente lateral para formar la direcci�n final.
//            Vector3 direccionFinal = (direccionBase + direccionPerpendicular * factorZigzag * amplitud).normalized;

//            // Aumenta la velocidad seg�n la aceleraci�n.
//            velocidadActual += aceleracion * Time.deltaTime;
//            // Limita la velocidad para que no la supere el m�ximo.
//            velocidadActual = Mathf.Min(velocidadActual, velocidadMaxima);

//            // Mueve el objeto en la direcci�n final calculada.
//            transform.position += direccionFinal * velocidad * Time.deltaTime;
//            if (animator != null) animator.SetBool("isMoving", true);
//        }
//        else
//        {
//            // Si est� lo suficientemente cerca del jugador, se queda quieto.
//            // Se reinicia la velocidad para que, si el jugador se aleja, comience de nuevo desde la velocidadInicial.
//            velocidadActual = velocidadInicial;
//        }
//    }
//}
