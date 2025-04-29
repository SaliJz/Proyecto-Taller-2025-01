using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

    [Header("Configuracion de vida")]
    public float vida = 100f;
    public Slider sliderVida;

    [Header("Danio por bala")]
    public float danioAlto = 20f;   // Danio si la bala NO coincide con el tipo
    public float danioBajo = 5f;    // Danio si la bala coincide con el tipo

    [Header("Renderizado")]
    [Tooltip("Arrastra aqui el MeshRenderer de tu enemigo")]
    public MeshRenderer meshRenderer;

    [Header("Prefabs al morir")]
    [Tooltip("Arrastra aqui los prefabs que pueden generarse al morir")]
    public GameObject[] prefabsAlMorir;

    [SerializeField] private int fragments = 50; // Fragmentos de informacion que suelta el enemigo
    private bool isDead = false; // Para evitar multiples muertes

    private TipoEnemigo tipo;

    void Start()
    {
        // 1) Elegir tipo al azar
        tipo = (TipoEnemigo)Random.Range(0, 3);
        Debug.Log("[VidaEnemigo] Tipo asignado: " + tipo);

        // 2) Determinar color segun el tipo
        Color colorAsignado = Color.white;
        switch (tipo)
        {
            case TipoEnemigo.Ametralladora:
                colorAsignado = Color.blue;
                break;
            case TipoEnemigo.Pistola:
                colorAsignado = Color.red;
                break;
            case TipoEnemigo.Escopeta:
                colorAsignado = Color.green;
                break;
        }

        // 3) Aplicar el color al MeshRenderer asignado
        if (meshRenderer != null)
        {
            meshRenderer.material.color = colorAsignado;
        }
        else
        {
            Debug.LogWarning("[VidaEnemigo] No se ha asignado el MeshRenderer en el Inspector.");
        }

        // 4) Inicializar slider de vida
        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

    /// <summary>
    /// Aplica danio generico y actualiza el slider.
    /// </summary>
    public void RecibirDanio(float danio)
    {
        if (isDead) return; // No hacer nada si ya esta muerto

        vida -= danio;
        if (sliderVida != null)
            sliderVida.value = vida;

        if (vida <= 0f)
        {
            vida = 0f;
            Morir();
        }
    }

    /// <summary>
    /// Aplica danio segun el tipo de bala (usa enum)
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {
        float danioAAplicar = danioAlto;

        // Si el enum coincide con el tipo de enemigo, aplicamos danio bajo
        if ((tipo == TipoEnemigo.Ametralladora && tipoBala == BalaPlayer.TipoBala.Ametralladora) ||
            (tipo == TipoEnemigo.Pistola && tipoBala == BalaPlayer.TipoBala.Pistola) ||
            (tipo == TipoEnemigo.Escopeta && tipoBala == BalaPlayer.TipoBala.Escopeta))
        {
            danioAAplicar = danioBajo;
        }

        RecibirDanio(danioAAplicar);
    }

    /// <summary>
    /// Maneja la muerte: instancia prefab (si hay) y destruye el objeto.
    /// </summary>
    void Morir()
    {
        if (!isDead) isDead = true;

        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
        {
            int idx = Random.Range(0, prefabsAlMorir.Length);
            Instantiate(prefabsAlMorir[idx], transform.position, transform.rotation);
        }

        HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragments de informacion
        FindObjectOfType<MissionManager>().RegisterKill(gameObject.tag); // Actualiza la mision

        Destroy(gameObject);
    }
}







//using UnityEngine;
//using UnityEngine.UI;

//public class VidaEnemigoGeneral : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

//    [Header("Configuraci�n de vida")]
//    public float vida = 100f;
//    public Slider sliderVida;

//    [Header("Da�o por bala")]
//    public float danioAlto = 20f;   // Da�o si la bala NO coincide con el tipo
//    public float danioBajo = 5f;    // Da�o si la bala coincide con el tipo

//    [Header("Renderizado")]
//    [Tooltip("Arrastra aqu� el MeshRenderer de tu enemigo")]
//    public MeshRenderer meshRenderer;

//    [Header("Prefabs al morir")]
//    [Tooltip("Arrastra aqu� los prefabs que pueden generarse al morir")]
//    public GameObject[] prefabsAlMorir;

//    // Nuevo agregado
//    [SerializeField] private int fragments = 50; // Fragmentos de informaci�n que suelta el enemigo
//    private bool isDead = false; // Para evitar m�ltiples muertes

//    private TipoEnemigo tipo;

//    void Start()
//    {
//        // 1) Elegir tipo al azar
//        tipo = (TipoEnemigo)Random.Range(0, 3);
//        Debug.Log("[VidaEnemigo] Tipo asignado: " + tipo);

//        // 2) Determinar color seg�n el tipo
//        Color colorAsignado = Color.white;
//        switch (tipo)
//        {
//            case TipoEnemigo.Ametralladora:
//                colorAsignado = Color.blue;
//                break;
//            case TipoEnemigo.Pistola:
//                colorAsignado = Color.red;
//                break;
//            case TipoEnemigo.Escopeta:
//                colorAsignado = Color.green;
//                break;
//        }

//        // 3) Aplicar el color al MeshRenderer asignado
//        if (meshRenderer != null)
//        {
//            meshRenderer.material.color = colorAsignado;
//        }
//        else
//        {
//            Debug.LogWarning("[VidaEnemigo] No se ha asignado el MeshRenderer en el Inspector.");
//        }

//        // 4) Inicializar slider de vida
//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }
//    }

//    /// <summary>
//    /// Aplica da�o gen�rico y actualiza el slider.
//    /// </summary>
//    public void RecibirDanio(float danio)
//    {
//        if (isDead) return; // No hacer nada si ya est� muerto

//        vida -= danio;
//        if (sliderVida != null)
//            sliderVida.value = vida;

//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    /// <summary>
//    /// Aplica da�o seg�n el tipo de bala (usa tags).
//    /// </summary>
//    public void RecibirDanioPorBala(string tagBala)
//    {
//        float danioAAplicar = danioAlto;

//        // Si el tag coincide con el tipo de enemigo, aplicamos da�o bajo
//        if ((tipo == TipoEnemigo.Ametralladora && tagBala == "BalaAmetralladora") ||
//            (tipo == TipoEnemigo.Pistola && tagBala == "BalaPistola") ||
//            (tipo == TipoEnemigo.Escopeta && tagBala == "BalaEscopeta"))
//        {
//            danioAAplicar = danioBajo;
//        }

//        RecibirDanio(danioAAplicar);
//    }

//    /// <summary>
//    /// Maneja la muerte: instancia prefab (si hay) y destruye el objeto.
//    /// </summary>
//    void Morir()
//    {
//        // Ya fue marcado como muerto en RecibirDanio, as� que esta verificaci�n puede quedar opcional
//        if (isDead == false) isDead = true;

//        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
//        {
//            int idx = Random.Range(0, prefabsAlMorir.Length);
//            Instantiate(prefabsAlMorir[idx], transform.position, transform.rotation);
//        }

//        HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragmentos de informaci�n
//        FindObjectOfType<MissionManager>().RegisterKill(gameObject.tag); // Actualiza la misi�n

//        Destroy(gameObject);
//    }
//}


