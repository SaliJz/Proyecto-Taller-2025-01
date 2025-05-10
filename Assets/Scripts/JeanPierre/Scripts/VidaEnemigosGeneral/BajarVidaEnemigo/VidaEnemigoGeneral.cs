using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora = 0, Pistola = 1, Escopeta = 2 }

    [SerializeField] private string enemyName = "Enemigo"; // Nombre del enemigo para la mision

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
        Log("[VidaEnemigo] Tipo asignado: " + tipo);

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
            Log("[VidaEnemigo] No se ha asignado el MeshRenderer en el Inspector.");
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

        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.AddInfoFragment(fragments); // Actualiza los fragments de informacion
        }

        else
        {
            Log("[VidaEnemigo] No se ha asignado el HUDManager en el Inspector.");
        }

        if (MissionManager.Instance != null)
        {
            MissionManager.Instance.RegisterKill(gameObject.tag, enemyName, tipo.ToString()); // Actualiza la mision
        }

        Destroy(gameObject);
    }
#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}




