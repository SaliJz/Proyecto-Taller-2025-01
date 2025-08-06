using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
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

    [Header("Colores HDR por tipo")]
    [ColorUsage(true, true)]
    public Color hdrColorAmetralladora = Color.blue;
    [ColorUsage(true, true)]
    public Color hdrColorPistola = Color.green;
    [ColorUsage(true, true)]
    public Color hdrColorEscopeta = Color.red;

    [Tooltip("Color que tomará el enemigo al ser afectado por Mindjack")]
    [ColorUsage(true, true)]
    public Color hdrColorMindjacked = Color.magenta;

    [Header("Configuración de HDR")]
    public float intervaloCambio = 10f;
    public float duracionTransicionColor = 1f;
    public int blinkCount = 4;
    public float blinkInterval = 0.1f;
    public float delayInicial = 0.6f;
    private Coroutine colorRoutine;
    private bool isBlinking = false;

    [Header("Renderizado")]
    public SkinnedMeshRenderer[] meshRenderers;
    public MeshRenderer[] staticMeshRenderers;

    [Header("Prefabs al morir por tipo")]
    public GameObject[] prefabsDropsAmmont;

    [Header("Fragmentos y HUD")]
    public int fragments = 50;
    public GameObject prefabFragments;

    [Header("Dissolve Settings")]
    public bool enableDissolve = true;
    public float dissolveDuration = 1f;

    private TipoEnemigo tipo;
    private Color colorOriginal;
    private Color baseEmissionColor;
    private Color currentEmissionColor;
    private bool isTransitioningColor = false;
    private bool isDead = false;
    private Animator animator;
    [SerializeField] bool isTracker;
    private List<Material> dissolveMaterials;
    private EnemyAbilityReceiver enemyAbilityReceiver;

    private Coroutine flashRoutine;

    void Awake()
    {
        enemyAbilityReceiver = GetComponent<EnemyAbilityReceiver>();
        animator = GetComponentInChildren<Animator>();
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        staticMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        dissolveMaterials = new List<Material>();
    }

    void Start()
    {
        // 1. Asignar un tipo y color único al nacer.
        tipo = (TipoEnemigo)UnityEngine.Random.Range(0, 3);
        colorOriginal = ObtenerColorPorTipo(tipo);

        foreach (var mr in meshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", colorOriginal);

        foreach (var mr in staticMeshRenderers)
            foreach (var mat in mr.materials)
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", colorOriginal);

        baseEmissionColor = colorOriginal;
        currentEmissionColor = colorOriginal; 

        // 2. Aplicar el color inicial
        AsignarColorYEmissionAMateriales(colorOriginal);

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }

        StartCoroutine(DetectarYAsignarNuevosMateriales(colorOriginal, 0.5f));
    }

    private void OnEnable()
    {
        if (enemyAbilityReceiver != null)
        {
            enemyAbilityReceiver.OnStateChanged += HandleEnemyStateChange;
        }

        if (enableDissolve)
        {
            StartCoroutine(AppearEffect());
        }
    }

    private void OnDisable()
    {
        if (enemyAbilityReceiver != null)
        {
            enemyAbilityReceiver.OnStateChanged -= HandleEnemyStateChange;
        }
    }

    private Color ObtenerColorPorTipo(TipoEnemigo t)
    {
        switch (t)
        {
            case TipoEnemigo.Ametralladora: return hdrColorAmetralladora;
            case TipoEnemigo.Pistola: return hdrColorPistola;
            case TipoEnemigo.Escopeta: return hdrColorEscopeta;
            default: return Color.white;
        }
    }

    private void AsignarColorYEmissionAMateriales(Color color)
    {
        currentEmissionColor = color;

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

        if (!isTransitioningColor)
        {
            // Parpadeo potente
            if (!isBlinking) StartCoroutine(Parpadeo());
            // Flash de emisión
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashEmission());
        }

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

        if (!isTransitioningColor)
        {
            // Parpadeo potente
            if (!isBlinking) StartCoroutine(Parpadeo());
            // Flash de emisión
            if (flashRoutine != null) StopCoroutine(flashRoutine);
            flashRoutine = StartCoroutine(FlashEmission());
        }

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
        if (enemyAbilityReceiver.CurrentState != EnemyAbilityReceiver.EnemyState.Mindjacked)
        {
            if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TipoEnemigo.Ametralladora) ||
                (tb == BalaPlayer.TipoBala.Pistola && tipo == TipoEnemigo.Pistola) ||
                (tb == BalaPlayer.TipoBala.Escopeta && tipo == TipoEnemigo.Escopeta))
            {
                return;
            }
        }

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
    }

    private void HandleEnemyStateChange(EnemyAbilityReceiver.EnemyState newState)
    {
        if (isDead) return;

        isTransitioningColor = true;

        if (newState == EnemyAbilityReceiver.EnemyState.Mindjacked)
        {
            AsignarColorYEmissionAMateriales(hdrColorMindjacked);
            StartCoroutine(DetectarYAsignarNuevosMateriales(hdrColorMindjacked, 0.5f));
        }
        else
        {
            AsignarColorYEmissionAMateriales(colorOriginal);
            StartCoroutine(DetectarYAsignarNuevosMateriales(colorOriginal, 0.5f));
        }

        StartCoroutine(FinalizarTransicionColor());
    }

    private IEnumerator FinalizarTransicionColor()
    {
        yield return new WaitForSeconds(0.6f);
        isTransitioningColor = false;
    }

    private IEnumerator FlashEmission()
    {
        var mats = meshRenderers.SelectMany(mr => mr.materials)
                    .Concat(staticMeshRenderers.SelectMany(mr => mr.materials))
                    .ToArray();

        Color baseColor = GetCurrentStateColor();
        float timer = 0f;

        while (timer < flashDuration)
        {
            float t = flashCurve.Evaluate(timer / flashDuration);
            float intensity = 1 + (flashIntensity - 1) * t;
            foreach (var mat in mats)
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", baseColor * intensity);

            timer += Time.deltaTime;
            yield return null;
        }

        foreach (var mat in mats)
            if (mat.HasProperty("_EmissionColor"))
                mat.SetColor("_EmissionColor", baseColor);

        currentEmissionColor = baseColor;
    }

    private Color GetCurrentStateColor()
    {
        if (enemyAbilityReceiver != null && enemyAbilityReceiver.CurrentState == EnemyAbilityReceiver.EnemyState.Mindjacked)
        {
            return hdrColorMindjacked;
        }
        return colorOriginal;
    }

    private IEnumerator Parpadeo()
    {
        if (isTransitioningColor) yield break;

        isBlinking = true;

        Color currentStateColor = GetCurrentStateColor();

        // Detecta soporte para _Color y _EmissionColor
        var supportsColor = meshRenderers
            .Select(r => r.material.HasProperty("_Color"))
            .ToList();
        var supportsHDR = meshRenderers
            .Select(r => r.material.HasProperty("_EmissionColor"))
            .ToList();

        var baseColors = meshRenderers
            .Select((r, i) => supportsColor[i]
                ? r.material.GetColor("_Color")
                : Color.white)
            .ToList();
        var baseHDR = meshRenderers
            .Select(_ => currentStateColor)
            .ToList();

        float half = blinkInterval * 0.5f;

        for (int i = 0; i < blinkCount; i++)
        {
            if (isTransitioningColor)
            {
                isBlinking = false;
                yield break;
            }

            // Parpadeo a blanco
            for (int j = 0; j < meshRenderers.Length; j++)
            {
                var rend = meshRenderers[j];
                if (supportsColor[j])
                    rend.material.SetColor("_Color", Color.white);
                if (supportsHDR[j])
                    rend.material.SetColor("_EmissionColor", Color.white);
            }
            yield return new WaitForSeconds(half);

            if (isTransitioningColor)
            {
                isBlinking = false;
                yield break;
            }

            // Restaurar valores originales
            for (int j = 0; j < meshRenderers.Length; j++)
            {
                var rend = meshRenderers[j];
                if (supportsColor[j])
                    rend.material.SetColor("_Color", baseColors[j]);
                if (supportsHDR[j])
                    rend.material.SetColor("_EmissionColor", baseHDR[j]);
            }
            yield return new WaitForSeconds(half);
        }

        currentEmissionColor = currentStateColor;
        isBlinking = false;
    }

    void Morir()
    {
        if (isDead) return;
        isDead = true;

        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
            gameObject.layer = LayerMask.NameToLayer("DeadEnemies");

        DesactiveScripts();

        if (prefabFragments != null)
            Instantiate(prefabFragments, transform.position, transform.rotation);

        GameObject selectedPrefab = prefabsDropsAmmont.Length == 0
            ? null
            : prefabsDropsAmmont[UnityEngine.Random.Range(0, prefabsDropsAmmont.Length)];

        if (selectedPrefab != null)
        {
            Debug.Log("Instanciando: " + selectedPrefab.name);
            GameObject droppedItem = Instantiate(selectedPrefab, transform.position, transform.rotation);
        }
        else
        {
            Debug.Log("No hay prefabs para instanciar");
        }

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

        Turret turret = GetComponent<Turret>();
        if (turret != null) Destroy(gameObject);
        StartCoroutine(TimeToDead());
    }

    private void DesactiveScripts()
    {
        if (enemyAbilityReceiver != null)
        {
            GameObject glitchParticle = enemyAbilityReceiver.GetActiveGlitchParticle();
            if (glitchParticle != null)
            {
                Destroy(glitchParticle);
            }

            if (enemyAbilityReceiver.CurrentState == EnemyAbilityReceiver.EnemyState.Mindjacked)
            {
                AsignarColorYEmissionAMateriales(colorOriginal);
            }

            enemyAbilityReceiver.StopAllAbilityEffects();
            enemyAbilityReceiver.enabled = false;
        }

        NavMeshAgent agent = GetComponent<NavMeshAgent>();
        if (agent != null)
            agent.enabled = false;

        AtaqueEnemigoAVidaPlayer ataque = GetComponent<AtaqueEnemigoAVidaPlayer>();
        if (ataque != null)
            ataque.enabled = false;

        EnemigoDisparador dispardor = GetComponent<EnemigoDisparador>();
        if (dispardor != null)
            dispardor.enabled = false;

        LookAtPlayerY lookAtPlayerY = GetComponent<LookAtPlayerY>();
        if (lookAtPlayerY != null)
            lookAtPlayerY.enabled = false;
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
}