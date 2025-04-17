using UnityEngine;
using System.Collections;

public class EnemigoVolador : MonoBehaviour
{
    public Transform player;               // referencia al player
    public float velocidadVuelo = 5f;      // velocidad al acercarse
    public float velocidadPicada = 10f;    // velocidad durante la picada
    public float velocidadSubida = 5f;     // velocidad al volver a altura
    public float distanciaPicada = 10f;    // distancia horizontal para activar la picada
    public float alturaVuelo = 20f;        // altura maxima de vuelo
    public float tiempoEspera = 1f;        // tiempo de espera antes de volver a acercar
    public float rotacionVelocidad = 2f;   // rapidez de rotacion

    // Parámetros de flotación
    public float amplitudFlotacion = 0.5f; // cuánto sube/baja
    public float frecuenciaFlotacion = 1f; // velocidad de oscilación

    private enum Estado { Acercarse, Picando, Subiendo, Esperando }
    private Estado estadoActual = Estado.Acercarse;
    private Vector3 objetivoPicada;        // posicion fija del player al iniciar cada picada

    void Start()
    {
        // asegurar altura inicial
        Vector3 p = transform.position;
        p.y = alturaVuelo;
        transform.position = p;
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.Acercarse:
                MoverAcercarse();
                Flotar();
                break;
            case Estado.Picando:
                MoverPicada();
                break;
            case Estado.Subiendo:
                MoverSubida();
                break;
            case Estado.Esperando:
                MirarAlPlayer();
                Flotar();
                break;
        }
    }

    void MoverAcercarse()
    {
        // calcular distancia horizontal (solo XZ)
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z)
        );

        // volar hacia la proyeccion del player en el mismo nivel
        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
        RotarHacia(dirPlanar);

        // mover hacia adelante localmente
        transform.Translate(Vector3.forward * velocidadVuelo * Time.deltaTime);

        // activar picada si estamos lo bastante cerca
        if (distXZ <= distanciaPicada)
        {
            objetivoPicada = player.position;
            estadoActual = Estado.Picando;
        }
    }

    void MoverPicada()
    {
        // picada hacia objetivo fijo
        Vector3 dir = (objetivoPicada - transform.position).normalized;
        RotarHacia(dir);
        transform.position = Vector3.MoveTowards(
            transform.position,
            objetivoPicada,
            velocidadPicada * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
        {
            estadoActual = Estado.Subiendo;
        }
    }

    void MoverSubida()
    {
        // volver a altura maxima sin pasarse
        Vector3 destino = new Vector3(transform.position.x, alturaVuelo, transform.position.z);
        Vector3 dir = (destino - transform.position).normalized;
        RotarHacia(dir);

        // moverse hacia arriba suavemente
        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);
        // clamp altura maxima
        Vector3 pos = transform.position;
        pos.y = Mathf.Min(pos.y, alturaVuelo);
        transform.position = pos;

        if (Mathf.Abs(transform.position.y - alturaVuelo) < 0.01f)
        {
            estadoActual = Estado.Esperando;
            StartCoroutine(TemporizarEspera());
        }
    }

    IEnumerator TemporizarEspera()
    {
        yield return new WaitForSeconds(tiempoEspera);
        estadoActual = Estado.Acercarse;
    }

    void MirarAlPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        RotarHacia(dir);
    }

    // Efecto de flotación vertical
    void Flotar()
    {
        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
        Vector3 pos = transform.position;
        pos.y = alturaVuelo + offset;
        transform.position = pos;
    }

    void RotarHacia(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion rotMeta = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            rotMeta,
            rotacionVelocidad * Time.deltaTime
        );
    }
}




//using UnityEngine;
//using System.Collections;

//public class EnemigoVolador : MonoBehaviour
//{
//    public Transform player;               // referencia al player
//    public float velocidadVuelo = 5f;      // velocidad al acercarse
//    public float velocidadPicada = 10f;    // velocidad durante la picada
//    public float velocidadSubida = 5f;     // velocidad al volver a altura
//    public float distanciaPicada = 10f;    // distancia horizontal para activar la picada
//    public float alturaVuelo = 20f;        // altura maxima de vuelo
//    public float tiempoEspera = 1f;        // tiempo de espera antes de volver a acercar
//    public float rotacionVelocidad = 2f;   // rapidez de rotacion

//    private enum Estado { Acercarse, Picando, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;
//    private Vector3 objetivoPicada;        // posicion fija del player al iniciar cada picada

//    void Start()
//    {
//        // asegurar altura inicial
//        Vector3 p = transform.position;
//        p.y = alturaVuelo;
//        transform.position = p;
//    }

//    void Update()
//    {
//        switch (estadoActual)
//        {
//            case Estado.Acercarse:
//                MoverAcercarse();
//                break;
//            case Estado.Picando:
//                MoverPicada();
//                break;
//            case Estado.Subiendo:
//                MoverSubida();
//                break;
//            case Estado.Esperando:
//                MirarAlPlayer();
//                break;
//        }
//    }

//    void MoverAcercarse()
//    {
//        // calcular distancia horizontal (solo XZ)
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        // volar hacia la proyeccion del player en el mismo nivel
//        Vector3 destinoPlanar = new Vector3(player.position.x, alturaVuelo, player.position.z);
//        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
//        RotarHacia(dirPlanar);

//        // mover hacia adelante localmente para transicion suave
//        transform.Translate(Vector3.forward * velocidadVuelo * Time.deltaTime);
//        // clamp para no pasarse de altura
//        Vector3 pos = transform.position;
//        pos.y = alturaVuelo;
//        transform.position = pos;

//        // activar picada si estamos lo bastante cerca
//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;
//            estadoActual = Estado.Picando;
//        }
//    }

//    void MoverPicada()
//    {
//        // picada hacia objetivo fijo
//        Vector3 dir = (objetivoPicada - transform.position).normalized;
//        RotarHacia(dir);
//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            objetivoPicada,
//            velocidadPicada * Time.deltaTime
//        );

//        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
//        {
//            estadoActual = Estado.Subiendo;
//        }
//    }

//    void MoverSubida()
//    {
//        // volver a altura maxima sin pasarse
//        Vector3 destino = new Vector3(transform.position.x, alturaVuelo, transform.position.z);
//        Vector3 dir = (destino - transform.position).normalized;
//        RotarHacia(dir);

//        // moverse hacia arriba suavemente
//        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);
//        // clamp altura maxima
//        Vector3 pos = transform.position;
//        pos.y = Mathf.Min(pos.y, alturaVuelo);
//        transform.position = pos;

//        if (Mathf.Abs(transform.position.y - alturaVuelo) < 0.01f)
//        {
//            estadoActual = Estado.Esperando;
//            StartCoroutine(TemporizarEspera());
//        }
//    }

//    IEnumerator TemporizarEspera()
//    {
//        yield return new WaitForSeconds(tiempoEspera);
//        estadoActual = Estado.Acercarse;
//    }

//    void MirarAlPlayer()
//    {
//        Vector3 dir = (player.position - transform.position).normalized;
//        RotarHacia(dir);
//    }

//    void RotarHacia(Vector3 dir)
//    {
//        if (dir == Vector3.zero) return;
//        Quaternion rotMeta = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.Slerp(
//            transform.rotation,
//            rotMeta,
//            rotacionVelocidad * Time.deltaTime
//        );
//    }
//}
