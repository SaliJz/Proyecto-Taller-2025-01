using System;
using System.Linq;
using System.Collections;
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
    public SkinnedMeshRenderer[] meshRenderers;

    [Header("Prefabs al morir por tipo")]
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Ametralladora")]
    public GameObject prefabMuerteAmetralladora;
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Pistola")]
    public GameObject prefabMuertePistola;
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Escopeta")]
    public GameObject prefabMuerteEscopeta;

    [Header("Fragmentos y HUD")]
    public int fragments = 50;

    private TipoEnemigo tipo;
    private bool isDead = false;

    // Color final que se determinó al inicio y que no debe cambiar
    private Color finalColor;

    // Referencia al controlador de parpadeo HDR
    private TipoColorHDRController colorController;

    Animator animator;
    void Awake()
    {
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();
        animator = GetComponentInChildren<Animator>();

    }

    void Start()
    {
        // 1) Determinamos tipo al azar y su color HDR
        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
#if UNITY_EDITOR
        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
#endif
        finalColor = ObtenerColorPorTipo(tipo);

        // 2) Asignamos Color y Emission a todos los materiales existentes
        AsignarColorYEmissionAMateriales(finalColor);

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }

        // 3) Durante los próximos 0.5 seg, detectamos si aparecen nuevos materiales
        StartCoroutine(DetectarYAsignarNuevosMateriales(finalColor, 0.5f));
    }

    void Update()
    {
        // Reaplicamos el color en cada frame para evitar que Unity lo sobreescriba:
        ReaplicarColorYEmissionAMateriales(finalColor);
    }

    private Color ObtenerColorPorTipo(TipoEnemigo t)
    {
        switch (t)
        {
            case TipoEnemigo.Ametralladora:
                return hdrColorAmetralladora;
            case TipoEnemigo.Pistola:
                return hdrColorPistola;
            case TipoEnemigo.Escopeta:
                return hdrColorEscopeta;
            default:
                return Color.white;
        }
    }

    private void AsignarColorYEmissionAMateriales(Color color)
    {
        foreach (var mr in meshRenderers)
        {
            Material[] mats = mr.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);

                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", color);
            }
            mr.materials = mats;
        }
    }

    private void ReaplicarColorYEmissionAMateriales(Color color)
    {
        foreach (var mr in meshRenderers)
        {
            var mats = mr.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];

                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);

                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", color);
            }
            mr.materials = mats;
        }
    }

    private IEnumerator DetectarYAsignarNuevosMateriales(Color color, float duracion)
    {
        float timer = 0f;
        var conteoInicial = meshRenderers.ToDictionary(
            mr => mr,
            mr => mr.materials.Length
        );

        while (timer < duracion)
        {
            foreach (var mr in meshRenderers)
            {
                Material[] currentMats = mr.materials;
                int inicial = conteoInicial[mr];
                if (currentMats.Length > inicial)
                {
                    for (int i = inicial; i < currentMats.Length; i++)
                    {
                        Material mat = currentMats[i];

                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", color);
                        else if (mat.HasProperty("_Color"))
                            mat.SetColor("_Color", color);

                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                            mat.SetColor("_EmissionColor", color);
                    }
                    conteoInicial[mr] = currentMats.Length;
                    mr.materials = currentMats;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void RecibirDanio(float d)
    {
        if (isDead) return;
        if (colorController != null)
            colorController.RecibirDanio(d);

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
        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
            (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
        {
            return;
        }

        float d;
        switch (tb)
        {
            case BalaPlayer.TipoBala.Ametralladora:
                d = danioAltoAmetralladora;
                break;
            case BalaPlayer.TipoBala.Pistola:
                d = danioAltoPistola;
                break;
            case BalaPlayer.TipoBala.Escopeta:
                d = danioAltoEscopeta;
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

        // Lógica de tutorial
        if (TutorialManager.Instance != null)
        {
            int index = TutorialManager.Instance.currentIndex;
            if (TutorialManager.Instance.scenes[index].sceneData.activationType == ActivationType.ByKills)
            {
                TutorialManager.Instance.StartScenarioByKills(index);
            }
        }

        // Instanciar el prefab correspondiente al tipo
        GameObject prefabAMorir = null;
        switch (tipo)
        {
            case TipoEnemigo.Ametralladora:
                prefabAMorir = prefabMuerteAmetralladora;
                break;
            case TipoEnemigo.Pistola:
                prefabAMorir = prefabMuertePistola;
                break;
            case TipoEnemigo.Escopeta:
                prefabAMorir = prefabMuerteEscopeta;
                break;
        }

        if (prefabAMorir != null)
            Instantiate(prefabAMorir, transform.position, transform.rotation);

        // Fragmentos y registro de muerte
        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());
        StartCoroutine(TimeToDead());
    }

    IEnumerator TimeToDead()
    {
        animator.SetBool("isDead", true);
        yield return new WaitForSeconds(2);
        Destroy(gameObject);
    }
}



//using System;
//using System.Linq;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public class VidaEnemigoGeneral : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración de vida")]
//    public float vida = 100f;
//    public Slider sliderVida;

//    [Header("Daño BAJO (coincide) por tipo de bala")]
//    public float danioBajoAmetralladora = 5f;
//    public float danioBajoPistola = 5f;
//    public float danioBajoEscopeta = 5f;

//    [Header("Daño ALTO (no coincide) por tipo de bala")]
//    public float danioAltoAmetralladora = 20f;
//    public float danioAltoPistola = 20f;
//    public float danioAltoEscopeta = 20f;

//    [Header("Multiplicador por headshot")]
//    public float headshotMultiplier = 2f;

//    [Header("Colliders")]
//    [Tooltip("Collider que corresponde a la cabeza")]
//    public Collider headCollider;
//    [Tooltip("Colliders que corresponden al cuerpo")]
//    public Collider[] bodyColliders;

//    [Header("Colores HDR por tipo")]
//    [ColorUsage(true, true)]
//    public Color hdrColorAmetralladora = Color.blue;
//    [ColorUsage(true, true)]
//    public Color hdrColorPistola = Color.red;
//    [ColorUsage(true, true)]
//    public Color hdrColorEscopeta = Color.green;

//    [Header("Renderizado")]
//    public MeshRenderer[] meshRenderers;

//    [Header("Prefabs al morir")]
//    public GameObject[] prefabsAlMorir;
//    public int fragments = 50;

//    private TipoEnemigo tipo;
//    private bool isDead = false;

//    // Color final que se determinó al inicio y que no debe cambiar
//    private Color finalColor;

//    // Referencia al controlador de parpadeo HDR
//    private TipoColorHDRController colorController;

//    void Awake()
//    {
//        meshRenderers = GetComponentsInChildren<MeshRenderer>();
//        colorController = GetComponent<TipoColorHDRController>();
//    }

//    void Start()
//    {
//        // 1) Determinamos tipo al azar y su color HDR
//        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
//#if UNITY_EDITOR
//        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
//#endif
//        finalColor = ObtenerColorPorTipo(tipo);

//        // 2) Asignamos Color y Emission a todos los materiales existentes
//        AsignarColorYEmissionAMateriales(finalColor);

//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }

//        // 3) Durante los próximos 0.5 seg, detectamos si aparecen nuevos materiales
//        StartCoroutine(DetectarYAsignarNuevosMateriales(finalColor, 0.5f));
//    }

//    void Update()
//    {
//        // Reaplicamos el color en cada frame para evitar que Unity lo sobreescriba:
//        ReaplicarColorYEmissionAMateriales(finalColor);
//    }

//    /// <summary>
//    /// Devuelve el color HDR que corresponde al tipo de enemigo.
//    /// </summary>
//    private Color ObtenerColorPorTipo(TipoEnemigo t)
//    {
//        switch (t)
//        {
//            case TipoEnemigo.Ametralladora:
//                return hdrColorAmetralladora;
//            case TipoEnemigo.Pistola:
//                return hdrColorPistola;
//            case TipoEnemigo.Escopeta:
//                return hdrColorEscopeta;
//            default:
//                return Color.white;
//        }
//    }

//    /// <summary>
//    /// Asigna a cada material:
//    ///   - Color (“_BaseColor”) igual al color indicado
//    ///   - Habilita _EMISSION y setea _EmissionColor = color HDR
//    /// </summary>
//    private void AsignarColorYEmissionAMateriales(Color color)
//    {
//        foreach (var mr in meshRenderers)
//        {
//            Material[] mats = mr.materials; // instancia cada material
//            for (int i = 0; i < mats.Length; i++)
//            {
//                Material mat = mats[i];

//                // 1) Asignar Color (tint) en lugar de BaseMap:
//                if (mat.HasProperty("_BaseColor"))
//                    mat.SetColor("_BaseColor", color);
//                else if (mat.HasProperty("_Color"))
//                    mat.SetColor("_Color", color);

//                // 2) Activar emisión HDR
//                mat.EnableKeyword("_EMISSION");
//                if (mat.HasProperty("_EmissionColor"))
//                    mat.SetColor("_EmissionColor", color);
//            }
//            mr.materials = mats;
//        }
//    }

//    /// <summary>
//    /// Reaplica el mismo color y emisión sin reasignar materiales,
//    /// para evitar que Unity los restablezca en algún momento.
//    /// </summary>
//    private void ReaplicarColorYEmissionAMateriales(Color color)
//    {
//        foreach (var mr in meshRenderers)
//        {
//            var mats = mr.materials; // esto ya garantiza instancias
//            for (int i = 0; i < mats.Length; i++)
//            {
//                Material mat = mats[i];

//                if (mat.HasProperty("_BaseColor"))
//                    mat.SetColor("_BaseColor", color);
//                else if (mat.HasProperty("_Color"))
//                    mat.SetColor("_Color", color);

//                mat.EnableKeyword("_EMISSION");
//                if (mat.HasProperty("_EmissionColor"))
//                    mat.SetColor("_EmissionColor", color);
//            }
//            // Reasignamos para asegurarnos de que la instancia permanezca activa
//            mr.materials = mats;
//        }
//    }

//    /// <summary>
//    /// Coroutine que durante 'duracion' segundos revisa cada frame si hay nuevos materiales
//    /// y les aplica Color + Emission, sin tocar BaseMap.
//    /// </summary>
//    private IEnumerator DetectarYAsignarNuevosMateriales(Color color, float duracion)
//    {
//        float timer = 0f;
//        var conteoInicial = meshRenderers.ToDictionary(
//            mr => mr,
//            mr => mr.materials.Length
//        );

//        while (timer < duracion)
//        {
//            foreach (var mr in meshRenderers)
//            {
//                Material[] currentMats = mr.materials;
//                int inicial = conteoInicial[mr];
//                if (currentMats.Length > inicial)
//                {
//                    for (int i = inicial; i < currentMats.Length; i++)
//                    {
//                        Material mat = currentMats[i];

//                        // Asignar Color:
//                        if (mat.HasProperty("_BaseColor"))
//                            mat.SetColor("_BaseColor", color);
//                        else if (mat.HasProperty("_Color"))
//                            mat.SetColor("_Color", color);

//                        // Activar emisión HDR:
//                        mat.EnableKeyword("_EMISSION");
//                        if (mat.HasProperty("_EmissionColor"))
//                            mat.SetColor("_EmissionColor", color);
//                    }
//                    conteoInicial[mr] = currentMats.Length;
//                    mr.materials = currentMats;
//                }
//            }

//            timer += Time.deltaTime;
//            yield return null;
//        }
//    }

//    /// <summary>
//    /// Aplica daño directo y parpadeo HDR.
//    /// </summary>
//    public void RecibirDanio(float d)
//    {
//        if (isDead) return;
//        if (colorController != null)
//            colorController.RecibirDanio(d);

//        vida -= d;
//        if (sliderVida != null) sliderVida.value = vida;
//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    /// <summary>
//    /// Maneja daño por bala del jugador (ignora si coincide tipo-bala/tipo-enemigo).
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
//    {
//        // Si tipos coinciden, no hay daño
//        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
//            (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
//            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
//        {
//            return;
//        }

//        // Si no coincide, tomamos daño alto
//        float d;
//        switch (tb)
//        {
//            case BalaPlayer.TipoBala.Ametralladora:
//                d = danioAltoAmetralladora;
//                break;
//            case BalaPlayer.TipoBala.Pistola:
//                d = danioAltoPistola;
//                break;
//            case BalaPlayer.TipoBala.Escopeta:
//                d = danioAltoEscopeta;
//                break;
//            default:
//                d = 0f;
//                break;
//        }

//        // Headshot
//        if (hitCollider == headCollider)
//            d *= headshotMultiplier;

//        RecibirDanio(d);
//    }

//    void Morir()
//    {
//        if (isDead) return;
//        isDead = true;

//        if (TutorialManager.Instance != null)
//        {
//            int index = TutorialManager.Instance.currentIndex;
//            if (TutorialManager.Instance.scenes[index].sceneData.activationType == ActivationType.ByKills)
//            {
//                TutorialManager.Instance.StartScenarioByKills(index);
//            }
//        }

//        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
//            Instantiate(prefabsAlMorir[UnityEngine.Random.Range(0, prefabsAlMorir.Length)],
//                        transform.position, transform.rotation);

//        HUDManager.Instance?.AddInfoFragment(fragments);
//        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

//        Destroy(gameObject);
//    }
//}





















































