using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemigoVolador : MonoBehaviour
{
    [Header("Referencias")]
    private Transform player;
    public GameObject objetoADestruir;

    [Header("Prefabs de efectos")]
    public GameObject prefabAviso;
    public GameObject prefabImpacto;

    [Header("Velocidades")]
    public float velocidadVuelo = 5f;
    public float velocidadPicada = 10f;
    public float velocidadSubida = 5f;

    [Header("Distancias")]
    public float distanciaPicada = 10f;

    [Header("Temporización")]
    public float tiempoEspera = 1f;
    public float tiempoAntesDeCaer = 1f;
    public float tiempoEnTierra = 1f;
    public float rotacionVelocidad = 2f;

    [Header("Inteligencia de Altura")]
    public float alturaSobreJugador = 5f;
    public float distanciaBajoTecho = 1f;
    public float suavizadoAltura = 2f;
    public float maxDistanciaRaycast = 50f;
    public LayerMask layerTechoVolador;

    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
    private Estado estadoActual = Estado.Acercarse;

    private Vector3 objetivoPicada;
    private GameObject avisoInstancia;
    private List<GameObject> instanciasEfectos = new List<GameObject>();
    private EnemyAbilityReceiver abilityReceiver;

    Animator animator;


    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        if (abilityReceiver == null)
            Debug.LogWarning("No se encontró EnemyAbilityReceiver en " + name);
        animator = GetComponentInChildren<Animator>();

        var jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador != null)
            player = jugador.transform;
        else
        {
            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
            enabled = false;
            return;
        }
    }

    void Update()
    {
        switch (estadoActual)
        {
            case Estado.Acercarse:
                MoverAcercarse();
                if (animator != null)
                {
                    animator.SetBool("isMoving", true);
                }
                break;
            case Estado.Avisando:
                MirarAlPlayer();
                break;
            case Estado.Picando:
                MoverPicada();
                if (animator != null) animator.SetBool("isMoving", false);
                if (animator != null) animator.SetTrigger("isAttack");
                break;
            case Estado.EnTierra:
                MirarAlPlayer();
                break;
            case Estado.Subiendo:
                MoverSubida();
                break;
            case Estado.Esperando:
                MirarAlPlayer();
                AjustarAlturaFluida();
                break;
        }
    }

    void MoverAcercarse()
    {
        float distXZ = Vector2.Distance(
            new Vector2(transform.position.x, transform.position.z),
            new Vector2(abilityReceiver.CurrentTarget.position.x, abilityReceiver.CurrentTarget.position.z)
        );

        Vector3 planarDest = new Vector3(abilityReceiver.CurrentTarget.position.x, transform.position.y, abilityReceiver.CurrentTarget.position.z);
        Vector3 dirPlanar = (planarDest - transform.position).normalized;
        RotarHacia(dirPlanar);

        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
        transform.Translate(Vector3.forward * speed * Time.deltaTime);

        AjustarAlturaFluida();
        if (distXZ <= distanciaPicada)
        {
            objetivoPicada = abilityReceiver.CurrentTarget.position;
            if (prefabAviso)
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
        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetTrigger("isAttack");
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
            if (prefabImpacto)
            {
                instanciasEfectos.Add(Instantiate(prefabImpacto, objetivoPicada, Quaternion.identity));
            }

            // Destruir el aviso al crear el impacto
            if (avisoInstancia != null)
            {
                Destroy(avisoInstancia);
                avisoInstancia = null;
            }

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
        float yObjetivo = abilityReceiver.CurrentTarget.position.y + alturaSobreJugador;
        if (Physics.Raycast(transform.position, Vector3.up, out var hit, maxDistanciaRaycast, layerTechoVolador))
        {
            yObjetivo = Mathf.Min(yObjetivo, hit.point.y - distanciaBajoTecho);
        }

        float paso = velocidadSubida * Time.deltaTime;
        float yNueva = Mathf.MoveTowards(transform.position.y, yObjetivo, paso);
        transform.position = new Vector3(transform.position.x, yNueva, transform.position.z);

        if (Mathf.Abs(yNueva - yObjetivo) < 0.01f)
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
        Vector3 dir = (abilityReceiver.CurrentTarget.position - transform.position).normalized;
        RotarHacia(dir);
    }

    void AjustarAlturaFluida()
    {
        float yObjetivo = abilityReceiver.CurrentTarget.position.y + alturaSobreJugador;
        if (Physics.Raycast(transform.position, Vector3.up, out var hit, maxDistanciaRaycast, layerTechoVolador))
            yObjetivo = Mathf.Min(yObjetivo, hit.point.y - distanciaBajoTecho);

        Vector3 destino = new Vector3(transform.position.x, yObjetivo, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * suavizadoAltura);
    }

    void RotarHacia(Vector3 dir)
    {
        if (dir == Vector3.zero) return;
        Quaternion meta = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, meta, rotacionVelocidad * Time.deltaTime);
    }

    void OnDestroy()
    {
        if (objetoADestruir != null)
            Destroy(objetoADestruir);

        foreach (var e in instanciasEfectos)
            if (e != null)
                Destroy(e);
    }
}
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencias")]
//    private Transform player;
//    public GameObject objetoADestruir;

//    [Header("Prefabs de efectos")]
//    public GameObject prefabAviso;
//    public GameObject prefabImpacto;

//    [Header("Velocidades")]
//    public float velocidadVuelo = 5f;
//    public float velocidadPicada = 10f;
//    public float velocidadSubida = 5f;

//    [Header("Distancias")]
//    public float distanciaPicada = 10f;

//    [Header("Temporización")]
//    public float tiempoEspera = 1f;
//    public float tiempoAntesDeCaer = 1f;
//    public float tiempoEnTierra = 1f;
//    public float rotacionVelocidad = 2f;

//    [Header("Inteligencia de Altura")]
//    public float alturaSobreJugador = 5f;
//    public float distanciaBajoTecho = 1f;
//    public float suavizadoAltura = 2f;
//    public float maxDistanciaRaycast = 50f;
//    public LayerMask layerTechoVolador;

//    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;

//    private Vector3 objetivoPicada;
//    private GameObject avisoInstancia;
//    private List<GameObject> instanciasEfectos = new List<GameObject>();
//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        if (abilityReceiver == null)
//            Debug.LogWarning("No se encontró EnemyAbilityReceiver en " + name);

//        var jugador = GameObject.FindGameObjectWithTag("Player");
//        if (jugador != null)
//            player = jugador.transform;
//        else
//        {
//            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
//            enabled = false;
//            return;
//        }
//    }

//    void Update()
//    {
//        switch (estadoActual)
//        {
//            case Estado.Acercarse:
//                MoverAcercarse();
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
//                AjustarAlturaFluida();
//                break;
//        }
//    }

//    void MoverAcercarse()
//    {
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        Vector3 planarDest = new Vector3(player.position.x, transform.position.y, player.position.z);
//        Vector3 dirPlanar = (planarDest - transform.position).normalized;
//        RotarHacia(dirPlanar);

//        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
//        transform.Translate(Vector3.forward * speed * Time.deltaTime);

//        AjustarAlturaFluida();

//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;
//            if (prefabAviso)
//                instanciasEfectos.Add(Instantiate(prefabAviso, objetivoPicada, Quaternion.identity));

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
//            if (prefabImpacto)
//                instanciasEfectos.Add(Instantiate(prefabImpacto, objetivoPicada, Quaternion.identity));

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
//        // 1) Altura objetivo sobre el jugador
//        float yObjetivo = player.position.y + alturaSobreJugador;

//        // 2) Si detecta techo encima, no superarlo
//        if (Physics.Raycast(transform.position, Vector3.up, out var hit, maxDistanciaRaycast, layerTechoVolador))
//        {
//            yObjetivo = Mathf.Min(yObjetivo, hit.point.y - distanciaBajoTecho);
//        }

//        // 3) Ascenso suave hasta esa altura
//        float paso = velocidadSubida * Time.deltaTime;
//        float yNueva = Mathf.MoveTowards(transform.position.y, yObjetivo, paso);
//        transform.position = new Vector3(transform.position.x, yNueva, transform.position.z);

//        // 4) Al llegar, paso a Esperando
//        if (Mathf.Abs(yNueva - yObjetivo) < 0.01f)
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

//    void AjustarAlturaFluida()
//    {
//        float yObjetivo = player.position.y + alturaSobreJugador;
//        if (Physics.Raycast(transform.position, Vector3.up, out var hit, maxDistanciaRaycast, layerTechoVolador))
//            yObjetivo = Mathf.Min(yObjetivo, hit.point.y - distanciaBajoTecho);

//        Vector3 destino = new Vector3(transform.position.x, yObjetivo, transform.position.z);
//        transform.position = Vector3.Lerp(destino, transform.position, Time.deltaTime * suavizadoAltura);
//    }

//    void RotarHacia(Vector3 dir)
//    {
//        if (dir == Vector3.zero) return;
//        Quaternion meta = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.Slerp(transform.rotation, meta, rotacionVelocidad * Time.deltaTime);
//    }

//    void OnDestroy()
//    {
//        if (objetoADestruir != null)
//            Destroy(objetoADestruir);

//        foreach (var e in instanciasEfectos)
//            if (e != null)
//                Destroy(e);
//    }
//}

//// Extensión auxiliar para ignorar componente Y en un Vector3
//public static class Vector3Extensions
//{
//    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
//}


//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class EnemigoVolador : MonoBehaviour
//{
//    [Header("Referencias")]
//    private Transform player;
//    public GameObject objetoADestruir;

//    [Header("Prefabs de efectos")]
//    public GameObject prefabAviso;
//    public GameObject prefabImpacto;

//    [Header("Velocidades")]
//    public float velocidadVuelo = 5f;
//    public float velocidadPicada = 10f;
//    public float velocidadSubida = 5f;

//    [Header("Distancias")]
//    public float distanciaPicada = 10f;

//    [Header("Temporización")]
//    public float tiempoEspera = 1f;
//    public float tiempoAntesDeCaer = 1f;
//    public float tiempoEnTierra = 1f;
//    public float rotacionVelocidad = 2f;

//    [Header("Inteligencia de Altura")]
//    public float alturaSobreJugador = 5f;
//    public float distanciaBajoTecho = 1f;
//    public float suavizadoAltura = 2f;
//    public float maxDistanciaRaycast = 50f;
//    public LayerMask layerTechoVolador;

//    private float baseHeight;

//    private enum Estado { Acercarse, Avisando, Picando, EnTierra, Subiendo, Esperando }
//    private Estado estadoActual = Estado.Acercarse;

//    private Vector3 objetivoPicada;
//    private GameObject avisoInstancia;
//    private List<GameObject> instanciasEfectos = new List<GameObject>();
//    private EnemyAbilityReceiver abilityReceiver;

//    void Start()
//    {
//        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
//        if (abilityReceiver == null)
//            Debug.LogWarning("No se encontró EnemyAbilityReceiver en " + name);

//        var jugador = GameObject.FindGameObjectWithTag("Player");
//        if (jugador != null)
//            player = jugador.transform;
//        else
//        {
//            Debug.LogError("No se encontró ningún GameObject con tag 'Player'");
//            enabled = false;
//            return;
//        }

//        baseHeight = transform.position.y;
//    }

//    void Update()
//    {
//        switch (estadoActual)
//        {
//            case Estado.Acercarse:
//                MoverAcercarse();
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
//                AjustarAlturaFluida();
//                break;
//        }
//    }

//    void MoverAcercarse()
//    {
//        float distXZ = Vector2.Distance(
//            new Vector2(transform.position.x, transform.position.z),
//            new Vector2(player.position.x, player.position.z)
//        );

//        // Avanza hacia el jugador en XZ
//        Vector3 dirPlanar = ((Vector3)player.position - transform.position).WithY(transform.position.y).normalized;
//        RotarHacia(dirPlanar);

//        float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : velocidadVuelo;
//        transform.Translate(Vector3.forward * speed * Time.deltaTime);

//        // Ajuste inteligente de altura
//        AjustarAlturaFluida();

//        if (distXZ <= distanciaPicada)
//        {
//            objetivoPicada = player.position;
//            if (prefabAviso)
//                instanciasEfectos.Add(Instantiate(prefabAviso, objetivoPicada, Quaternion.identity));

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
//            if (prefabImpacto)
//                instanciasEfectos.Add(Instantiate(prefabImpacto, objetivoPicada, Quaternion.identity));

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
//        float targetY = baseHeight;
//        RotarHacia(Vector3.up);

//        transform.Translate(Vector3.up * velocidadSubida * Time.deltaTime, Space.World);
//        var pos = transform.position;
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

//    void AjustarAlturaFluida()
//    {
//        // 1) Objetivo: siempre por encima del jugador
//        float yObjetivo = player.position.y + alturaSobreJugador;

//        // 2) Detecta el techo más cercano encima de este enemigo
//        if (Physics.Raycast(transform.position, Vector3.up, out var hit, maxDistanciaRaycast, layerTechoVolador))
//        {
//            float yTecho = hit.point.y - distanciaBajoTecho;
//            yObjetivo = Mathf.Min(yObjetivo, yTecho);
//        }

//        // 3) Suaviza la transición
//        Vector3 destino = new Vector3(transform.position.x, yObjetivo, transform.position.z);
//        transform.position = Vector3.Lerp(transform.position, destino, Time.deltaTime * suavizadoAltura);
//    }

//    void RotarHacia(Vector3 dir)
//    {
//        if (dir == Vector3.zero) return;
//        Quaternion meta = Quaternion.LookRotation(dir);
//        transform.rotation = Quaternion.Slerp(transform.rotation, meta, rotacionVelocidad * Time.deltaTime);
//    }

//    void OnDestroy()
//    {
//        if (objetoADestruir != null)
//            Destroy(objetoADestruir);

//        foreach (var efecto in instanciasEfectos)
//            if (efecto != null)
//                Destroy(efecto);
//    }
//}

//// Extensión auxiliar para ignorar el componente Y en un Vector3
//public static class Vector3Extensions
//{
//    public static Vector3 WithY(this Vector3 v, float y) => new Vector3(v.x, y, v.z);
//}


