using UnityEngine;
using System.Collections;

public class EnemigoVolador : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;               // referencia al player
    public GameObject objetoADestruir;     // otro GameObject a destruir cuando este muera

    [Header("Velocidades")]
    public float velocidadVuelo = 5f;      // velocidad al acercarse
    public float velocidadPicada = 10f;    // velocidad durante la picada
    public float velocidadSubida = 5f;     // velocidad al volver a altura

    [Header("Distancias y Alturas")]
    public float distanciaPicada = 10f;    // distancia horizontal para activar la picada
    public float alturaVuelo = 20f;        // distancia vertical por encima del jugador

    [Header("Temporización")]
    public float tiempoEspera = 1f;        // tiempo de espera antes de volver a acercar
    public float rotacionVelocidad = 2f;   // rapidez de rotacion

    [Header("Flotación")]
    public float amplitudFlotacion = 0.5f; // cuánto sube/baja
    public float frecuenciaFlotacion = 1f; // velocidad de oscilación

    private enum Estado { Acercarse, Picando, Subiendo, Esperando }
    private Estado estadoActual = Estado.Acercarse;
    private Vector3 objetivoPicada;        // posición fija del player al iniciar cada picada

    void Start()
    {
        // Coloca al enemigo a "alturaVuelo" sobre el jugador
        Vector3 p = transform.position;
        p.y = player.position.y + alturaVuelo;
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
        // distancia horizontal (solo XZ)
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z)
        );

        // volar hacia la proyección del player en el plano XZ
        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
        RotarHacia(dirPlanar);
        transform.Translate(Vector3.forward * velocidadVuelo * Time.deltaTime);

        // activar picada
        if (distXZ <= distanciaPicada)
        {
            objetivoPicada = player.position;  // capturamos la Y actual aquí
            estadoActual = Estado.Picando;
        }
    }

    void MoverPicada()
    {
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
        // sube hasta "alturaVuelo" sobre la posición actual del player
        float targetY = player.position.y + alturaVuelo;
        Vector3 destino = new Vector3(transform.position.x, targetY, transform.position.z);
        Vector3 dir = (destino - transform.position).normalized;
        RotarHacia(dir);

        // moverse hacia arriba en world space
        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);

        // clamp para no pasarse
        Vector3 pos = transform.position;
        pos.y = Mathf.Min(pos.y, targetY);
        transform.position = pos;

        if (Mathf.Abs(transform.position.y - targetY) < 0.01f)
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

    void Flotar()
    {
        // siempre flota en torno a (player.y + alturaVuelo)
        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
        Vector3 pos = transform.position;
        pos.y = player.position.y + alturaVuelo + offset;
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

    void OnDestroy()
    {
        if (objetoADestruir != null)
        {
            Destroy(objetoADestruir);
        }
    }
}
