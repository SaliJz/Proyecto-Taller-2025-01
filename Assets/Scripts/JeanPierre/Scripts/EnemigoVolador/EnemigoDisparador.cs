using UnityEngine;

public class EnemigoDisparador : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    public Transform jugador;

    [Header("Movimiento relativo al jugador")]
    public float distanciaDeseada = 5f;
    public float velocidadMovimiento = 3f;
    public float margen = 0.5f; // Zona de tolerancia para movimiento fluido

    [Header("Disparo de bala")]
    public float distanciaDisparo = 10f;
    public GameObject balaPrefab;
    public Transform puntoDisparo;
    public float velocidadBala = 10f;

    [Header("Intervalo entre disparos")]
    public float intervaloDisparo = 3f;
    private float temporizadorDisparo = 0f;

    private void Update()
    {
        if (jugador == null)
        {
            Debug.LogWarning("El jugador no ha sido asignado en " + gameObject.name);
            return;
        }

        // Dirección y distancia horizontal hacia el jugador (ignorando el eje Y)
        Vector3 direccionHaciaJugador = jugador.position - transform.position;
        direccionHaciaJugador.y = 0f;
        float distanciaAlJugador = direccionHaciaJugador.magnitude;

        Vector3 direccionMovimiento = direccionHaciaJugador.normalized;

        if (distanciaAlJugador > distanciaDeseada + margen)
        {
            // Acércate al jugador
            Vector3 nuevaPos = Vector3.MoveTowards(transform.position,
                                                   transform.position + direccionMovimiento,
                                                   velocidadMovimiento * Time.deltaTime);
            transform.position = nuevaPos;
        }
        else if (distanciaAlJugador < distanciaDeseada - margen)
        {
            // Aléjate del jugador
            Vector3 nuevaPos = Vector3.MoveTowards(transform.position,
                                                   transform.position - direccionMovimiento,
                                                   velocidadMovimiento * Time.deltaTime);
            transform.position = nuevaPos;
        }

        // Rotación para mirar al jugador
        Vector3 direccionRotacion = jugador.position - transform.position;
        direccionRotacion.y = 0f;
        if (direccionRotacion != Vector3.zero)
        {
            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionRotacion);
            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadMovimiento * Time.deltaTime);
        }

        // Control de disparos
        temporizadorDisparo += Time.deltaTime;
        if (temporizadorDisparo >= intervaloDisparo)
        {
            if (distanciaAlJugador <= distanciaDisparo)
            {
                DispararBala();
            }
            temporizadorDisparo = 0f;
        }
    }

    private void DispararBala()
    {
        if (balaPrefab != null)
        {
            Vector3 spawnPosition = (puntoDisparo != null) ? puntoDisparo.position : transform.position;
            GameObject balaInstanciada = Instantiate(balaPrefab, spawnPosition, Quaternion.identity);

            Vector3 direccionDisparo = (jugador.position - spawnPosition).normalized;

            BalaEnemigoVolador balaScript = balaInstanciada.GetComponent<BalaEnemigoVolador>();
            if (balaScript != null)
            {
                balaScript.direccion = direccionDisparo;
                balaScript.tiempoCarga = 1.0f;
                balaScript.escalaFinal = new Vector3(1f, 1f, 1f);
                balaScript.velocidad = velocidadBala;
                balaScript.colorFinal = Color.red;
                balaScript.blinkSpeed = 2f;
                balaScript.spawnTransform = puntoDisparo;
            }
            else
            {
                Debug.LogWarning("El prefab de bala no tiene el componente 'BalaEnemigoVolador'");
            }
        }
        else
        {
            Debug.LogWarning("No se ha asignado el prefab de la bala en " + gameObject.name);
        }
    }
}



//using UnityEngine;

//public class EnemigoDisparador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Movimiento relativo al jugador")]
//    // Distancia a la que el enemigo quiere mantenerse del jugador para disparar.
//    public float distanciaDeseada = 5f;
//    // Velocidad con la que se mueve el enemigo.
//    public float velocidadMovimiento = 3f;

//    [Header("Disparo de bala")]
//    // Distancia máxima a la que se permite disparar.
//    public float distanciaDisparo = 10f;
//    // Prefab de la bala (debe tener el script BalaEnemigoVolador).
//    public GameObject balaPrefab;
//    // Transform que indica el punto de creación de la bala (opcional).
//    public Transform puntoDisparo;
//    // Velocidad de la bala.
//    public float velocidadBala = 10f;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    private void Update()
//    {
//        if (jugador == null)
//        {
//            Debug.LogWarning("El jugador no ha sido asignado en " + gameObject.name);
//            return;
//        }

//        // Calcula la dirección y distancia horizontal hacia el jugador (ignorando el eje Y)
//        Vector3 direccionHaciaJugador = jugador.position - transform.position;
//        direccionHaciaJugador.y = 0f;
//        float distanciaAlJugador = direccionHaciaJugador.magnitude;

//        // Si el jugador está a una distancia diferente a la deseada, el enemigo se mueve para ajustarla.
//        // Si está demasiado lejos, se acerca; si está demasiado cerca, se aleja.
//        Vector3 direccionMovimiento = direccionHaciaJugador.normalized;
//        if (distanciaAlJugador > distanciaDeseada)
//        {
//            // Acércate al jugador.
//            transform.position += direccionMovimiento * velocidadMovimiento * Time.deltaTime;
//        }
//        else if (distanciaAlJugador < distanciaDeseada)
//        {
//            // Aléjate del jugador.
//            transform.position -= direccionMovimiento * velocidadMovimiento * Time.deltaTime;
//        }
//        // Si la distancia es exactamente la deseada, no se mueve.

//        // Rotación para mirar al jugador
//        Vector3 direccionRotacion = jugador.position - transform.position;
//        direccionRotacion.y = 0f;
//        if (direccionRotacion != Vector3.zero)
//        {
//            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionRotacion);
//            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadMovimiento * Time.deltaTime);
//        }

//        // Control de disparos
//        temporizadorDisparo += Time.deltaTime;
//        if (temporizadorDisparo >= intervaloDisparo)
//        {
//            // Solo dispara si el jugador está dentro de la distancia de disparo.
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
//            // Se utiliza el punto asignado; si no se asigna, se usa la posición del enemigo.
//            Vector3 spawnPosition = (puntoDisparo != null) ? puntoDisparo.position : transform.position;
//            GameObject balaInstanciada = Instantiate(balaPrefab, spawnPosition, Quaternion.identity);

//            // Calculamos la dirección hacia el jugador desde el punto de disparo.
//            Vector3 direccionDisparo = (jugador.position - spawnPosition).normalized;

//            // Asignar parámetros al script de la bala (BalaEnemigoVolador)
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




//using UnityEngine;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencia al Jugador")]
//    public Transform jugador;

//    [Header("Parametros de movimiento (valores asignados al azar)")]
//    public Vector2 rangoDistanciaDeseada = new Vector2(3f, 7f);
//    public Vector2 rangoVelocidadMovimiento = new Vector2(2f, 4f);
//    public Vector2 rangoAlturaVuelo = new Vector2(4f, 8f);

//    [Header("Oscilacion para efecto de alas (valores asignados al azar)")]
//    public Vector2 rangoAmplitudOscilacion = new Vector2(0.3f, 0.7f);
//    public Vector2 rangoFrecuenciaOscilacion = new Vector2(1.5f, 3f);

//    [Header("Rotacion")]
//    public float velocidadRotacion = 5f;

//    [Header("Disparo de bala")]
//    // Distancia máxima a la que se permite disparar la bala.
//    public float distanciaDisparo = 10f;
//    // Prefab de la bala (asegúrate de que tenga el script BalaEnemigoVolador).
//    public GameObject balaPrefab;

//    [Header("Intervalo entre disparos")]
//    public float intervaloDisparo = 3f;
//    private float temporizadorDisparo = 0f;

//    [Header("Punto de creación de la bala")]
//    // Transform que indica el punto de creación de la bala, asignable en el Inspector.
//    public Transform puntoDisparo;

//    [Header("Velocidad de bala")]
//    // Velocidad única que se usará para todos los disparos.
//    public float velocidadBala = 10f;

//    // Variables internas asignadas al azar.
//    private float distanciaDeseada;
//    private float velocidadMovimiento;
//    private float alturaVuelo;
//    private float amplitudOscilacion;
//    private float frecuenciaOscilacion;

//    private void Start()
//    {
//        // Asignamos valores aleatorios para movimiento y oscilación.
//        distanciaDeseada = Random.Range(rangoDistanciaDeseada.x, rangoDistanciaDeseada.y);
//        velocidadMovimiento = Random.Range(rangoVelocidadMovimiento.x, rangoVelocidadMovimiento.y);
//        alturaVuelo = Random.Range(rangoAlturaVuelo.x, rangoAlturaVuelo.y);
//        amplitudOscilacion = Random.Range(rangoAmplitudOscilacion.x, rangoAmplitudOscilacion.y);
//        frecuenciaOscilacion = Random.Range(rangoFrecuenciaOscilacion.x, rangoFrecuenciaOscilacion.y);
//    }

//    private void Update()
//    {
//        if (jugador == null)
//        {
//            Debug.LogWarning("El jugador no ha sido asignado en " + gameObject.name);
//            return;
//        }

//        // --- Movimiento del enemigo ---
//        Vector3 posJugadorXZ = new Vector3(jugador.position.x, 0f, jugador.position.z);
//        Vector3 posEnemigoXZ = new Vector3(transform.position.x, 0f, transform.position.z);
//        Vector3 direccion = posJugadorXZ - posEnemigoXZ;
//        Vector3 posDeseadaXZ = posJugadorXZ - (direccion.normalized * distanciaDeseada);
//        Vector3 nuevaPosXZ = Vector3.MoveTowards(posEnemigoXZ, posDeseadaXZ, velocidadMovimiento * Time.deltaTime);
//        float nuevaY = (jugador.position.y + alturaVuelo) + Mathf.Sin(Time.time * frecuenciaOscilacion) * amplitudOscilacion;
//        transform.position = new Vector3(nuevaPosXZ.x, nuevaY, nuevaPosXZ.z);

//        // --- Rotación para mirar al jugador ---
//        Vector3 direccionRot = jugador.position - transform.position;
//        direccionRot.y = 0f;
//        if (direccionRot != Vector3.zero)
//        {
//            Quaternion rotacionDeseada = Quaternion.LookRotation(direccionRot);
//            transform.rotation = Quaternion.Slerp(transform.rotation, rotacionDeseada, velocidadRotacion * Time.deltaTime);
//        }

//        // --- Control de disparos ---
//        temporizadorDisparo += Time.deltaTime;
//        if (temporizadorDisparo >= intervaloDisparo)
//        {
//            float distanciaAlJugador = Vector3.Distance(transform.position, jugador.position);
//            if (distanciaAlJugador <= distanciaDisparo)
//            {
//                DispararBala();
//                temporizadorDisparo = 0f;
//            }
//        }
//    }

//    private void DispararBala()
//    {
//        if (balaPrefab != null)
//        {
//            // Se utiliza el punto asignado; si no se asigna, se usa la posición del enemigo.
//            Vector3 spawnPosition = (puntoDisparo != null) ? puntoDisparo.position : transform.position;
//            GameObject balaInstanciada = Instantiate(balaPrefab, spawnPosition, Quaternion.identity);

//            // Calculamos la dirección hacia el jugador desde el punto de disparo.
//            Vector3 direccionDisparo = (jugador.position - spawnPosition).normalized;

//            // Asignamos los parámetros al script de la bala (BalaEnemigoVolador).
//            BalaEnemigoVolador balaScript = balaInstanciada.GetComponent<BalaEnemigoVolador>();
//            if (balaScript != null)
//            {
//                balaScript.direccion = direccionDisparo;
//                balaScript.tiempoCarga = 1.0f;
//                balaScript.escalaFinal = new Vector3(1f, 1f, 1f);
//                balaScript.velocidad = velocidadBala;
//                // Asignamos el efecto de parpadeo épico (color y velocidad de blink).
//                balaScript.colorFinal = Color.red;
//                balaScript.blinkSpeed = 2f;
//                // También asignamos el punto de spawn para que la bala pueda seguirlo si fuera necesario.
//                balaScript.spawnTransform = puntoDisparo;
//            }
//        }
//        else
//        {
//            Debug.LogWarning("No se ha asignado el prefab de la bala en " + gameObject.name);
//        }
//    }
//}

