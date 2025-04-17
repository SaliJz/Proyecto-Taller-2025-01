using System.Collections;
using UnityEngine;

public class BalaEnemigoVolador : MonoBehaviour
{
    [Header("Par�metros de carga y escala")]
    public float tiempoCarga = 1.0f;
    public Vector3 escalaFinal = Vector3.one;

    [Header("Par�metros de disparo")]
    public float velocidad = 10f;
    [HideInInspector]
    public Vector3 direccion;

    [Header("Efecto de parpadeo �pico")]
    public Color colorFinal = Color.red;
    public float blinkSpeed = 2f;

    // La bala seguir� al spawnTransform mientras se encuentre en carga.
    // Se asigna desde el script del enemigo.
    public Transform spawnTransform;
    // Se guarda la �ltima posici�n v�lida del spawn.
    private Vector3 spawnPosition;

    private bool disparada = false;
    private Renderer rend;

    private void Start()
    {
        // Se inicia la escala peque�a.
        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        rend = GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.white;
        }
        // Se asigna la posici�n inicial: si hay spawnTransform, se toma su posici�n; de lo contrario, se usa la posici�n actual.
        if (spawnTransform)
        {
            spawnPosition = spawnTransform.position;
        }
        else
        {
            spawnPosition = transform.position;
        }
        StartCoroutine(CargarYExpulsar());
    }

    private IEnumerator CargarYExpulsar()
    {
        float elapsed = 0f;
        Vector3 escalaInicial = transform.localScale;

        // Durante la fase de carga, la bala sigue la posici�n del spawnTransform (si existe)
        // Si se destruye, se usa la �ltima posici�n conocida.
        while (elapsed < tiempoCarga)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / tiempoCarga;
            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);

            // Actualizamos la posici�n solo si spawnTransform sigue siendo v�lido
            if (spawnTransform)
            {
                transform.position = spawnTransform.position;
                // Guardamos la posici�n actual para usarla en caso de error o destrucci�n
                spawnPosition = spawnTransform.position;
            }
            else
            {
                // Si spawnTransform es nulo o ha sido destruido, se usa la �ltima posici�n almacenada.
                transform.position = spawnPosition;
            }

            yield return null;
        }

        // Cuando finaliza la carga, la bala se desengancha y se mueve de forma independiente.
        if (spawnTransform)
        {
            transform.parent = null;
        }
        spawnTransform = null;
        disparada = true;
    }

    private void Update()
    {
        if (disparada)
        {
            // La bala se mueve en la direcci�n asignada.
            transform.position += direccion * velocidad * Time.deltaTime;

            // Efecto de parpadeo: interpolaci�n entre colorFinal y blanco.
            if (rend != null)
            {
                float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
                Color nuevoColor = Color.Lerp(colorFinal, Color.white, t);
                nuevoColor.a = Mathf.Lerp(0.5f, 1f, t);
                rend.material.color = nuevoColor;
            }
        }
    }

    // Si la bala choca con un GameObject con tag "Player", se le inflige da�o y se destruye.
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(20);
            }
            Destroy(gameObject);
        }
    }
}


//using System.Collections;
//using UnityEngine;

//public class BalaEnemigoVolador : MonoBehaviour
//{
//    [Header("Parametros de carga y escala")]
//    // Tiempo durante el cual la bala cambia su escala (de 0.1 a escalaFinal)
//    public float tiempoCarga = 1.0f;
//    // Escala final que se alcanzar� antes del disparo
//    public Vector3 escalaFinal = Vector3.one;

//    [Header("Parametros de disparo")]
//    // Velocidad a la que se desplazar� la bala una vez expulsada
//    public float velocidad = 10f;
//    // Direcci�n (normalizada) hacia la que se disparar� la bala; se asigna desde el enemigo
//    [HideInInspector]
//    public Vector3 direccion;

//    [Header("Efecto de parpadeo �pico")]
//    // Color base para el efecto (se interpola con blanco)
//    public Color colorFinal = Color.red;
//    // Controla la velocidad del efecto de parpadeo
//    public float blinkSpeed = 2f;

//    private bool disparada = false;
//    private Renderer rend;
//    // Posici�n inicial (de creaci�n) que se mantiene hasta que la bala es expulsada
//    private Vector3 spawnPosition;

//    private void Start()
//    {
//        // Se inicia con una escala peque�a en cada eje
//        transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
//        // Se registra la posici�n de creaci�n
//        spawnPosition = transform.position;
//        rend = GetComponent<Renderer>();
//        if (rend != null)
//        {
//            // Color inicial, por ejemplo blanco con opacidad completa
//            rend.material.color = Color.white;
//        }
//        // Inicia la secuencia de carga
//        StartCoroutine(CargarYExpulsar());
//    }

//    private IEnumerator CargarYExpulsar()
//    {
//        float elapsed = 0f;
//        Vector3 escalaInicial = transform.localScale;
//        // Durante la fase de carga, se interpola la escala sin mover la bala de su posici�n de creaci�n
//        while (elapsed < tiempoCarga)
//        {
//            elapsed += Time.deltaTime;
//            float t = elapsed / tiempoCarga;
//            transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
//            // Se mantiene la posici�n en spawnPosition
//            transform.position = spawnPosition;
//            yield return null;
//        }
//        // Finalizada la carga, la bala se marca como expulsada y comienza a moverse
//        disparada = true;
//    }

//    private void Update()
//    {
//        if (disparada)
//        {
//            // Movimiento de la bala
//            transform.position += direccion * velocidad * Time.deltaTime;

//            // Efecto de parpadeo �pico: transici�n fluida con PingPong
//            if (rend != null)
//            {
//                float t = Mathf.PingPong(Time.time * blinkSpeed, 1f);
//                Color nuevoColor = Color.Lerp(colorFinal, Color.white, t);
//                nuevoColor.a = Mathf.Lerp(0.5f, 1f, t);
//                rend.material.color = nuevoColor;
//            }
//        }
//    }
//}
