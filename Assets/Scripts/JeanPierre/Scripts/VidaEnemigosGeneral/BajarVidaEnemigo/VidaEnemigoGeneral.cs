using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

    [Header("Configuración de vida")]
    public float vida = 100f;
    public Slider sliderVida;

    [Header("Daño BAJO (coincide) por tipo de bala")]
    public float danioBajoAmetralladora = 5f;
    public float danioBajoPistola = 5f;
    public float danioBajoEscopeta = 5f;

    [Header("Daño ALTO (no coincide) por tipo de bala")]
    public float danioAltoAmetralladora = 20f;
    public float danioAltoPistola = 20f;
    public float danioAltoEscopeta = 20f;

    [Header("Multiplicador por headshot")]
    public float headshotMultiplier = 2f;

    [Header("Colliders")]
    [Tooltip("Collider que corresponde a la cabeza")]
    public Collider headCollider;
    [Tooltip("Colliders que corresponden al cuerpo")]
    public Collider[] bodyColliders;

    [Header("Colores HDR por tipo")]
    [ColorUsage(true, true)]
    public Color hdrColorAmetralladora = Color.blue;
    [ColorUsage(true, true)]
    public Color hdrColorPistola = Color.red;
    [ColorUsage(true, true)]
    public Color hdrColorEscopeta = Color.green;

    [Header("Renderizado")]
    public MeshRenderer[] meshRenderers;

    [Header("Prefabs al morir")]
    public GameObject[] prefabsAlMorir;
    public int fragments = 50;

    private TipoEnemigo tipo;
    private bool isDead = false;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Start()
    {
        tipo = (TipoEnemigo)Random.Range(0, 3);
#if UNITY_EDITOR
        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
#endif

        // Seleccionamos el color HDR directamente
        Color finalColor = tipo == TipoEnemigo.Ametralladora ? hdrColorAmetralladora
                        : tipo == TipoEnemigo.Pistola ? hdrColorPistola
                        : hdrColorEscopeta;

        foreach (var mr in meshRenderers)
        {
            Material mat = mr.material;

            // Base Color
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", finalColor);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", finalColor);

            // Emission Color
            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", finalColor);
        }

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
    }

    public void RecibirDanio(float d)
    {
        if (isDead) return;
        vida -= d;
        if (sliderVida != null) sliderVida.value = vida;
        if (vida <= 0f)
        {
            vida = 0f;
            Morir();
        }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
    {
        float d;
        switch (tb)
        {
            case BalaPlayer.TipoBala.Ametralladora:
                d = (tipo == TipoEnemigo.Ametralladora) ? danioBajoAmetralladora : danioAltoAmetralladora;
                break;
            case BalaPlayer.TipoBala.Pistola:
                d = (tipo == TipoEnemigo.Pistola) ? danioBajoPistola : danioAltoPistola;
                break;
            case BalaPlayer.TipoBala.Escopeta:
                d = (tipo == TipoEnemigo.Escopeta) ? danioBajoEscopeta : danioAltoEscopeta;
                break;
            default:
                d = 0f;
                break;
        }

        if (hitCollider == headCollider)
            d *= headshotMultiplier;

        RecibirDanio(d);
    }

    void Morir()
    {
        if (isDead) return;
        isDead = true;

        TutorialEnemies tutorial = GetComponent<TutorialEnemies>();
        if (tutorial != null)
        {
            foreach (int index in tutorial.IndexScenes)
            {
                TutorialManager.Instance.StartScenarioByKills(index);
            }
        }

        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
            Instantiate(prefabsAlMorir[Random.Range(0, prefabsAlMorir.Length)], transform.position, transform.rotation);

        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

        Destroy(gameObject);
    }
}


