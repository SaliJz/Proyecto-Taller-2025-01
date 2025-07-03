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
    private Vector3 direccion;
    private Rigidbody rb;

    private void Start()
    {
        // Cachear componentes
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        // Hacer la bala cinem�tica para que no reaccione a colisiones/gravedad mientras carga
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Escala inicial peque�a
        transform.localScale = Vector3.one * 0.1f;
        if (rend) rend.material.color = Color.white;

        StartCoroutine(CargarYDisparar());
    }

    private IEnumerator CargarYDisparar()
    {
        float elapsed = 0f;
        Vector3 escalaInicial = transform.localScale;

        // Fase de carga: escala desde 0.1 hasta 1 y sigue siendo hija de puntoDisparo
        while (elapsed < tiempoCarga)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiempoCarga;
            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.one, t);
            yield return null;
        }

        // Calcula direcci�n hacia el jugador justo al disparo
        direccion = (jugador.position - transform.position).normalized;

        // Desparenta y habilita f�sica para que reciba velocidad
        transform.parent = null;
        disparada = true;

        if (rb != null)
        {
            rb.isKinematic = false;              // ahora s� reacciona a f�sica
            rb.linearVelocity = direccion * velocidad;
        }
    }

    private void Update()
    {
        if (!disparada)
        {
            // Mientras no dispare, obligamos su posici�n a coincidir con el puntoDisparo
            if (transform.parent != null)
                transform.position = transform.parent.position;

            // Efecto de parpadeo mientras carga
            if (rend)
            {
                float t = Mathf.PingPong(Time.time * 2f, 1f);
                Color c = Color.Lerp(Color.red, Color.white, t);
                c.a = Mathf.Lerp(0.5f, 1f, t);
                rend.material.color = c;
            }
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


//// BalaEnemigoVolador.cs
//using System.Collections;
//using UnityEngine;

//public class BalaEnemigoVolador : MonoBehaviour
//{
//    [HideInInspector] public Transform jugador;
//    [HideInInspector] public float velocidad;

//    [Header("Carga antes del disparo")]
//    public float tiempoCarga = 1f;
//    private bool disparada = false;
//    private Renderer rend;
//    private Vector3 direccion;    // guardamos la direcci�n al disparar

//    private void Start()
//    {
//        transform.localScale = Vector3.one * 0.1f;
//        rend = GetComponent<Renderer>();
//        if (rend) rend.material.color = Color.white;

//        StartCoroutine(CargarYDisparar());
//    }

//    private IEnumerator CargarYDisparar()
//    {
//        float elapsed = 0f;
//        Vector3 escalaInicial = transform.localScale;

//        // Fase de carga: sigue en puntoDisparo (parent) y escala
//        while (elapsed < tiempoCarga)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / tiempoCarga;
//            transform.localScale = Vector3.Lerp(escalaInicial, Vector3.one, t);
//            yield return null;
//        }

//        // Desparent para liberar la bala
//        transform.parent = null;
//        disparada = true;

//        // Calcula la direcci�n justo al disparo
//        direccion = (jugador.position - transform.position).normalized;

//        // Si tiene Rigidbody, asignamos su velocidad
//        var rb = GetComponent<Rigidbody>();
//        if (rb != null)
//        {
//            rb.velocity = direccion * velocidad;
//        }
//        // si no tiene Rigidbody, podr�as moverlo en Update con 'direccion'
//    }

//    private void Update()
//    {
//        if (!disparada)
//            return;

//        // Si no tienes Rigidbody, usar�as esto:
//        // transform.position += direccion * velocidad * Time.deltaTime;

//        // (Opcional) efectos de parpadeo aqu�...
//        if (rend)
//        {
//            float t = Mathf.PingPong(Time.time * 2f, 1f);
//            Color nuevoColor = Color.Lerp(Color.red, Color.white, t);
//            nuevoColor.a = Mathf.Lerp(0.5f, 1f, t);
//            rend.material.color = nuevoColor;
//        }
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            var ph = other.GetComponent<PlayerHealth>();
//            if (ph) ph.TakeDamage(20);
//            Destroy(gameObject);
//        }
//    }
//}



















