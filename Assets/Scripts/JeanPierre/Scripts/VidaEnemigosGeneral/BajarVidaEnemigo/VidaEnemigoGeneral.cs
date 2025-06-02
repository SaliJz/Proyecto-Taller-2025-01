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
        // 1) Determinamos tipo al azar y su color HDR
        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
#if UNITY_EDITOR
        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
#endif
        Color finalColor = ObtenerColorPorTipo(tipo);

        // 2) Asignamos color de BaseColor (sin cambiar Base Map) y Emission a materiales existentes
        AsignarColorYEmissionAMateriales(finalColor);

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }

        // 3) Durante los próximos 0.5 seg, detectamos y aplicamos color a nuevos materiales
        StartCoroutine(DetectarYAsignarColorANuevosMateriales(finalColor, 0.5f));
    }

    /// <summary>
    /// Devuelve el color HDR que corresponde al tipo de enemigo.
    /// </summary>
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

    /// <summary>
    /// Asigna a cada material existente:
    ///   - "_BaseColor" (o "_Color") = color del tipo de enemigo
    ///   - Activa _EMISSION y asigna "_EmissionColor" = HDR correspondiente
    /// </summary>
    private void AsignarColorYEmissionAMateriales(Color color)
    {
        foreach (var mr in meshRenderers)
        {
            Material[] mats = mr.materials; // instancia cada material
            for (int i = 0; i < mats.Length; i++)
            {
                Material mat = mats[i];

                // 1) Tinte del albedo: BaseColor (URP) o _Color (legacy)
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", color);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", color);

                // 2) Activar emisión HDR y asignar color
                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", color);
            }
            // Reasignamos el array completo para asegurar que Unity reconozca las instancias
            mr.materials = mats;
        }
    }

    /// <summary>
    /// Coroutine que dura 'duracion' segundos y revisa cada frame si aparecen nuevos materiales;
    /// si hay, aplica el mismo color y emisión.
    /// </summary>
    private IEnumerator DetectarYAsignarColorANuevosMateriales(Color color, float duracion)
    {
        float timer = 0f;
        // Guardamos cuántos materiales tenía cada renderer al inicio
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
                    // Hay nuevos materiales desde 'inicial' hasta currentMats.Length
                    for (int i = inicial; i < currentMats.Length; i++)
                    {
                        Material mat = currentMats[i];

                        // 1) Tintar albedo, sin tocar la Base Map
                        if (mat.HasProperty("_BaseColor"))
                            mat.SetColor("_BaseColor", color);
                        else if (mat.HasProperty("_Color"))
                            mat.SetColor("_Color", color);

                        // 2) Activar emisión HDR y asignar color
                        mat.EnableKeyword("_EMISSION");
                        if (mat.HasProperty("_EmissionColor"))
                            mat.SetColor("_EmissionColor", color);
                    }
                    // Actualizamos conteo para no reprocesar estos materiales
                    conteoInicial[mr] = currentMats.Length;
                    mr.materials = currentMats;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    /// <summary>
    /// Aplica daño directo y parpadeo HDR.
    /// </summary>
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

    /// <summary>
    /// Maneja daño por bala del jugador (ignora si coincide tipo-bala/tipo-enemigo).
    /// </summary>
    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
    {
        // Si tipos coinciden, no hay daño
        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
            (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
        {
            return;
        }

        // Si no coincide, tomamos daño alto
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

        // Headshot
        if (hitCollider == headCollider)
            d *= headshotMultiplier;

        RecibirDanio(d);
    }

    void Morir()
    {
        if (isDead) return;
        isDead = true;

        if (TutorialManager.Instance != null)
        {
            int index = TutorialManager.Instance.currentIndex;
            if (TutorialManager.Instance.scenes[index].sceneData.activationType == ActivationType.ByKills)
            {
                TutorialManager.Instance.StartScenarioByKills(index);
            }
        }

        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
            Instantiate(prefabsAlMorir[UnityEngine.Random.Range(0, prefabsAlMorir.Length)],
                        transform.position, transform.rotation);

        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

        Destroy(gameObject);
    }
}



//using System;
//using System.Linq;
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

//    // Referencia al controlador de parpadeo HDR
//    private TipoColorHDRController colorController;

//    void Awake()
//    {
//        meshRenderers = GetComponentsInChildren<MeshRenderer>();
//        // Obtenemos componente para disparar el parpadeo cuando haya daño
//        colorController = GetComponent<TipoColorHDRController>();
//    }

//    void Start()
//    {
//        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
//#if UNITY_EDITOR
//        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
//#endif

//        // Seleccionamos el color HDR directamente
//        Color finalColor = tipo == TipoEnemigo.Ametralladora ? hdrColorAmetralladora
//                        : tipo == TipoEnemigo.Pistola ? hdrColorPistola
//                        : hdrColorEscopeta;

//        foreach (var mr in meshRenderers)
//        {
//            Material mat = mr.material;

//            // Base Color (según shader)
//            if (mat.HasProperty("_BaseColor"))
//                mat.SetColor("_BaseColor", finalColor);
//            else if (mat.HasProperty("_Color"))
//                mat.SetColor("_Color", finalColor);

//            // Emission Color
//            mat.EnableKeyword("_EMISSION");
//            if (mat.HasProperty("_EmissionColor"))
//                mat.SetColor("_EmissionColor", finalColor);
//        }

//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }
//    }

//    /// <summary>
//    /// Aplica daño directo (e.g., por explosion o efecto) y dispara parpadeo HDR.
//    /// </summary>
//    public void RecibirDanio(float d)
//    {
//        if (isDead) return;

//        // Disparar parpadeo HDR
//        if (colorController != null)
//            colorController.RecibirDanio(d);

//        // Aplicar resta de vida
//        vida -= d;
//        if (sliderVida != null) sliderVida.value = vida;
//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    /// <summary>
//    /// Método que maneja el daño proveniente de una bala del jugador.
//    /// Si el tipo de la bala coincide con el tipo del enemigo, no hace nada.
//    /// </summary>
//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
//    {
//        // Si el tipo de bala coincide con el tipo de enemigo, no hay daño ni parpadeo
//        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
//            (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
//            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
//        {
//            return;
//        }

//        // Si no coincide, calculamos el daño “alto”
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

//        // Si headshot, multiplicamos
//        if (hitCollider == headCollider)
//            d *= headshotMultiplier;

//        // Aplicamos daño normal (que a su vez dispara parpadeo HDR)
//        RecibirDanio(d);
//    }

//    void Morir()
//    {
//        if (isDead) return;
//        isDead = true;

//        //TutorialEnemies tutorial = GetComponent<TutorialEnemies>();
//        //if (tutorial != null)
//        //{
//        //    foreach (int index in tutorial.IndexScenes)
//        //    {
//        //        TutorialManager.Instance.StartScenarioByKills(index);
//        //    }
//        //}

//        if (TutorialManager.Instance != null)
//        {
//            //Ahora la deteccion lo hace por el indice actual del TutorialManager
//            int index = TutorialManager.Instance.currentIndex;

//            if (TutorialManager.Instance.scenes[index].sceneData.activationType == ActivationType.ByKills)
//            {
//                TutorialManager.Instance.StartScenarioByKills(index);
//            }

//        }


//        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
//            Instantiate(prefabsAlMorir[UnityEngine.Random.Range(0, prefabsAlMorir.Length)], transform.position, transform.rotation);

//        HUDManager.Instance?.AddInfoFragment(fragments);
//        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

//        Destroy(gameObject);
//    }
//}
























































