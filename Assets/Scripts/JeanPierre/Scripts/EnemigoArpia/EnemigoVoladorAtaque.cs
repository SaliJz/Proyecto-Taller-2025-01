using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class EnemigoVolador : MonoBehaviour
{
    [Header("Referencias")]
    private Transform player;               // se captura automáticamente por tag
    public GameObject objetoADestruir;      // otro GameObject a destruir cuando este muera

    [Header("Prefabs de efectos")]
    public GameObject prefabAviso;          // aviso antes de la caída
    public GameObject prefabImpacto;        // efecto al impactar

    [Header("Velocidades")]
    public float velocidadVuelo = 5f;       // velocidad al acercarse
    public float velocidadPicada = 10f;     // velocidad durante la picada
    public float velocidadSubida = 5f;      // velocidad al volver a altura

    [Header("Distancias y Alturas")]
    public float distanciaPicada = 10f;     // distancia horizontal para activar la picada
    public float alturaVuelo = 20f;         // altura deseada sobre el jugador

    [Header("Temporización")]
    public float tiempoEspera = 1f;         // espera tras subir antes de reanudar
    public float tiempoAntesDeCaer = 1f;    // tiempo entre aviso y picada
    public float tiempoEnTierra = 1f;       // tiempo que permanece en la posición de impacto
    public float rotacionVelocidad = 2f;    // rapidez de rotación

    [Header("Flotación")]
    public float amplitudFlotacion = 0.5f;  // amplitud de oscilación vertical
    public float frecuenciaFlotacion = 1f;  // frecuencia de oscilación

    [Header("Colisión con techos")]
    public LayerMask ceilingMask;           // capa(s) de los techos
    public float ceilingClearance = 0.5f;   // espacio mínimo bajo el techo

    [Header("Colisión con suelos")]
    public LayerMask floorMask;             // capa(s) del suelo
    public float floorClearance = 0.1f;     // espacio mínimo sobre el suelo

    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
    private Estado estadoActual = Estado.Acercarse;

    private Vector3 objetivoPicada;         // posición de impacto
    private GameObject avisoInstancia;      // instancia del prefabAviso
    private List<GameObject> instanciasEfectos = new List<GameObject>();

    private EnemyAbilityReceiver abilityReceiver;

    void Start()
    {
        // referencias
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        if (abilityReceiver == null)
            Debug.LogWarning("No se encontró EnemyAbilityReceiver en " + name);

        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
            enabled = false;
            return;
        }
        player = jugador.transform;

        // colocación inicial (entre suelo y techo)
        float desiredY = player.position.y + alturaVuelo;
        float minY = GetMinimumHeight();
        float maxY = GetMaximumHeight();
        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(desiredY, minY, maxY);
        transform.position = pos;
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
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(player.position.x, player.position.z)
        );

        // volar hacia la proyección del jugador en XZ
        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
        RotarHacia((destinoPlanar - transform.position).normalized);

        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        if (distXZ <= distanciaPicada)
        {
            // calculamos punto de impacto real en el suelo
            objetivoPicada = GetImpactPoint(player.position);

            // aviso en el suelo
            if (prefabAviso != null)
            {
                avisoInstancia = Instantiate(prefabAviso, objetivoPicada, Quaternion.identity);
                instanciasEfectos.Add(avisoInstancia);
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
        RotarHacia((objetivoPicada - transform.position).normalized);
        transform.position = Vector3.MoveTowards(
            transform.position,
            objetivoPicada,
            velocidadPicada * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
        {
            // efecto de impacto
            if (prefabImpacto != null)
            {
                var imp = Instantiate(prefabImpacto, objetivoPicada, Quaternion.identity);
                instanciasEfectos.Add(imp);
            }
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
        float maxY = GetMaximumHeight();
        float minY = GetMinimumHeight();
        Vector3 pos = transform.position;
        float targetY = player.position.y + alturaVuelo;
        // subir sin pasar techo ni hundirse
        pos.y = Mathf.MoveTowards(pos.y, targetY, velocidadSubida * Time.deltaTime);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        transform.position = pos;

        if (Mathf.Abs(pos.y - targetY) < 0.01f)
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
        RotarHacia((player.position - transform.position).normalized);
    }

    void Flotar()
    {
        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
        float desiredY = player.position.y + alturaVuelo + offset;
        float minY = GetMinimumHeight();
        float maxY = GetMaximumHeight();

        Vector3 pos = transform.position;
        pos.y = Mathf.Clamp(desiredY, minY, maxY);
        transform.position = pos;
    }

    Vector3 GetImpactPoint(Vector3 horizontalPos)
    {
        // raycast desde arriba hacia abajo para encontrar suelo real
        float originY = player.position.y + alturaVuelo + ceilingClearance + 1f;
        Vector3 origen = new Vector3(horizontalPos.x, originY, horizontalPos.z);
        float maxDist = alturaVuelo + ceilingClearance + 2f;

        if (Physics.Raycast(origen, Vector3.down, out RaycastHit hit, maxDist, floorMask))
            return new Vector3(horizontalPos.x, hit.point.y + floorClearance, horizontalPos.z);

        // fallback: sobre el jugador
        return new Vector3(horizontalPos.x, player.position.y + floorClearance, horizontalPos.z);
    }

    float GetMaximumHeight()
    {
        // techo: raycast desde punto deseado hacia arriba
        float desiredY = player.position.y + alturaVuelo;
        Vector3 origin = new Vector3(player.position.x, desiredY + 0.1f, player.position.z);

        if (Physics.Raycast(origin, Vector3.up, out RaycastHit hit, Mathf.Infinity, ceilingMask))
            return hit.point.y - ceilingClearance;

        return desiredY;
    }

    float GetMinimumHeight()
    {
        // suelo: raycast desde el jugador hacia abajo
        Vector3 origin = player.position + Vector3.up * 0.1f;
        float maxDist = alturaVuelo + 10f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, maxDist, floorMask))
            return hit.point.y + floorClearance;

        return player.position.y;
    }

    void RotarHacia(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotacionVelocidad * Time.deltaTime);
    }

    void OnDestroy()
    {
        if (objetoADestruir != null)
            Destroy(objetoADestruir);
        foreach (var fx in instanciasEfectos)
            if (fx != null)
                Destroy(fx);
    }
}













// arriba

//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencias")]
//    private Transform player;               // se captura automáticamente por tag
//    public GameObject objetoADestruir;      // otro GameObject a destruir cuando este muera

//    [Header("Prefabs de efectos")]
//    public GameObject prefabAviso;          // aviso antes de la caída
//    public GameObject prefabImpacto;        // efecto al impactar

//    [Header("Velocidades")]
//    public float velocidadVuelo = 5f;       // velocidad al acercarse
//    public float velocidadPicada = 10f;     // velocidad durante la picada
//    public float velocidadSubida = 5f;      // velocidad al volver a altura

//    [Header("Distancias y Alturas")]
//    public float distanciaPicada = 10f;     // distancia horizontal para activar la picada
//    public float alturaVuelo = 20f;         // altura deseada sobre el jugador

//    [Header("Temporización")]
//    public float tiempoEspera = 1f;         // espera tras subir antes de reanudar
//    public float tiempoAntesDeCaer = 1f;    // tiempo entre aviso y picada
//    public float tiempoEnTierra = 1f;       // tiempo que permanece en la posición de impacto
//    public float rotacionVelocidad = 2f;    // rapidez de rotación

//    [Header("Flotación")]
//    public float amplitudFlotacion = 0.5f;  // amplitud de oscilación vertical
//    public float frecuenciaFlotacion = 1f;  // frecuencia de oscilación

//    [Header("Colisión con techos")]
//    public LayerMask ceilingMask;           // capa(s) de los techos
//    public float clearance = 0.5f;          // espacio mínimo bajo el techo

//    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;

//    private Vector3 objetivoPicada;         // posición capturada para la picada
//    private GameObject avisoInstancia;      // instancia del prefabAviso

//    // Lista para almacenar todas las instancias creadas (aviso e impacto)
//    private List<GameObject> instanciasEfectos = new List<GameObject>();

//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        // referenciar componentes
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        if (abilityReceiver == null)
//            Debug.LogWarning("No se encontró EnemyAbilityReceiver en " + name);

//        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
//        if (jugador == null)
//        {
//            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
//            enabled = false;
//            return;
//        }
//        player = jugador.transform;

//        // colocación inicial, respetando techos
//        Vector3 pos = transform.position;
//        pos.y = GetAllowedHeight();
//        transform.position = pos;
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
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        // volar hacia la proyección del jugador (en XZ)
//        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
//        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
//        RotarHacia(dirPlanar);

//        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
//        transform.Translate(Vector3.forward * speed * Time.deltaTime);

//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;
//            if (prefabAviso != null)
//            {
//                avisoInstancia = Instantiate(prefabAviso, objetivoPicada, Quaternion.identity);
//                instanciasEfectos.Add(avisoInstancia);
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

//        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
//        {
//            if (prefabImpacto != null)
//            {
//                GameObject imp = Instantiate(prefabImpacto, objetivoPicada, Quaternion.identity);
//                instanciasEfectos.Add(imp);
//            }
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
//        float alturaPermitida = GetAllowedHeight();
//        Vector3 pos = transform.position;
//        pos.y = Mathf.MoveTowards(pos.y, alturaPermitida, velocidadSubida * Time.deltaTime);
//        transform.position = pos;

//        if (Mathf.Abs(pos.y - alturaPermitida) < 0.01f)
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
//        float baseY = player.position.y + alturaVuelo;
//        float offset = Mathf.Sin(Time.time * frecuenciaFlotacion) * amplitudFlotacion;
//        float alturaDeseada = baseY + offset;
//        float alturaPermitida = GetAllowedHeight();

//        Vector3 pos = transform.position;
//        // oscilar siempre por debajo del techo
//        pos.y = Mathf.Min(alturaDeseada, alturaPermitida);
//        transform.position = pos;
//    }

//    float GetAllowedHeight()
//    {
//        // lanzamos el raycast desde la posición del jugador hacia arriba,
//        // con longitud alturaVuelo + clearance
//        Vector3 origen = player.position;
//        float maxDist = alturaVuelo + clearance;

//        if (Physics.Raycast(origen, Vector3.up, out RaycastHit hit, maxDist, ceilingMask))
//        {
//            // devuelve justo por debajo del punto de colisión
//            return hit.point.y - clearance;
//        }

//        // si no hay techo en ese rango, usar la altura nominal
//        return player.position.y + alturaVuelo;
//    }

//    void RotarHacia(Vector3 dir)
//    {
//        if (dir == Vector3.zero) return;
//        Quaternion target = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.Slerp(transform.rotation, target, rotacionVelocidad * Time.deltaTime);
//    }

//    void OnDestroy()
//    {
//        if (objetoADestruir != null)
//            Destroy(objetoADestruir);
//        foreach (var fx in instanciasEfectos)
//            if (fx != null)
//                Destroy(fx);
//    }
//}












// origen


//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencias")]
//    private Transform player;               // ahora se captura automáticamente por tag
//    public GameObject objetoADestruir;      // otro GameObject a destruir cuando este muera

//    [Header("Prefabs de efectos")]
//    public GameObject prefabAviso;          // aviso antes de la caída
//    public GameObject prefabImpacto;        // efecto al impactar

//    [Header("Configuración de posición")]
//    [Tooltip("(Obsoleto) Altura en Y a la que se instancian los prefabs de aviso e impacto")]
//    public float alturaPrefabY = 0f;        // ya no se utilizará para posicionar prefabs

//    [Header("Velocidades")]
//    public float velocidadVuelo = 5f;       // velocidad al acercarse
//    public float velocidadPicada = 10f;     // velocidad durante la picada
//    public float velocidadSubida = 5f;      // velocidad al volver a altura

//    [Header("Distancias y Alturas")]
//    public float distanciaPicada = 10f;     // distancia horizontal para activar la picada
//    public float alturaVuelo = 20f;         // altura sobre el jugador

//    [Header("Temporización")]
//    public float tiempoEspera = 1f;         // espera tras subir antes de reanudar
//    public float tiempoAntesDeCaer = 1f;    // tiempo entre aviso y picada
//    public float tiempoEnTierra = 1f;       // tiempo que permanece en la posición de impacto
//    public float rotacionVelocidad = 2f;    // rapidez de rotación

//    [Header("Flotación")]
//    public float amplitudFlotacion = 0.5f;  // amplitud de oscilación vertical
//    public float frecuenciaFlotacion = 1f;  // frecuencia de oscilación

//    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;

//    private Vector3 objetivoPicada;         // posición capturada para la picada
//    private GameObject avisoInstancia;      // instancia del prefabAviso

//    // Lista para almacenar todas las instancias creadas (aviso e impacto)
//    private List<GameObject> instanciasEfectos = new List<GameObject>();

//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        if (abilityReceiver == null)
//        {
//            Debug.LogWarning("No se encontró el componente EnemyAbilityReceiver en " + gameObject.name);
//        }

//        // Capturamos al jugador automáticamente por tag
//        GameObject jugador = GameObject.FindGameObjectWithTag("Player");
//        if (jugador != null)
//        {
//            player = jugador.transform;
//        }
//        else
//        {
//            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
//            enabled = false;
//            return;
//        }

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
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        // volar hacia la proyección del jugador
//        Vector3 destinoPlanar = new Vector3(player.position.x, transform.position.y, player.position.z);
//        Vector3 dirPlanar = (destinoPlanar - transform.position).normalized;
//        RotarHacia(dirPlanar);

//        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
//        transform.Translate(Vector3.forward * speed * Time.deltaTime);

//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;  // fija la posición actual del jugador

//            // crea aviso en la posición del jugador
//            if (prefabAviso != null)
//            {
//                Vector3 posAviso = player.position;
//                avisoInstancia = Instantiate(prefabAviso, posAviso, Quaternion.identity);
//                instanciasEfectos.Add(avisoInstancia);
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

//        if (Vector3.Distance(transform.position, objetivoPicada) < 0.1f)
//        {
//            // crea efecto de impacto en el punto de picada
//            if (prefabImpacto != null)
//            {
//                Vector3 posImpacto = objetivoPicada;
//                GameObject impacto = Instantiate(prefabImpacto, posImpacto, Quaternion.identity);
//                instanciasEfectos.Add(impacto);
//            }

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
//        float targetY = player.position.y + alturaVuelo;
//        Vector3 destino = new Vector3(transform.position.x, targetY, transform.position.z);
//        Vector3 dir = (destino - transform.position).normalized;
//        RotarHacia(dir);

//        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);

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
//        // Destruye el otro objeto referenciado si existe
//        if (objetoADestruir != null)
//            Destroy(objetoADestruir);

//        // Destruye inmediatamente todas las instancias de efectos creados
//        foreach (var efecto in instanciasEfectos)
//        {
//            if (efecto != null)
//                Destroy(efecto);
//        }
//    }
//}

