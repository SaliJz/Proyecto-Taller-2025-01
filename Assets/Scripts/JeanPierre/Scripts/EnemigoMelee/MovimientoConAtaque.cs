﻿using UnityEngine;

public class MovimientoConAtaque : MonoBehaviour
{
    [Header("Referencias")]
    public Transform playerTransform;
    private Animator animator;

    [Header("Ajustes de movimiento")]
    public float speed = 5f;
    public float rotationSpeed = 5f;

    [Header("Ajustes de ataque")]
    public float attackRange = 2f;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();
        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // --- Ataque manual con tecla E ---
        if (Input.GetKeyDown(KeyCode.E))
        {
            animator.SetBool("Ataque", true);
            return; // opcional, para que no haga nada más este frame
        }
        if (Input.GetKeyUp(KeyCode.E))
        {
            animator.SetBool("Ataque", false);
        }

        // --- Rotación y movimiento hacia el jugador ---
        RotateTowardsPlayer();

        float distance = Vector3.Distance(
            new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z),
            transform.position
        );

        // --- Ataque automático al acercarse ---
        if (distance <= attackRange)
        {
            animator.SetBool("Ataque", true);
        }
        else
        {
            animator.SetBool("Ataque", false);
            MoveTowardsPlayer();
        }
    }

    void RotateTowardsPlayer()
    {
        Vector3 dir = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z)
                      - transform.position;
        if (dir.sqrMagnitude > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, rotationSpeed * Time.deltaTime);
        }
    }

    void MoveTowardsPlayer()
    {
        Vector3 target = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
        Vector3 dir = (target - transform.position).normalized;
        transform.position += dir * speed * Time.deltaTime;
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



