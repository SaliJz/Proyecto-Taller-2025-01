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


