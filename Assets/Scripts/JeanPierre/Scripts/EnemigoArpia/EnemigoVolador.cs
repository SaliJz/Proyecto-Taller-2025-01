using UnityEngine;
using System.Collections;

public class EnemigoVolador : MonoBehaviour
{
    [Header("Referencias")]
    private Transform player;               // ahora se captura automáticamente por tag
    public GameObject objetoADestruir;      // otro GameObject a destruir cuando este muera

    [Header("Prefabs de efectos")]
    public GameObject prefabAviso;          // aviso antes de la caída
    public GameObject prefabImpacto;        // efecto al impactar

    [Header("Configuración de posición")]
    [Tooltip("Altura en Y a la que se instancian los prefabs de aviso e impacto")]
    public float alturaPrefabY = 0f;

    [Header("Velocidades")]
    public float velocidadVuelo = 5f;       // velocidad al acercarse
    public float velocidadPicada = 10f;     // velocidad durante la picada
    public float velocidadSubida = 5f;      // velocidad al volver a altura

    [Header("Distancias y Alturas")]
    public float distanciaPicada = 10f;     // distancia horizontal para activar la picada
    public float alturaVuelo = 20f;         // altura sobre el jugador

    [Header("Temporización")]
    public float tiempoEspera = 1f;         // espera tras subir antes de reanudar
    public float tiempoAntesDeCaer = 1f;    // tiempo entre aviso y picada
    public float tiempoEnTierra = 1f;       // tiempo que permanece en la posición de impacto
    public float rotacionVelocidad = 2f;    // rapidez de rotación

    [Header("Flotación")]
    public float amplitudFlotacion = 0.5f;  // amplitud de oscilación vertical
    public float frecuenciaFlotacion = 1f;  // frecuencia de oscilación

    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
    private Estado estadoActual = Estado.Acercarse;

    private Vector3 objetivoPicada;         // posición capturada para la picada
    private GameObject avisoInstancia;      // instancia del prefabAviso

    void Start()
    {
        // Capturamos al jugador automáticamente por tag
        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
        {
            player = jugador.transform;
        }
        else
        {
            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
            enabled = false; // desactivamos este script para evitar errores posteriores
            return;
        }

        // Inicial: sitúa al enemigo a alturaVuelo sobre el jugador
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
            case Estado.Avisando:
                MirarAlPlayer();
                break;
            case Estado.Picando:
                MoverPicada();
                break;
            case Estado.EnTierra:
                MirarAlPlayer();
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
        // distancia horizontal (XZ) al jugador
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z)
        );

        // volar hacia la proyección del jugador
        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
        RotarHacia(dirPlanar);
        transform.Translate(Vector3.forward * velocidadVuelo * Time.deltaTime);

        // al alcanzar la distancia de picada:
        if (distXZ <= distanciaPicada)
        {
            objetivoPicada = player.position;  // fija la posición actual del jugador

            // crea aviso en la altura configurada
            if (prefabAviso != null)
            {
                Vector3 posAviso = new Vector3(objetivoPicada.x, alturaPrefabY, objetivoPicada.z);
                avisoInstancia = Instantiate(prefabAviso, posAviso, Quaternion.identity);
            }

            estadoActual = Estado.Avisando;
            StartCoroutine(EsperarYCaer());
        }
    }

    IEnumerator EsperarYCaer()
    {
        yield return new WaitForSeconds(tiempoAntesDeCaer);
        estadoActual = Estado.Picando;
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

        // al llegar al punto de impacto:
        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
        {
            // crea efecto de impacto en la altura configurada
            if (prefabImpacto != null)
            {
                Vector3 posImpacto = new Vector3(objetivoPicada.x, alturaPrefabY, objetivoPicada.z);
                Instantiate(prefabImpacto, posImpacto, Quaternion.identity);
            }

            // destruye el aviso
            if (avisoInstancia != null)
                Destroy(avisoInstancia);

            estadoActual = Estado.EnTierra;
            StartCoroutine(EsperarEnTierra());
        }
    }

    IEnumerator EsperarEnTierra()
    {
        yield return new WaitForSeconds(tiempoEnTierra);
        estadoActual = Estado.Subiendo;
    }

    void MoverSubida()
    {
        // sube hasta la altura deseada sobre el jugador
        float targetY = player.position.y + alturaVuelo;
        Vector3 destino = new Vector3(transform.position.x, targetY, transform.position.z);
        Vector3 dir = (destino - transform.position).normalized;
        RotarHacia(dir);

        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);

        // no sobrepasar la altura
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
        // oscila verticalmente alrededor de la altura de vuelo
        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
        Vector3 pos = transform.position;
        pos.y = player.position.y + alturaVuelo + offset;
        transform.position = pos;
    }

    void RotarHacia(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion meta = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, meta, rotacionVelocidad * Time.deltaTime);
    }

    void OnDestroy()
    {
        // al destruirse, también destruye el objeto referenciado
        if (objetoADestruir != null)
            Destroy(objetoADestruir);
    }
}



//using UnityEngine;
//using System.Collections;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencias")]
//    public Transform player;               // referencia al player
//    public GameObject objetoADestruir;     // otro GameObject a destruir cuando este muera

//    [Header("Prefabs de efectos")]
//    public GameObject prefabAviso;         // aviso antes de la caída
//    public GameObject prefabImpacto;       // efecto al impactar

//    [Header("Configuración de posición")]
//    [Tooltip("Altura en Y a la que se instancian los prefabs de aviso e impacto")]
//    public float alturaPrefabY = 0f;

//    [Header("Velocidades")]
//    public float velocidadVuelo = 5f;      // velocidad al acercarse
//    public float velocidadPicada = 10f;    // velocidad durante la picada
//    public float velocidadSubida = 5f;     // velocidad al volver a altura

//    [Header("Distancias y Alturas")]
//    public float distanciaPicada = 10f;    // distancia horizontal para activar la picada
//    public float alturaVuelo = 20f;        // altura sobre el jugador

//    [Header("Temporización")]
//    public float tiempoEspera = 1f;        // espera tras subir antes de reanudar
//    public float tiempoAntesDeCaer = 1f;   // tiempo entre aviso y picada
//    public float tiempoEnTierra = 1f;      // tiempo que permanece en la posición de impacto
//    public float rotacionVelocidad = 2f;   // rapidez de rotación

//    [Header("Flotación")]
//    public float amplitudFlotacion = 0.5f; // amplitud de oscilación vertical
//    public float frecuenciaFlotacion = 1f; // frecuencia de oscilación

//    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;

//    private Vector3 objetivoPicada;        // posición capturada para la picada
//    private GameObject avisoInstancia;     // instancia del prefabAviso

//    void Start()
//    {
//        // Inicial: sitúa al enemigo a alturaVuelo sobre el jugador
//        Vector3 p = transform.position;
//        p.y = player.position.y + alturaVuelo;
//        transform.position = p;
//    }

//    void Update()
//    {
//        switch (estadoActual)
//        {
//            case Estado.Acercarse:
//                MoverAcercarse();
//                Flotar();
//                break;
//            case Estado.Avisando:
//                MirarAlPlayer();
//                break;
//            case Estado.Picando:
//                MoverPicada();
//                break;
//            case Estado.EnTierra:
//                MirarAlPlayer();
//                break;
//            case Estado.Subiendo:
//                MoverSubida();
//                break;
//            case Estado.Esperando:
//                MirarAlPlayer();
//                Flotar();
//                break;
//        }
//    }

//    void MoverAcercarse()
//    {
//        // distancia horizontal (XZ) al jugador
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        // volar hacia la proyección del jugador
//        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
//        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
//        RotarHacia(dirPlanar);
//        transform.Translate(Vector3.forward * velocidadVuelo * Time.deltaTime);

//        // al alcanzar la distancia de picada:
//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;  // fija la posición actual del jugador

//            // crea aviso en la altura configurada
//            if (prefabAviso != null)
//            {
//                Vector3 posAviso = new Vector3(objetivoPicada.x, alturaPrefabY, objetivoPicada.z);
//                avisoInstancia = Instantiate(prefabAviso, posAviso, Quaternion.identity);
//            }

//            estadoActual = Estado.Avisando;
//            StartCoroutine(EsperarYCaer());
//        }
//    }

//    IEnumerator EsperarYCaer()
//    {
//        yield return new WaitForSeconds(tiempoAntesDeCaer);
//        estadoActual = Estado.Picando;
//    }

//    void MoverPicada()
//    {
//        Vector3 dir = (objetivoPicada - transform.position).normalized;
//        RotarHacia(dir);
//        transform.position = Vector3.MoveTowards(
//            transform.position,
//            objetivoPicada,
//            velocidadPicada * Time.deltaTime
//        );

//        // al llegar al punto de impacto:
//        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
//        {
//            // crea efecto de impacto en la altura configurada
//            if (prefabImpacto != null)
//            {
//                Vector3 posImpacto = new Vector3(objetivoPicada.x, alturaPrefabY, objetivoPicada.z);
//                Instantiate(prefabImpacto, posImpacto, Quaternion.identity);
//            }

//            // destruye el aviso
//            if (avisoInstancia != null)
//                Destroy(avisoInstancia);

//            estadoActual = Estado.EnTierra;
//            StartCoroutine(EsperarEnTierra());
//        }
//    }

//    IEnumerator EsperarEnTierra()
//    {
//        yield return new WaitForSeconds(tiempoEnTierra);
//        estadoActual = Estado.Subiendo;
//    }

//    void MoverSubida()
//    {
//        // sube hasta la altura deseada sobre el jugador
//        float targetY = player.position.y + alturaVuelo;
//        Vector3 destino = new Vector3(transform.position.x, targetY, transform.position.z);
//        Vector3 dir = (destino - transform.position).normalized;
//        RotarHacia(dir);

//        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);

//        // no sobrepasar la altura
//        Vector3 pos = transform.position;
//        pos.y = Mathf.Min(pos.y, targetY);
//        transform.position = pos;

//        if (Mathf.Abs(transform.position.y - targetY) < 0.01f)
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

//    void Flotar()
//    {
//        // oscila verticalmente alrededor de la altura de vuelo
//        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
//        Vector3 pos = transform.position;
//        pos.y = player.position.y + alturaVuelo + offset;
//        transform.position = pos;
//    }

//    void RotarHacia(Vector3 dir)
//    {
//        if (dir == Vector3.zero) return;
//        Quaternion meta = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.Slerp(transform.rotation, meta, rotacionVelocidad * Time.deltaTime);
//    }

//    void OnDestroy()
//    {
//        // al destruirse, también destruye el objeto referenciado
//        if (objetoADestruir != null)
//            Destroy(objetoADestruir);
//    }
//}
