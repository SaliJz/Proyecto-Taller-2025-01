using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemigoRosa : MonoBehaviour
{
    public enum TipoEnemigo { Pistola }

    [Header("Configuraci�n de vida")]
    public float vida = 100f;
    public Slider sliderVida;

    [Header("Da�o BAJO (coincide) por tipo de bala")]
    public float danioBajoPistola = 5f;

    [Header("Da�o ALTO (no coincide) por tipo de bala")]
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
    private TipoColorHDRController colorController;
    private Animator animator;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<MeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        // Forzar siempre �Pistola�
        tipo = TipoEnemigo.Pistola;
#if UNITY_EDITOR
        Debug.Log($"[EnemigoRosa] Tipo: {tipo}");
#endif

        // Seleccionamos el color HDR de Pistola
        Color finalColor = hdrColorPistola;
        foreach (var mr in meshRenderers)
        {
            Material mat = mr.material;

            // Base Color (seg�n shader)
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
    /// Aplica da�o directo (e.j., por explosi�n o efecto) y dispara parpadeo HDR.
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
    /// Maneja el da�o proveniente de una bala del jugador.
    /// Ahora s� recibe da�o con todas las balas, incluyendo Pistola.
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
    {
        float d;

        // Determinar da�o seg�n el tipo de bala
        switch (tb)
        {
            case BalaPlayer.TipoBala.Pistola:
                d = danioBajoPistola;
                break;
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

        //if (TutorialManager.Instance != null)
        //{
        //    int index = TutorialManager.Instance.currentDialogue;
        //    if (TutorialManager.Instance.GetCurrentSceneActivationType() == ActivationType.ByKills)
        //    {
        //        TutorialManager.Instance.ScenarioActivationCheckerByKills();
        //    }
        //}

        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
            Instantiate(prefabsAlMorir[UnityEngine.Random.Range(0, prefabsAlMorir.Length)],
                        transform.position, transform.rotation);

        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

        StartCoroutine(TimeToDead());
    }
    IEnumerator TimeToDead()
    {
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        nav.enabled = false;
        if (animator != null) animator.SetBool("isDead", true);
        yield return new WaitForSeconds(2f);
        if (animator != null) animator.SetBool("Die", true);
        Destroy(gameObject);
    }
}

