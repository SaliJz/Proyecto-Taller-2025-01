﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemigoRosa : MonoBehaviour
{
    public enum TypeInvulnerability { None, Gun, Rifle, Shotgun }

    [Header("Configuración de vida")]
    public float vida = 100f;
    public Slider sliderVida;

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
    public Collider headCollider;
    public Collider[] bodyColliders;

    [Header("Colores HDR")]
    [ColorUsage(true, true)]
    public Color hdrColorPistola = Color.red;

    [Header("Renderizado")]
    // Para mallas con skin
    public SkinnedMeshRenderer[] meshRenderers;
    // Para mallas estáticas
    public MeshRenderer[] staticMeshRenderers;

    [Header("Prefabs al morir por tipo")]
    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Pistola")]
    public GameObject fragmentDeath;

    [Header("Prefabs al morir")]
    public GameObject[] prefabsAlMorir;
    public int fragments = 50;

    [Header("Dissolve Settings")]
    [Tooltip("Activar efecto dissolve al morir")]
    public bool enableDissolve = true;
    [Tooltip("Duración del efecto dissolve en segundos")]
    public float dissolveDuration = 1f;

    [SerializeField] private TypeInvulnerability tipo;
    private bool isDead = false;

    // Color final que se determinó al inicio y que no debe cambiar
    private Color finalColor;

    // Referencia al controlador de parpadeo HDR original (si existe)
    private TipoColorHDRController colorController;

    private Animator animator;
    [SerializeField] bool isTracker;

    // Lista de materiales para el dissolve
    private List<Material> dissolveMaterials;
    // Lista de todos los materiales para el flash
    private Material[] flashMaterials;

    void Awake()
    {
        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
        staticMeshRenderers = GetComponentsInChildren<MeshRenderer>();
        colorController = GetComponent<TipoColorHDRController>();
        dissolveMaterials = new List<Material>();

        // Preparar lista de materiales para el parpadeo
        flashMaterials = meshRenderers.SelectMany(mr => mr.materials)
            .Concat(staticMeshRenderers.SelectMany(mr => mr.materials))
            .ToArray();
    }

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

#if UNITY_EDITOR
        Debug.Log($"[EnemigoRosa] Tipo: {tipo}");
#endif

        // Asignar color HDR inicial
        finalColor = hdrColorPistola;
        foreach (var mr in meshRenderers)
        {
            foreach (var mat in mr.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", finalColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", finalColor);

                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", finalColor);
            }
        }
        foreach (var mr in staticMeshRenderers)
        {
            foreach (var mat in mr.materials)
            {
                if (mat.HasProperty("_BaseColor"))
                    mat.SetColor("_BaseColor", finalColor);
                else if (mat.HasProperty("_Color"))
                    mat.SetColor("_Color", finalColor);

                mat.EnableKeyword("_EMISSION");
                if (mat.HasProperty("_EmissionColor"))
                    mat.SetColor("_EmissionColor", finalColor);
            }
        }

        if (sliderVida != null)
        {
            sliderVida.maxValue = vida;
            sliderVida.value = vida;
        }

        StartCoroutine(AppearEffect());
    }

    public void RecibirDanio(float d)
    {
        if (isDead) return;
        colorController?.RecibirDanio(d);

        // Iniciar parpadeo HDR potente
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
        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TypeInvulnerability.Rifle) ||
            (tb == BalaPlayer.TipoBala.Pistola && tipo == TypeInvulnerability.Shotgun) ||
            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TypeInvulnerability.Gun))
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
        Debug.Log($"RecibirDanioPorBala: tipo invulnerabilidad={tipo}, tipo bala={tb}");
    }

    private IEnumerator FlashEmission()
    {
        // Guardar el color base de emisión
        Color baseColor = flashMaterials.FirstOrDefault()?.GetColor("_EmissionColor") ?? Color.white;
        float timer = 0f;

        while (timer < flashDuration)
        {
            float t = flashCurve.Evaluate(timer / flashDuration);
            float intensity = 1 + (flashIntensity - 1) * t;
            foreach (var mat in flashMaterials)
            {
                mat.SetColor("_EmissionColor", baseColor * intensity);
            }

            timer += Time.deltaTime;
            yield return null;
        }

        // Restaurar color base
        foreach (var mat in flashMaterials)
        {
            mat.SetColor("_EmissionColor", baseColor);
        }
    }

    void Morir()
    {
        if (isDead) return;
        isDead = true;

        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
        {
            int i = Random.Range(0, prefabsAlMorir.Length);
            Instantiate(prefabsAlMorir[i], transform.position, transform.rotation);
        }
        if (fragmentDeath != null)
            Instantiate(fragmentDeath, transform.position, transform.rotation);

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

        if (TutorialManager0.Instance != null)
        {
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb != null)
                rb.constraints &= ~RigidbodyConstraints.FreezePositionY;

            if (TutorialManager0.Instance.currentDialogueIndex == 10)
                TutorialManager0.Instance.ConfirmAdvance();

            if (TutorialManager0.Instance.currentDialogueIndex == 7)
            {
                TutorialManager0.Instance.spriteJumpToUIs[3].gameObject.GetComponent<SpriteRenderer>().enabled = false;
                TutorialManager0.Instance.spriteJumpToUIs[3].ejecutarAnimacion = true;
            }   
        }

        if (sliderVida != null)
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
        NavMeshAgent nav = GetComponent<NavMeshAgent>();
        if (nav != null) nav.enabled = false;

        if (animator != null) animator.SetTrigger("isDead");
        yield return new WaitForSeconds(2f);

        if (animator != null) animator.SetBool("Die", true);
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

    private void OnDestroy()
    {
        if (TutorialManager0.Instance != null && TutorialManager0.Instance.currentDialogueIndex == 7)
        {
            Debug.Log("ConfirmAdvance llamado por enemigo muerto");
            TutorialManager0.Instance.StartCoroutine(
                TutorialManager0.Instance.ActivateTransitionBetweenTwoCameras(2, 3.5f, 3, 2.5f));
            TutorialManager0.Instance.ConfirmAdvance();
        }
    }
}






//using System.Collections;
//using System.Collections.Generic;
//using System.Linq;
//using UnityEngine;
//using UnityEngine.AI;
//using UnityEngine.UI;

//public class EnemigoRosa : MonoBehaviour
//{
//    public enum TypeIvulnerability { None, Gun, Rifle, Shotgun }

//    [Header("Configuración de vida")]
//    public float vida = 100f;
//    public Slider sliderVida;

//    [Header("Daño ALTO (no coincide) por tipo de bala")]
//    public float danioAltoAmetralladora = 20f;
//    public float danioAltoPistola = 20f;
//    public float danioAltoEscopeta = 20f;

//    [Header("Multiplicador por headshot")]
//    public float headshotMultiplier = 2f;

//    [Header("Colliders")]
//    public Collider headCollider;
//    public Collider[] bodyColliders;

//    [Header("Colores HDR")]
//    [ColorUsage(true, true)]
//    public Color hdrColorPistola = Color.red;

//    [Header("Renderizado")]
//    // Para mallas con skin
//    public SkinnedMeshRenderer[] meshRenderers;
//    // Para mallas estáticas
//    public MeshRenderer[] staticMeshRenderers;

//    [Header("Prefabs al morir por tipo")]
//    [Tooltip("Prefab a instanciar cuando muere un enemigo tipo Pistola")]
//    public GameObject fragmentDeath;

//    [Header("Prefabs al morir")]
//    public GameObject[] prefabsAlMorir;
//    public int fragments = 50;

//    [Header("Dissolve Settings")]      // ADICIÓN
//    [Tooltip("Activar efecto dissolve al morir")]
//    public bool enableDissolve = true;   // ADICIÓN
//    [Tooltip("Duración del efecto dissolve en segundos")]
//    public float dissolveDuration = 1f;   // ADICIÓN

//    [SerializeField] private TypeIvulnerability tipo;
//    private bool isDead = false;

//    // Color final que se determinó al inicio y que no debe cambiar
//    private Color finalColor;

//    // Referencia al controlador de parpadeo HDR
//    private TipoColorHDRController colorController;

//    private Animator animator;

//    [SerializeField] bool isTracker; //variable exclusiva del tracker para no pedir navmesh en la muerte

//    // Lista de materiales sobre los que se aplicará dissolve
//    private List<Material> dissolveMaterials; // ADICIÓN

//    void Awake()
//    {
//        meshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();
//        colorController = GetComponent<TipoColorHDRController>();
//        dissolveMaterials = new List<Material>();
//    }

//    void Start()
//    {
//        animator = GetComponentInChildren<Animator>();

//#if UNITY_EDITOR
//        Debug.Log($"[EnemigoRosa] Tipo: {tipo}");
//#endif

//        //// Asignar color HDR
//        finalColor = hdrColorPistola;
//        foreach (var mr in meshRenderers)
//        {
//            Material mat = mr.material;

//            if (mat.HasProperty("_BaseColor"))
//                mat.SetColor("_BaseColor", finalColor);
//            else if (mat.HasProperty("_Color"))
//                mat.SetColor("_Color", finalColor);

//            mat.EnableKeyword("_EMISSION");
//            if (mat.HasProperty("_EmissionColor"))
//                mat.SetColor("_EmissionColor", finalColor);
//        }

//        if (sliderVida != null)
//        {
//            sliderVida.maxValue = vida;
//            sliderVida.value = vida;
//        }

//        StartCoroutine(AppearEffect());
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
//        if ((tb == BalaPlayer.TipoBala.Ametralladora && tipo == TypeIvulnerability.Rifle) ||
//            (tb == BalaPlayer.TipoBala.Pistola && tipo == TypeIvulnerability.Shotgun) ||
//            (tb == BalaPlayer.TipoBala.Escopeta && tipo == TypeIvulnerability.Gun))
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
//        Debug.Log($"RecibirDanioPorBala: tipo invulnerabilidad={tipo}, tipo bala={tb}");
//    }
//    void Morir()
//    {
//        if (isDead) return;
//        isDead = true;


//        // Si hay prefabs genéricos disponibles, elegir uno aleatorio
//        if (prefabsAlMorir != null && prefabsAlMorir.Length > 0)
//            {
//                int i = Random.Range(0, prefabsAlMorir.Length);
//            Instantiate(prefabsAlMorir[i], transform.position, transform.rotation);
//        }

//        if (fragmentDeath != null)
//            Instantiate(fragmentDeath, transform.position, transform.rotation);        

//        HUDManager.Instance?.AddInfoFragment(fragments);
//        MissionManager.Instance?.RegisterKill(gameObject.tag, name, tipo.ToString());

//        // Recolectar materiales para el efecto dissolve
//        if (enableDissolve)
//        {
//            dissolveMaterials.Clear();
//            foreach (var mr in meshRenderers)
//                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount")));
//            foreach (var mr in staticMeshRenderers)
//                dissolveMaterials.AddRange(mr.materials.Where(m => m.HasProperty("_DissolveAmount")));

//            StartCoroutine(DissolveEffect());
//        }

//        if (TutorialManager0.Instance != null)
//        {
//            Rigidbody rb = GetComponent<Rigidbody>();
//            if (rb != null)
//            {
//                rb.constraints &= ~RigidbodyConstraints.FreezePositionY;
//            }

//            if (TutorialManager0.Instance.currentDialogueIndex == 10)
//            {
//                TutorialManager0.Instance.ConfirmAdvance();
//            }
//        }

//        if (sliderVida != null)
//            Destroy(sliderVida.gameObject);


//        StartCoroutine(TimeToDead());
//    }
//    private IEnumerator DissolveEffect() // ADICIÓN
//    {
//        float timer = 0f;
//        while (timer < dissolveDuration)
//        {
//            float value = Mathf.Lerp(0f, 1f, timer / dissolveDuration);
//            foreach (var mat in dissolveMaterials)
//                mat.SetFloat("_DissolveAmount", value);
//            timer += Time.deltaTime;
//            yield return null;
//        }
//        // asegurar valor final
//        foreach (var mat in dissolveMaterials)
//            mat.SetFloat("_DissolveAmount", 1f);
//    } // ADICIÓN


//    IEnumerator TimeToDead()
//    {

//        NavMeshAgent nav = GetComponent<NavMeshAgent>();
//        if (nav != null) nav.enabled = false;

//        if (animator != null) animator.SetTrigger("isDead");
//        yield return new WaitForSeconds(2f);

//        if (animator != null) animator.SetBool("Die", true);

//        Destroy(gameObject);
//    }

//    private IEnumerator AppearEffect()
//    {
//        // Inicializa los materiales con DissolveAmount = 1 (totalmente disuelto)
//        foreach (var mr in meshRenderers)
//            foreach (var mat in mr.materials)
//                if (mat.HasProperty("_DissolveAmount"))
//                    mat.SetFloat("_DissolveAmount", 1f);

//        foreach (var mr in staticMeshRenderers)
//            foreach (var mat in mr.materials)
//                if (mat.HasProperty("_DissolveAmount"))
//                    mat.SetFloat("_DissolveAmount", 1f);

//        float timer = 0f;
//        while (timer < dissolveDuration)
//        {
//            float value = Mathf.Lerp(1f, 0f, timer / dissolveDuration);
//            foreach (var mr in meshRenderers)
//                foreach (var mat in mr.materials)
//                    if (mat.HasProperty("_DissolveAmount"))
//                        mat.SetFloat("_DissolveAmount", value);

//            foreach (var mr in staticMeshRenderers)
//                foreach (var mat in mr.materials)
//                    if (mat.HasProperty("_DissolveAmount"))
//                        mat.SetFloat("_DissolveAmount", value);

//            timer += Time.deltaTime;
//            yield return null;
//        }
//        // Asegura el valor final
//        foreach (var mr in meshRenderers)
//            foreach (var mat in mr.materials)
//                if (mat.HasProperty("_DissolveAmount"))
//                    mat.SetFloat("_DissolveAmount", 0f);

//        foreach (var mr in staticMeshRenderers)
//            foreach (var mat in mr.materials)
//                if (mat.HasProperty("_DissolveAmount"))
//                    mat.SetFloat("_DissolveAmount", 0f);
//    }

//    private void OnEnable()
//    {

//        StartCoroutine(AppearEffect());
//    }

//    private void OnDestroy()
//    {
//        if (TutorialManager0.Instance != null && TutorialManager0.Instance.currentDialogueIndex == 7)
//        {
//            Debug.Log("ConfirmAdvance llamado por enemigo muerto");
//            TutorialManager0.Instance.StartCoroutine(TutorialManager0.Instance.ActivateTransitionBetweenTwoCameras(2, 3.5f, 3, 2.5f));
//            TutorialManager0.Instance.ConfirmAdvance();
//        }


//    }
//}
