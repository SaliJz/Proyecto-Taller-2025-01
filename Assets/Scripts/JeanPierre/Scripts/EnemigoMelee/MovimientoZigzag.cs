using UnityEngine;

public class MovimientoZigzag : MonoBehaviour
{
    [Header("Referencia al Jugador (se buscará por tag 'Player')")]
    public Transform playerTransform;

    [Header("Parámetros de Movimiento")]
    // Velocidad inicial del objeto.
    public float velocidadInicial = 5f;
    // Límite máximo de velocidad.
    public float velocidadMaxima = 10f;
    // Aceleración a lo largo del tiempo.
    public float aceleracion = 0.1f;
    // Amplitud máxima del movimiento lateral (zigzag).
    public float amplitud = 2f;
    // Escala de tiempo para el cambio de la oscilación (cuanto mayor, más lento).
    public float escalaRuido = 1f;
    // Semilla para variar el Perlin Noise (opcional).
    public float semillaRuido = 0f;
    // Distancia mínima para detener el movimiento.
    public float distanciaMinima = 1f;

    // Velocidad actual acumulada.
    private float velocidadActual;

    private EnemyAbilityReceiver abilityReceiver;

    Animator animator;


    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        if (abilityReceiver == null)
        {
            abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        }
        else
        {
            Debug.LogWarning("No se encontró el componente EnemyAbilityReceiver en " + gameObject.name);
        }

        velocidadActual = velocidadInicial;

        // Se busca el objeto Player utilizando el tag "Player" si no se asignó manualmente.
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
            else
            {
                Debug.LogError("No se encontró ningún objeto con el tag 'Player'.");
            }
        }
    }

    void Update()
    {
        if (playerTransform == null)
            return;

        float velocidad = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadActual;

        // Calcula la distancia entre este objeto y el jugador.
        float distanciaAlPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // Siempre se ajusta la rotación para que el frente (local forward) mire al jugador.
        Vector3 direccionMirada = (playerTransform.position - transform.position).normalized;
        // Para mantener el giro solo en el plano horizontal, descomenta la siguiente línea:
        // direccionMirada.y = 0f;
        if (direccionMirada.sqrMagnitude > 0.001f)
        {
            transform.rotation = Quaternion.LookRotation(direccionMirada);
        }

        // Si la distancia es mayor que la mínima, se ejecuta el movimiento.
        if (distanciaAlPlayer > distanciaMinima)
        {
            // Calcula la dirección base hacia el jugador.
            Vector3 direccionBase = (playerTransform.position - transform.position).normalized;
            // Se calcula una dirección lateral (perpendicular) usando Vector3.up.
            Vector3 direccionPerpendicular = Vector3.Cross(direccionBase, Vector3.up).normalized;

            // Se genera un factor lateral suave e irregular con Perlin Noise (valor entre -1 y 1).
            float ruido = Mathf.PerlinNoise((Time.time + semillaRuido) * escalaRuido, 0f);
            float factorZigzag = (ruido - 0.5f) * 2f;

            // Combina la dirección base y la componente lateral para formar la dirección final.
            Vector3 direccionFinal = (direccionBase + direccionPerpendicular * factorZigzag * amplitud).normalized;

            // Aumenta la velocidad según la aceleración.
            velocidadActual += aceleracion * Time.deltaTime;
            // Limita la velocidad para que no la supere el máximo.
            velocidadActual = Mathf.Min(velocidadActual, velocidadMaxima);

            // Mueve el objeto en la dirección final calculada.
            transform.position += direccionFinal * velocidad * Time.deltaTime;
            if (animator != null) animator.SetBool("isMoving", true);
        }
        else
        {
            // Si está lo suficientemente cerca del jugador, se queda quieto.
            // Se reinicia la velocidad para que, si el jugador se aleja, comience de nuevo desde la velocidadInicial.
            velocidadActual = velocidadInicial;
        }
    }
}
