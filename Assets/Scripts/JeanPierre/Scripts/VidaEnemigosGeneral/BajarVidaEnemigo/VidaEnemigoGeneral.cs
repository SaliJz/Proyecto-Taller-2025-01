using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic; // ADICIÓN
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AI;

public class VidaEnemigoGeneral : MonoBehaviour
{
    private BalaPlayer.TipoBala? lastWeaponUsedToKill = null;

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
    public Color hdrColorPistola = Color.green;    // antes rojo, ahora verde
    [ColorUsage(true, true)]
    public Color hdrColorEscopeta  = Color.red;     // antes verde, ahora rojo

    [Header("Renderizado")]
    // Para mallas con skin
    public SkinnedMeshRenderer[] meshRenderers;
    // Para mallas estáticas
    public MeshRenderer[] staticMeshRenderers;

    [Header("Prefabs al morir por tipo")]
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Ametralladora")]
    public GameObject prefabMuerteAmetralladora;
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Pistola")]
    public GameObject prefabMuertePistola;
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Escopeta")]
    public GameObject prefabMuerteEscopeta;

    [Header("Fragmentos y HUD")]
    public int fragments = 50;

    [Header("Dissolve Settings")]      // ADICIÓN
    [Tooltip("Activar efecto dissolve al morir")]
    public bool enableDissolve = true;   // ADICIÓN
    [Tooltip("Duración del efecto dissolve en segundos")]
    public float dissolveDuration = 1f;   // ADICIÓN

    private TipoEnemigo tipo;
    private bool isDead = false;

    // Color final que se determinó al inicio y que no debe cambiar
    private Color finalColor;

    // Referencia al controlador de parpadeo HDR
    private TipoColorHDRController colorController;

    private Animator animator;

    [SerializeField] bool isTracker; //variable exclusiva del tracker para no pedir navmesh en la muerte

    // Lista de materiales sobre los que se aplicará dissolve
    private List<Material> dissolveMaterials; // ADICIÓN

    private GameObject dataBaseManager;

    void Awake()
    {
        // Obtener ambos tipos de renderers de los hijos
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        staticMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();

        // Inicializar lista de materiales para dissolve
        dissolveMaterials = new List<Material>(); // ADICIÓN
    }

    void Start()
    {
        dataBaseManager = GameObject.Find("DataBaseManager");
        animator = GetComponentInChildren<Animator>();

        //tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
#if UNITY_EDITOR
        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
#endif
        //finalColor = ObtenerColorPorTipo(tipo);

        // Asignar color y emisión a todos los materiales
        //AsignarColorYEmissionAMateriales(finalColor);

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }

        // Detectar nuevos materiales durante 0.5 seg
        StartCoroutine(DetectarYAsignarNuevosMateriales(finalColor, 0.5f));
    }

    //void Update()
    //{
    //    // Reaplicar color cada frame
    //    ReaplicarColorYEmissionAMateriales(finalColor);
    //}

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

    public void AsignarColorYEmissionAMateriales(Color color)
    {
        // SkinnedMeshRenderers
        foreach (var mr in meshRenderers)
            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);

        // MeshRenderers
        foreach (var mr in staticMeshRenderers)
            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);
    }

    private void ReaplicarColorYEmissionAMateriales(Color color)
    {
        // SkinnedMeshRenderers
        foreach (var mr in meshRenderers)
            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);

        // MeshRenderers
        foreach (var mr in staticMeshRenderers)
            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);
    }

    private void AplicarColorAListaDeMateriales(Material[] materials, Color color, Action<Material[]> asignarMats)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            var mat = materials[i];
            if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", color);
            else if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", color);

            mat.EnableKeyword("_EMISSION");
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", color);

            // Inicializar valor de dissolve en 0 al comienzo
            if (enableDissolve && mat.HasProperty("_DissolveAmount"))
                mat.SetFloat("_DissolveAmount", 0f); // ADICIÓN
        }
        asignarMats(materials);
    }

    private IEnumerator DetectarYAsignarNuevosMateriales(Color color, float duracion)
    {
        float timer = 0f;
        // Conteos iniciales
        var conteoSkinned = meshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);
        var conteoStatic = staticMeshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);

        while (timer < duracion)
        {
            // Nuevos en Skinned
            foreach (var mr in meshRenderers)
            {
                var mats = mr.materials;
                int inicial = conteoSkinned[mr];
                if (mats.Length > inicial)
                {
                    for (int i = inicial; i < mats.Length; i++)
                    {
                        AplicarColorAListaDeMateriales(new[] { mats[i] }, color, _ => { });
                        if (enableDissolve && mats[i].HasProperty("_DissolveAmount"))
                            mats[i].SetFloat("_DissolveAmount", 0f); // ADICIÓN
                    }
                    conteoSkinned[mr] = mats.Length;
                    mr.materials = mats;
                }
            }
            // Nuevos en Static
            foreach (var mr in staticMeshRenderers)
            {
                var mats = mr.materials;
                int inicial = conteoStatic[mr];
                if (mats.Length > inicial)
                {
                    for (int i = inicial; i < mats.Length; i++)
                    {
                        AplicarColorAListaDeMateriales(new[] { mats[i] }, color, _ => { });
                        if (enableDissolve && mats[i].HasProperty("_DissolveAmount"))
                            mats[i].SetFloat("_DissolveAmount", 0f); // ADICIÓN
                    }
                    conteoStatic[mr] = mats.Length;
                    mr.materials = mats;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void RecibirDanio(float d)
    {
        if (isDead) return;
        colorController?.RecibirDanio(d);

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
            return;

        float d = tb switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAltoAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioAltoPistola,
            BalaPlayer.TipoBala.Escopeta => danioAltoEscopeta,
            _ => 0f
        };

        if (hitCollider == headCollider)
            d *= headshotMultiplier;

        RecibirDanio(d);

        if (vida - d <= 0f)
        {
            lastWeaponUsedToKill = tb;
            Insert_Top_Weapon_Per_Enemy insert_Top_Weapon_Per_Enemy = dataBaseManager.GetComponent<Insert_Top_Weapon_Per_Enemy>();

            int weapon_id = GetWeaponIdByName(lastWeaponUsedToKill);
            if (weapon_id != -1)
            {
                insert_Top_Weapon_Per_Enemy.Execute(transform.gameObject.GetComponent<EnemyType>().enemy_id, weapon_id);
                Debug.Log("Insertando datos de enemigo y arma");
            }
        }
          

    }

    public int GetWeaponIdByName(object lastWeaponUsedToKill)
    {
        WeaponType[] allWeapons = FindObjectsOfType<WeaponType>();
        string armaNombre = lastWeaponUsedToKill.ToString();

        foreach (WeaponType weapon in allWeapons)
        {
            switch (armaNombre)
            {
                case "Pistola":
                    if (weapon.currentWeaponType == WeaponType.CurrentWeaponType.Gun)
                        return weapon.weapon_id;
                    break;

                case "Ametralladora":
                    if (weapon.currentWeaponType == WeaponType.CurrentWeaponType.Rifle)
                        return weapon.weapon_id;
                    break;

                case "Escopeta":
                    if (weapon.currentWeaponType == WeaponType.CurrentWeaponType.Shotgun)
                        return weapon.weapon_id;
                    break;
            }
        }

        return -1;
    }


    void Morir()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log($"Enemigo murió por arma: {lastWeaponUsedToKill}");

        //if (TutorialManager.Instance != null)
        //{
        //    int index = TutorialManager.Instance.currentDialogue;
        //    if (TutorialManager.Instance.GetCurrentSceneActivationType() == ActivationType.ByKills)
        //    {
        //        TutorialManager.Instance.ScenarioActivationCheckerByKills();
        //    }
        //}

        GameObject prefabAMorir = tipo switch
        {
            TipoEnemigo.Ametralladora => prefabMuerteAmetralladora,
            TipoEnemigo.Pistola => prefabMuertePistola,
            TipoEnemigo.Escopeta => prefabMuerteEscopeta,
            _ => null
        };

        if (prefabAMorir != null)
            Instantiate(prefabAMorir, transform.position, transform.rotation);

        HUDManager.Instance?.AddInfoFragment(fragments);
        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

        // Recolectar materiales para el efecto dissolve
        if (enableDissolve)
        {
            dissolveMaterials.Clear(); // ADICIÓN
            foreach (var mr in meshRenderers)
                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount"))); // ADICIÓN
            foreach (var mr in staticMeshRenderers)
                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount"))); // ADICIÓN

            StartCoroutine(DissolveEffect()); // ADICIÓN
        }

        StartCoroutine(TimeToDead());
    }

    // Nuevo coroutine para animar el DissolveAmount de 0 a 1
    private IEnumerator DissolveEffect() // ADICIÓN
    {
        float timer = 0f;
        while (timer < dissolveDuration)
        {
            float value = Mathf.Lerp(0f, 1f, timer / dissolveDuration);
            foreach (var mat in dissolveMaterials)
                mat.SetFloat("_DissolveAmount", value);
            timer += Time.deltaTime;
            yield return null;
        }
        // asegurar valor final
        foreach (var mat in dissolveMaterials)
            mat.SetFloat("_DissolveAmount", 1f);
    } // ADICIÓN

    IEnumerator TimeToDead()
    {
        if (isTracker)
        {
            EnemigoVolador enemigoVolador = GetComponent<EnemigoVolador>();
            enemigoVolador.enabled = false;

        }
        else
        {
            NavMeshAgent nav = GetComponent<NavMeshAgent>();
            nav.enabled = false;
        }
        if (animator != null) animator.SetBool("isDead", true);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    public void SetTipoDesdeColor(Color color)
    {
        if (color == hdrColorAmetralladora)
            tipo = TipoEnemigo.Ametralladora;

        else if (color == hdrColorPistola)
            tipo = TipoEnemigo.Pistola;
        else if (color == hdrColorEscopeta)
            tipo = TipoEnemigo.Escopeta;
        else
            tipo = TipoEnemigo.Ametralladora;

        //finalColor = color;
    }
}







































//using System;
//using System.Linq;
//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.AI;

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
//    public Color hdrColorPistola = Color.green;    // antes rojo, ahora verde
//    [ColorUsage(true, true)]
//    public Color hdrColorEscopeta = Color.red;     // antes verde, ahora rojo

//    [Header("Renderizado")]
//    // Para mallas con skin
//    public SkinnedMeshRenderer[] meshRenderers;
//    // Para mallas estáticas
//    public MeshRenderer[] staticMeshRenderers;

//    [Header("Prefabs al morir por tipo")]
//    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Ametralladora")]
//    public GameObject prefabMuerteAmetralladora;
//    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Pistola")]
//    public GameObject prefabMuertePistola;
//    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Escopeta")]
//    public GameObject prefabMuerteEscopeta;

//    [Header("Fragmentos y HUD")]
//    public int fragments = 50;

//    private TipoEnemigo tipo;
//    private bool isDead = false;

//    // Color final que se determinó al inicio y que no debe cambiar
//    private Color finalColor;

//    // Referencia al controlador de parpadeo HDR
//    private TipoColorHDRController colorController;

//    private Animator animator;

//    [SerializeField] bool isTracker;//variable exclusiva del tracker para no pedir navmesh en la muerte
//    void Awake()
//    {
//        // Obtener ambos tipos de renderers de los hijos
//        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
//        staticMeshRenderers = GetComponentsInChildren<MeshRenderer>();
//        colorController = GetComponent<TipoColorHDRController>();
//    }

//    void Start()
//    {
//        animator = GetComponentInChildren<Animator>();

//        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
//#if UNITY_EDITOR
//        Debug.Log($"[VidaEnemigo] Tipo: {tipo}");
//#endif
//        finalColor = ObtenerColorPorTipo(tipo);

//        // Asignar color y emisión a todos los materiales
//        AsignarColorYEmissionAMateriales(finalColor);

//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }

//        // Detectar nuevos materiales durante 0.5 seg
//        StartCoroutine(DetectarYAsignarNuevosMateriales(finalColor, 0.5f));
//    }

//    void Update()
//    {
//        // Reaplicar color cada frame
//        ReaplicarColorYEmissionAMateriales(finalColor);
//    }

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

//    private void AsignarColorYEmissionAMateriales(Color color)
//    {
//        // SkinnedMeshRenderers
//        foreach (var mr in meshRenderers)
//            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);

//        // MeshRenderers
//        foreach (var mr in staticMeshRenderers)
//            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);
//    }

//    private void ReaplicarColorYEmissionAMateriales(Color color)
//    {
//        // SkinnedMeshRenderers
//        foreach (var mr in meshRenderers)
//            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);

//        // MeshRenderers
//        foreach (var mr in staticMeshRenderers)
//            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);
//    }

//    private void AplicarColorAListaDeMateriales(Material[] materials, Color color, Action<Material[]> asignarMats)
//    {
//        for (int i = 0; i < materials.Length; i++)
//        {
//            var mat = materials[i];
//            if (mat.HasProperty("_BaseColor"))
//                mat.SetColor("_BaseColor", color);
//            else if (mat.HasProperty("_Color"))
//                mat.SetColor("_Color", color);

//            mat.EnableKeyword("_EMISSION");
//            if (mat.HasProperty("_EmissionColor"))
//                mat.SetColor("_EmissionColor", color);
//        }
//        asignarMats(materials);
//    }

//    private IEnumerator DetectarYAsignarNuevosMateriales(Color color, float duracion)
//    {
//        float timer = 0f;
//        // Conteos iniciales
//        var conteoSkinned = meshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);
//        var conteoStatic = staticMeshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);

//        while (timer < duracion)
//        {
//            // Nuevos en Skinned
//            foreach (var mr in meshRenderers)
//            {
//                var mats = mr.materials;
//                int inicial = conteoSkinned[mr];
//                if (mats.Length > inicial)
//                {
//                    for (int i = inicial; i < mats.Length; i++)
//                        AplicarColorAListaDeMateriales(new[] { mats[i] }, color, _ => { });
//                    conteoSkinned[mr] = mats.Length;
//                    mr.materials = mats;
//                }
//            }
//            // Nuevos en Static
//            foreach (var mr in staticMeshRenderers)
//            {
//                var mats = mr.materials;
//                int inicial = conteoStatic[mr];
//                if (mats.Length > inicial)
//                {
//                    for (int i = inicial; i < mats.Length; i++)
//                        AplicarColorAListaDeMateriales(new[] { mats[i] }, color, _ => { });
//                    conteoStatic[mr] = mats.Length;
//                    mr.materials = mats;
//                }
//            }

//            timer += Time.deltaTime;
//            yield return null;
//        }
//    }

//    public void RecibirDanio(float d)
//    {
//        if (isDead) return;
//        colorController?.RecibirDanio(d);

//        vida -= d;
//        if (sliderVida != null) sliderVida.value = vida;
//        if (vida <= 0f)
//        {
//            vida = 0f;
//            Morir();
//        }
//    }

//    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb, Collider hitCollider)
//    {
//        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
//            (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
//            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
//            return;

//        float d = tb switch
//        {
//            BalaPlayer.TipoBala.Ametralladora => danioAltoAmetralladora,
//            BalaPlayer.TipoBala.Pistola => danioAltoPistola,
//            BalaPlayer.TipoBala.Escopeta => danioAltoEscopeta,
//            _ => 0f
//        };

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
//            int index = TutorialManager.Instance.currentDialogueIndex;
//            if (TutorialManager.Instance.GetCurrentSceneActivationType() == ActivationType.ByKills)
//            {
//                TutorialManager.Instance.ScenarioActivationCheckerByKills();
//            }
//        }

//        GameObject prefabAMorir = tipo switch
//        {
//            TipoEnemigo.Ametralladora => prefabMuerteAmetralladora,
//            TipoEnemigo.Pistola => prefabMuertePistola,
//            TipoEnemigo.Escopeta => prefabMuerteEscopeta,
//            _ => null
//        };

//        if (prefabAMorir != null)
//            Instantiate(prefabAMorir, transform.position, transform.rotation);

//        HUDManager.Instance?.AddInfoFragment(fragments);
//        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());
//        StartCoroutine(TimeToDead());
//    }

//    IEnumerator TimeToDead()
//    {
//        if (isTracker)
//        {
//            EnemigoVolador enemigoVolador = GetComponent<EnemigoVolador>();
//            enemigoVolador.enabled = false;

//        }
//        else
//        {
//            NavMeshAgent nav = GetComponent<NavMeshAgent>();
//            nav.enabled = false;
//        }
//        if (animator != null) animator.SetBool("isDead", true);
//        yield return new WaitForSeconds(2f);
//        Destroy(gameObject);
//    }
//}







