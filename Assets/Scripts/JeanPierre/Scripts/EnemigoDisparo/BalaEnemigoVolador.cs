// BalaEnemigoVolador.cs
using System.Collections;
using UnityEngine;

public class BalaEnemigoVolador : MonoBehaviour
{
    [HideInInspector] public Transform jugador;
    [HideInInspector] public float velocidad;

    [Header("Carga antes del disparo")]
    public float tiempoCarga = 1f;
    private bool disparada = false;
    private Renderer rend;
    private Vector3 direccion;    // guardamos la dirección al disparar

    private void Start()
    {
        transform.localScale = Vector3.one * 0.1f;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = Color.white;

        StartCoroutine(CargarYDisparar());
    }

    private IEnumerator CargarYDisparar()
    {
        float elapsed = 0f;
        Vector3 escalaInicial = transform.localScale;

        // Fase de carga: sigue en puntoDisparo (parent) y escala
        while (elapsed < tiempoCarga)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiempoCarga;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.one, t);
            yield return null;
        }

        // Desparent para liberar la bala
        transform.parent = null;
        disparada = true;

        // Calcula la dirección justo al disparo
        direccion = (jugador.position - transform.position).normalized;

        // Si tiene Rigidbody, asignamos su velocidad
        var rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direccion * velocidad;
        }
        // si no tiene Rigidbody, podrías moverlo en Update con 'direccion'
    }

    private void Update()
    {
        if (!disparada)
            return;

        // Si no tienes Rigidbody, usarías esto:
        // transform.position += direccion * velocidad * Time.deltaTime;

        // (Opcional) efectos de parpadeo aquí...
        if (rend)
        {
            float t = Mathf.PingPong(Time.time * 2f, 1f);
            Color nuevoColor = Color.Lerp(Color.red, Color.white, t);
            nuevoColor.a = Mathf.Lerp(0.5f, 1f, t);
            rend.material.color = nuevoColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var ph = other.GetComponent<PlayerHealth>();
            if (ph) ph.TakeDamage(20);
            Destroy(gameObject);
        }
    }
}


//using System.Collections;
//using UnityEngine;

//public class BalaEnemigoVolador : MonoBehaviour
//{
//    [Header("Parámetros de carga y escala")]
//    public float tiempoCarga = 1.0f;
//    public Vector3 escalaFinal = Vector3.one;

//    [Header("Parámetros de disparo")]
//    public float velocidad = 10f;
//    [HideInInspector]
//    public Vector3 direccion;

//    [Header("Efecto de parpadeo épico")]
//    public Color colorFinal = Color.red;
//    public float blinkSpeed = 2f;

//    // La bala seguirá al spawnTransform mientras se encuentre en carga.
//    // Se asigna desde el script del enemigo.
//    public Transform spawnTransform;
//    // Se guarda la última posición válida del spawn.
//    private Vector3 spawnPosition;

//    private bool disparada = false;
//    private Renderer rend;

//    private void Start()
//    {
//        // Se inicia la escala pequeña.
//        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//        rend = GetComponent<Renderer>();
//        if (rend != null)
//        {
//            rend.material.color = Color.white;
//        }
//        // Se asigna la posición inicial: si hay spawnTransform, se toma su posición; de lo contrario, se usa la posición actual.
//        if (spawnTransform)
//        {
//            spawnPosition = spawnTransform.position;
//        }
//        else
//        {
//            spawnPosition = transform.position;
//        }
//        StartCoroutine(CargarYExpulsar());
//    }

//    private IEnumerator CargarYExpulsar()
//    {
//        float elapsed = 0f;
//        Vector3 escalaInicial = transform.localScale;

//        // Durante la fase de carga, la bala sigue la posición del spawnTransform (si existe)
//        // Si se destruye, se usa la última posición conocida.
//        while (elapsed < tiempoCarga)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / tiempoCarga;
//            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);

//            // Actualizamos la posición solo si spawnTransform sigue siendo válido
//            if (spawnTransform)
//            {
//                transform.position = spawnTransform.position;
//                // Guardamos la posición actual para usarla en caso de error o destrucción
//                spawnPosition = spawnTransform.position;
//            }
//            else
//            {
//                // Si spawnTransform es nulo o ha sido destruido, se usa la última posición almacenada.
//                transform.position = spawnPosition;
//            }

//            yield return null;
//        }

//        // Cuando finaliza la carga, la bala se desengancha y se mueve de forma independiente.
//        if (spawnTransform)
//        {
//            transform.parent = null;
//        }
//        spawnTransform = null;
//        disparada = true;
//    }

//    private void Update()
//    {
//        if (disparada)
//        {
//            // La bala se mueve en la dirección asignada.
//            transform.position += direccion * velocidad * Time.deltaTime;

//            // Efecto de parpadeo: interpolación entre colorFinal y blanco.
//            if (rend != null)
//            {
//                float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
//                Color nuevoColor = Color.Lerp(colorFinal, Color.white, t);
//                nuevoColor.a = Mathf.Lerp(0.5f, 1f, t);
//                rend.material.color = nuevoColor;
//            }
//        }
//    }

//    // Si la bala choca con un GameObject con tag "Player", se le inflige daño y se destruye.
//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
//            if (playerHealth != null)
//            {
//                playerHealth.TakeDamage(20);
//            }
//            Destroy(gameObject);
//        }
//    }
//}

