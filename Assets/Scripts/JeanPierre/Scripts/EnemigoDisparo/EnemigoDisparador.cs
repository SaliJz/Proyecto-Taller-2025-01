using System.Collections;
using UnityEngine;

public class EnemigoDisparador : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    public Transform jugador;

    [Header("Disparo de bala")]
    public GameObject balaPrefab;
    [Tooltip("Transform desde donde se instancia y sigue la bala hasta el disparo")]
    public Transform puntoDisparo;
    public float velocidadBala = 10f;
    public float distanciaDisparo = 10f;

    [Header("Efecto de disparo")]
    [Tooltip("Prefab del efecto que se crea al disparar (muzzle flash, etc.)")]
    public GameObject efectoDisparoPrefab;

    [Header("Tiempo de carga de la bala")]
    public float tiempoCargaBala = 1f;

    [Header("Intervalo entre disparos")]
    public float intervaloDisparo = 3f;
    private float temporizadorDisparo = 0f;

    private Animator animator;

    private void Awake()
    {
        // Busca el jugador por tag si no está asignado
        if (jugador == null)
        {
            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
            if (jugadorGO)
                jugador = jugadorGO.transform;
            else
                Debug.LogWarning($"No se encontró Player en {name}");
        }

        animator = GetComponentInChildren<Animator>();
    }

    private void Update()
    {
        if (jugador == null) return;

        temporizadorDisparo += Time.deltaTime;

        // Comprueba intervalo y distancia
        if (temporizadorDisparo >= intervaloDisparo &&
            Vector3.Distance(transform.position, jugador.position) <= distanciaDisparo)
        {
            StartCoroutine(SpawnChargeAndShoot());
            temporizadorDisparo = 0f;
        }
    }

    private IEnumerator SpawnChargeAndShoot()
    {
        if (balaPrefab == null || puntoDisparo == null)
            yield break;

        // 0) Instanciar el efecto de disparo como hijo para que siga al puntoDisparo
        GameObject efecto = null;
        if (efectoDisparoPrefab != null)
        {
            efecto = Instantiate(
                efectoDisparoPrefab,
                puntoDisparo.position,
                puntoDisparo.rotation,
                puntoDisparo
            );
        }

        // 1) Instanciar la bala en el puntoDisparo como hija para que siga su movimiento
        GameObject bala = Instantiate(
            balaPrefab,
            puntoDisparo.position,
            puntoDisparo.rotation,
            puntoDisparo
        );

        // 2) Configurar el script de la bala
        var balaScript = bala.GetComponent<BalaEnemigoVolador>();
        if (balaScript != null)
        {
            balaScript.jugador = jugador;
            balaScript.velocidad = velocidadBala;
            balaScript.tiempoCarga = tiempoCargaBala;
        }

        // 3) Disparar animación de carga, si existe
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetTrigger("Disparar");
        }

        // 4) Esperar el tiempo de carga antes de “liberar” bala y efecto
        yield return new WaitForSeconds(tiempoCargaBala);

        // 5) Desprender (unparent) la bala sólo si aún existe
        if (bala != null)
            bala.transform.SetParent(null);

        // 6) Desprender el efecto sólo si aún existe
        if (efecto != null)
            efecto.transform.SetParent(null);

        // (Opcional) Si no se destruye solo: Destroy(efecto, 2f);
    }
}











//using System.Collections;
//using UnityEngine;

//public class EnemigoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Disparo de bala")]
//    public GameObject balaPrefab;
//    [Tooltip("Transform desde donde se instancia y sigue la bala hasta el disparo")]
//    public Transform puntoDisparo;
//    public float velocidadBala = 10f;
//    public float distanciaDisparo = 10f;

//    [Header("Tiempo de carga de la bala")]
//    public float tiempoCargaBala = 1f;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    private Animator animator;

//    private void Awake()
//    {
//        // Busca el jugador por tag si no está asignado
//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO)
//                jugador = jugadorGO.transform;
//            else
//                Debug.LogWarning($"No se encontró Player en {name}");
//        }

//        animator = GetComponentInChildren<Animator>();
//    }

//    private void Update()
//    {
//        if (jugador == null) return;

//        temporizadorDisparo += Time.deltaTime;

//        // Comprueba intervalo y distancia
//        if (temporizadorDisparo >= intervaloDisparo &&
//            Vector3.Distance(transform.position, jugador.position) <= distanciaDisparo)
//        {
//            StartCoroutine(SpawnChargeAndShoot());
//            temporizadorDisparo = 0f;
//        }
//    }

//    private IEnumerator SpawnChargeAndShoot()
//    {
//        if (balaPrefab == null || puntoDisparo == null)
//            yield break;

//        // 1) Instanciar la bala en el puntoDisparo como hija para que siga su movimiento
//        GameObject bala = Instantiate(
//            balaPrefab,
//            puntoDisparo.position,
//            puntoDisparo.rotation,
//            puntoDisparo
//        );

//        // 2) Configurar el script de la bala
//        var balaScript = bala.GetComponent<BalaEnemigoVolador>();
//        if (balaScript != null)
//        {
//            balaScript.jugador = jugador;
//            balaScript.velocidad = velocidadBala;
//            balaScript.tiempoCarga = tiempoCargaBala;
//        }

//        // 3) Disparar animación de carga, si existe
//        if (animator != null)
//        {
//            animator.SetBool("isMoving", false);
//            animator.SetTrigger("Disparar");
//        }

//        // 4) Esperar a que la bala termine de escalar (tiempo de carga)
//        //    y dentro de su propio CargarYDisparar() se desparenta y dispara
//        yield return new WaitForSeconds(tiempoCargaBala);
//    }
//}


























