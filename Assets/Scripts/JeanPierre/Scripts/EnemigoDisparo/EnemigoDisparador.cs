// EnemigoDisparador.cs
using UnityEngine;

public class EnemigoDisparador : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    public Transform jugador;

    [Header("Disparo de bala")]
    public float distanciaDisparo = 10f;
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float velocidadBala = 10f;

    [Header("Intervalo entre disparos")]
    public float intervaloDisparo = 3f;
    private float temporizadorDisparo = 0f;

    Animator animator;

    private void Awake()
    {
        if (jugador == null)
        {
            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO) jugador = jugadorGO.transform;
            else Debug.LogWarning($"No se encontró Player en {name}");
        }
        animator = GetComponentInChildren<Animator>();

    }

    private void Update()
    {
        if (jugador == null) return;
        
        temporizadorDisparo += Time.deltaTime;
        if (temporizadorDisparo >= intervaloDisparo &&
            Vector3.Distance(transform.position, jugador.position) <= distanciaDisparo)
        {
            DispararBala();
            temporizadorDisparo = 0f;
        }
    }

    private void DispararBala()
    {
        if (balaPrefab == null) return;
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Disparar");
        }
        // 1) Instancia la bala COMO HIJA de puntoDisparo
        Vector3 spawnPos = puntoDisparo.position;
        GameObject bala = Instantiate(balaPrefab, spawnPos, puntoDisparo.rotation, puntoDisparo);

        // 2) Pasa la referencia al jugador y la velocidad
        var balaScript = bala.GetComponent<BalaEnemigoVolador>();
        if (balaScript != null)
        {
            balaScript.jugador = jugador;
            balaScript.velocidad = velocidadBala;
            balaScript.tiempoCarga = balaScript.tiempoCarga;   // usa su propio valor por defecto
        }
    }
}


//// EnemigoDisparador.cs
//using UnityEngine;

//public class EnemigoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Disparo de bala")]
//    public float distanciaDisparo = 10f;
//    public GameObject balaPrefab;
//    public Transform puntoDisparo;
//    public float velocidadBala = 10f;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    private void Awake()
//    {
//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null)
//                jugador = jugadorGO.transform;
//            else
//                Debug.LogWarning($"No se encontró ningún GameObject con tag 'Player' en {name}");
//        }
//    }

//    private void Update()
//    {
//        if (jugador == null) return;

//        temporizadorDisparo += Time.deltaTime;
//        float dist = Vector3.Distance(transform.position, jugador.position);

//        if (temporizadorDisparo >= intervaloDisparo)
//        {
//            if (dist <= distanciaDisparo)
//                DispararBala();

//            temporizadorDisparo = 0f;
//        }
//    }

//    private void DispararBala()
//    {
//        if (balaPrefab == null)
//        {
//            Debug.LogWarning($"No se ha asignado el prefab de la bala en {name}");
//            return;
//        }

//        // Instancia la bala en la posición y rotación de 'puntoDisparo'
//        Vector3 spawnPos = puntoDisparo ? puntoDisparo.position : transform.position;
//        Quaternion spawnRot = puntoDisparo ? puntoDisparo.rotation : Quaternion.identity;
//        GameObject bala = Instantiate(balaPrefab, spawnPos, spawnRot);

//        var balaScript = bala.GetComponent<BalaEnemigoVolador>();
//        if (balaScript != null)
//        {
//            balaScript.direccion = (jugador.position - spawnPos).normalized;
//            balaScript.velocidad = velocidadBala;
//            balaScript.spawnTransform = puntoDisparo;
//        }
//        else
//        {
//            Debug.LogWarning($"El prefab de bala no tiene el componente 'BalaEnemigoVolador'");
//        }
//    }
//}












//using UnityEngine;

//public class EnemigoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Movimiento relativo al jugador")]
//    public float distanciaDeseada = 5f;
//    public float velocidadMovimiento = 3f;
//    public float margen = 0.5f; // Zona de tolerancia para movimiento fluido

//    [Header("Disparo de bala")]
//    public float distanciaDisparo = 10f;
//    public GameObject balaPrefab;
//    public Transform puntoDisparo;
//    public float velocidadBala = 10f;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    private EnemyAbilityReceiver abilityReceiver;

//    private void Awake()
//    {
//        if (abilityReceiver == null)
//        {
//            abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        }
//        else
//        {
//            Debug.LogWarning("No se encontró el componente EnemyAbilityReceiver en " + gameObject.name);
//        }

//        // Intentar asignar el jugador por tag si no se asignó en el Inspector
//        if (jugador == null)
//        {
//            GameObject jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO != null)
//            {
//                jugador = jugadorGO.transform;
//            }
//            else
//            {
//                Debug.LogWarning("No se encontró ningún GameObject con tag 'Player' para asignar 'jugador' en " + gameObject.name);
//            }
//        }
//    }

//    private void Update()
//    {
//        if (jugador == null)
//        {
//            Debug.LogWarning("El jugador no ha sido asignado en " + gameObject.name);
//            return;
//        }

//        // Obtener la velocidad del enemigo desde el componente EnemyAbilityReceiver
//        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadMovimiento;

//        // Dirección y distancia horizontal hacia el jugador (ignorando el eje Y)
//        Vector3 direccionHaciaJugador = jugador.position - transform.position;
//        direccionHaciaJugador.y = 0f;
//        float distanciaAlJugador = direccionHaciaJugador.magnitude;

//        Vector3 direccionMovimiento = direccionHaciaJugador.normalized;

//        if (distanciaAlJugador > distanciaDeseada + margen)
//        {
//            // Acércate al jugador
//            Vector3 nuevaPos = Vector3.MoveTowards(transform.position,
//                                                   transform.position + direccionMovimiento,
//                                                   speed * Time.deltaTime);
//            transform.position = nuevaPos;
//        }
//        else if (distanciaAlJugador < distanciaDeseada - margen)
//        {
//            // Aléjate del jugador
//            Vector3 nuevaPos = Vector3.MoveTowards(transform.position,
//                                                   transform.position - direccionMovimiento,
//                                                   speed * Time.deltaTime);
//            transform.position = nuevaPos;
//        }

//        // Rotación para mirar al jugador
//        Vector3 direccionRotacion = jugador.position - transform.position;
//        direccionRotacion.y = 0f;
//        if (direccionRotacion != Vector3.zero)
//        {
//            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionRotacion);
//            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, speed * Time.deltaTime);
//        }

//        // Control de disparos
//        temporizadorDisparo += Time.deltaTime;
//        if (temporizadorDisparo >= intervaloDisparo)
//        {
//            if (distanciaAlJugador <= distanciaDisparo)
//            {
//                DispararBala();
//            }
//            temporizadorDisparo = 0f;
//        }
//    }

//    private void DispararBala()
//    {
//        if (balaPrefab != null)
//        {
//            Vector3 spawnPosition = (puntoDisparo != null) ? puntoDisparo.position : transform.position;
//            GameObject balaInstanciada = Instantiate(balaPrefab, spawnPosition, Quaternion.identity);

//            Vector3 direccionDisparo = (jugador.position - spawnPosition).normalized;

//            BalaEnemigoVolador balaScript = balaInstanciada.GetComponent<BalaEnemigoVolador>();
//            if (balaScript != null)
//            {
//                balaScript.direccion = direccionDisparo;
//                balaScript.tiempoCarga = 1.0f;
//                balaScript.escalaFinal = new Vector3(1f, 1f, 1f);
//                balaScript.velocidad = velocidadBala;
//                balaScript.colorFinal = Color.red;
//                balaScript.blinkSpeed = 2f;
//                balaScript.spawnTransform = puntoDisparo;
//            }
//            else
//            {
//                Debug.LogWarning("El prefab de bala no tiene el componente 'BalaEnemigoVolador'");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("No se ha asignado el prefab de la bala en " + gameObject.name);
//        }
//    }
//}