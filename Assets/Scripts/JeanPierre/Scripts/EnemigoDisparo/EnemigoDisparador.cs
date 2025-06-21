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

        // 4) Esperar a que la bala termine de escalar (tiempo de carga)
        //    y dentro de su propio CargarYDisparar() se desparenta y dispara
        yield return new WaitForSeconds(tiempoCargaBala);
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

//    [Header("Tiempo de carga de la bala")]
//    public float tiempoCargaBala = 1f;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    private Animator animator;

//    private void Awake()
//    {
//        // Si no se asignó el jugador en el Inspector, lo busca por tag
//        if (jugador == null)
//        {
//            var jugadorGO = GameObject.FindGameObjectWithTag("Player");
//            if (jugadorGO) jugador = jugadorGO.transform;
//            else Debug.LogWarning($"No se encontró Player en {name}");
//        }

//        // Cachea el Animator hijo si existe
//        animator = GetComponentInChildren<Animator>();
//    }

//    private void Update()
//    {
//        if (jugador == null) return;

//        temporizadorDisparo += Time.deltaTime;

//        // Dispara solo si ha pasado el intervalo y estamos dentro del rango
//        bool okTiempo = temporizadorDisparo >= intervaloDisparo;
//        bool enRango = Vector3.Distance(transform.position, jugador.position) <= distanciaDisparo;

//        if (okTiempo && enRango)
//        {
//            DispararBala();
//            temporizadorDisparo = 0f;
//        }
//    }

//    private void DispararBala()
//    {
//        if (balaPrefab == null || puntoDisparo == null) return;

//        // Lanza la animación de disparo si hay Animator
//        if (animator != null)
//        {
//            animator.SetBool("isMoving", false);
//            animator.SetTrigger("Disparar");
//        }

//        // Instancia la bala como hija de puntoDisparo
//        Vector3 spawnPos = puntoDisparo.position;
//        GameObject bala = Instantiate(
//            balaPrefab,
//            spawnPos,
//            puntoDisparo.rotation,
//            puntoDisparo
//        );

//        // Configura el script de la bala
//        var balaScript = bala.GetComponent<BalaEnemigoVolador>();
//        if (balaScript != null)
//        {
//            balaScript.jugador = jugador;
//            balaScript.velocidad = velocidadBala;
//            balaScript.tiempoCarga = tiempoCargaBala;
//        }
//    }
//}




























