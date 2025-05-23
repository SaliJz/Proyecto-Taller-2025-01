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

    [Header("Renderizado")]
    public MeshRenderer[] meshRenderers;

    [Header("Prefabs al morir")]
    public GameObject[] prefabsAlMorir;
    public int fragments = 50;

    private TipoEnemigo tipo;
    private bool isDead = false;

    void Awake()
    {
        // Siempre poblamos el array con todos los MeshRenderer hijos
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
    }

    void Start()
    {
        tipo = (TipoEnemigo)Random.Range(0, 3);
#if UNITY_EDITOR
        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
#endif

        Color c = tipo == TipoEnemigo.Ametralladora ? Color.blue
                : tipo == TipoEnemigo.Pistola ? Color.red
                                                : Color.green;
        foreach (var mr in meshRenderers)
            mr.material.color = c;

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
        else if (bodyColliders != null && bodyColliders.Contains(hitCollider))
        {
            // daño normal
        }

        RecibirDanio(d);
    }

    void Morir()
    {
        isDead = true;
        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
            Instantiate(prefabsAlMorir[Random.Range(0, prefabsAlMorir.Length)], transform.position, transform.rotation);

        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

        Destroy(gameObject);
    }
}







