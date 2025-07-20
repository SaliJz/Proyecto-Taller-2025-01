using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using static EnemigoRosa;
using static UnityEngine.EventSystems.EventTrigger;

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

    [Header("Parpadeo HDR potente")]
    [Tooltip("Cuánto multiplicar la emisión en el flash")]
    public float flashIntensity = 2f;
    [Tooltip("Duración del pico de brillo")]
    public float flashDuration = 0.2f;
    [Tooltip("Curva para suavizar el pico")]
    public AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Colliders")]
    [Tooltip("Collider que corresponde a la cabeza")]
    public Collider headCollider;
    [Tooltip("Colliders que corresponden al cuerpo")]
    public Collider[] bodyColliders;


    [Header("Renderizado")]
    public SkinnedMeshRenderer[] meshRenderers;
    public MeshRenderer[] staticMeshRenderers;

    [Header("Prefabs al morir por tipo")]
    public GameObject prefabMuerteAmetralladora;
    public GameObject prefabMuertePistola;
    public GameObject prefabMuerteEscopeta;

    [Header("Fragmentos y HUD")]
    public int fragments = 50;

    [Header("Dissolve Settings")]
    public bool enableDissolve = true;
    public float dissolveDuration = 1f;

    private TipoEnemigo tipo;
    private bool isDead = false;
    private TipoColorHDRController colorController;
    private Animator animator;
    [SerializeField] bool isTracker;
    private List<Material> dissolveMaterials;

    private GameObject dataBaseManager;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        staticMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();
        dissolveMaterials = new List<Material>();
    }

    void Start()
    {
        dataBaseManager = GameObject.Find("DataBaseManager");
        animator = GetComponentInChildren<Animator>();

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }
        EnemyType.CurrentEnemyType tipo =GetComponent<EnemyType>().currentEnemyType;
        Color color = LevelManager_SQL.Instance.GetColorByEnemyType(tipo); 
        AsignarColorYEmissionAMateriales(color);
        SetTipoDesdeColor(color);
        //StartCoroutine(DetectarYAsignarNuevosMateriales(color, 0.5f));
        StartCoroutine(AppearEffect());
    }

    public void AsignarColorYEmissionAMateriales(Color color)
    {
        foreach (var mr in meshRenderers)
            AplicarColorAListaDeMateriales(mr.materials, color, mats => mr.materials = mats);
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

            if (enableDissolve && mat.HasProperty("_DissolveAmount"))
                mat.SetFloat("_DissolveAmount", 0f);
        }
        asignarMats(materials);
    }

    private IEnumerator DetectarYAsignarNuevosMateriales(Color color, float duracion)
    {
        float timer = 0f;
        var conteoSkinned = meshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);
        var conteoStatic = staticMeshRenderers.ToDictionary(mr => mr, mr => mr.materials.Length);

        while (timer < duracion)
        {
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
                            mats[i].SetFloat("_DissolveAmount", 0f);
                    }
                    conteoSkinned[mr] = mats.Length;
                    mr.materials = mats;
                }
            }
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
                            mats[i].SetFloat("_DissolveAmount", 0f);
                    }
                    conteoStatic[mr] = mats.Length;
                    mr.materials = mats;
                }
            }

            timer += Time.deltaTime;
            yield return null;
        }
    }

    public void ApplyAbilityDamage(float damage)
    {
        if (isDead) return;
        colorController?.RecibirDanio(damage);

        StopCoroutine(nameof(FlashEmission));
        StartCoroutine(FlashEmission());

        vida -= damage;
        if (sliderVida != null) sliderVida.value = vida;
        if (vida <= 0f)
        {
            vida = 0f;
            Morir();
        }
    }

    public void RecibirDanio(float d)
    {
        if (isDead) return;
        colorController?.RecibirDanio(d);

        // Parpadeo potente HDR
        StopCoroutine(nameof(FlashEmission));
        StartCoroutine(FlashEmission());

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

    private IEnumerator FlashEmission()
    {
        var mats = meshRenderers.SelectMany(mr => mr.materials)
                    .Concat(staticMeshRenderers.SelectMany(mr => mr.materials))
                    .ToArray();

        Color baseColor = mats.FirstOrDefault()?.GetColor("_EmissionColor") ?? Color.white;
        float timer = 0f;

        while (timer < flashDuration)
        {
            float t = flashCurve.Evaluate(timer / flashDuration);
            float intensity = 1 + (flashIntensity - 1) * t;
            foreach (var mat in mats)
                mat.SetColor("_EmissionColor", baseColor * intensity);

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var mat in mats)
            mat.SetColor("_EmissionColor", baseColor);
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

        if (enableDissolve)
        {
            dissolveMaterials.Clear();
            foreach (var mr in meshRenderers)
                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount")));
            foreach (var mr in staticMeshRenderers)
                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount")));

            StartCoroutine(DissolveEffect());
        }

        Destroy(sliderVida.gameObject);
        StartCoroutine(TimeToDead());
    }

    private IEnumerator DissolveEffect()
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
        foreach (var mat in dissolveMaterials)
            mat.SetFloat("_DissolveAmount", 1f);
    }

    IEnumerator TimeToDead()
    {
        if (isTracker)
            GetComponent<EnemigoVolador>().enabled = false;
        else
            GetComponent<NavMeshAgent>().enabled = false;

        if (animator != null) animator.SetBool("isDead", true);
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    private IEnumerator AppearEffect()
    {
        foreach (var mr in meshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_DissolveAmount"))
                    mat.SetFloat("_DissolveAmount", 1f);

        foreach (var mr in staticMeshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_DissolveAmount"))
                    mat.SetFloat("_DissolveAmount", 1f);

        float timer = 0f;
        while (timer < dissolveDuration)
        {
            float value = Mathf.Lerp(1f, 0f, timer / dissolveDuration);
            foreach (var mr in meshRenderers)
                foreach (var mat in mr.materials)
                    if (mat.HasProperty("_DissolveAmount"))
                        mat.SetFloat("_DissolveAmount", value);
            foreach (var mr in staticMeshRenderers)
                foreach (var mat in mr.materials)
                    if (mat.HasProperty("_DissolveAmount"))
                        mat.SetFloat("_DissolveAmount", value);

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var mr in meshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_DissolveAmount"))
                    mat.SetFloat("_DissolveAmount", 0f);
        foreach (var mr in staticMeshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_DissolveAmount"))
                    mat.SetFloat("_DissolveAmount", 0f);
    }

    private void OnEnable()
    {
        StartCoroutine(AppearEffect());
    }
    public void SetTipoDesdeColor(Color color)
    {
        // Usar valores aproximados en lugar de Color == Color.green
        if (Approximately(color, Color.blue))
            tipo = TipoEnemigo.Ametralladora;
        else if (Approximately(color, Color.green))
            tipo = TipoEnemigo.Pistola;
        else if (Approximately(color, Color.red))
            tipo = TipoEnemigo.Escopeta;
    }

    private bool Approximately(Color a, Color b, float tolerance = 0.05f)
    {
        return Mathf.Abs(a.r - b.r) < tolerance &&
               Mathf.Abs(a.g - b.g) < tolerance &&
               Mathf.Abs(a.b - b.b) < tolerance;
    }
}








