using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class EnemigoPistolaTutorial : MonoBehaviour
{
    public enum TipoEnemigo { Pistola }

    [Header("Configuración de vida")]
    public float vida = 100f;
    public Slider sliderVida;

    [Header("Daño BAJO (coincide) por tipo de bala")]
    public float danioBajoPistola = 5f;

    [Header("Daño ALTO (no coincide) por tipo de bala")]
    public float danioAltoAmetralladora = 20f;
    public float danioAltoEscopeta = 20f;

    [Header("Multiplicador por headshot")]
    public float headshotMultiplier = 2f;

    [Header("Colliders")]
    [Tooltip("Collider que corresponde a la cabeza")]
    public Collider headCollider;
    [Tooltip("Colliders que corresponden al cuerpo")]
    public Collider[] bodyColliders;

    [Header("Colores HDR")]
    [ColorUsage(true, true)]
    public Color hdrColorPistola = Color.red;

    [Header("Renderizado")]
    public MeshRenderer[] meshRenderers;

    [Header("Prefabs al morir")]
    public GameObject[] prefabsAlMorir;
    public int fragments = 50;

    private TipoEnemigo tipo;
    private bool isDead = false;

    // Referencia al controlador de parpadeo HDR
    private TipoColorHDRController colorController;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();
    }

    void Start()
    {
        // Forzar siempre “Pistola”
        tipo = TipoEnemigo.Pistola;
#if UNITY_EDITOR
        Debug.Log($"[EnemigoPistolaTutorial] Tipo: {tipo}");
#endif

        // Seleccionamos el color HDR de Pistola
        Color finalColor = hdrColorPistola;

        foreach (var mr in meshRenderers)
        {
            Material mat = mr.material;

            // Base Color (según shader)
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

    /// <summary>
    /// Aplica daño directo (e.j., por explosión o efecto) y dispara parpadeo HDR.
    /// </summary>
    public void RecibirDanio(float d)
    {
        if (isDead) return;

        // Disparar parpadeo HDR
        if (colorController != null)
            colorController.RecibirDanio(d);

        // Aplicar resta de vida
        vida -= d;
        if (sliderVida != null) sliderVida.value = vida;
        if (vida <= 0f)
        {
            vida = 0f;
            Morir();
        }
    }

    /// <summary>
    /// Maneja el daño proveniente de una bala del jugador.
    /// Si la bala es de tipo Pistola, no recibe daño.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
    {
        // Si la bala es Pistola (único tipo), no hay daño ni parpadeo
        if (tb == BalaPlayer.TipoBala.Pistola)
        {
            return;
        }

        // Si no coincide, calculamos el daño “alto”
        float d;
        switch (tb)
        {
            case BalaPlayer.TipoBala.Ametralladora:
                d = danioAltoAmetralladora;
                break;
            case BalaPlayer.TipoBala.Escopeta:
                d = danioAltoEscopeta;
                break;
            default:
                d = 0f;
                break;
        }

        // Si headshot, multiplicamos
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
