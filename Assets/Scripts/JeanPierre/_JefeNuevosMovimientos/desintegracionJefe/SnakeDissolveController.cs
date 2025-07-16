//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class SnakeDissolveController : MonoBehaviour
//{
//    [Header("Configuración de Dissolve")]
//    [Tooltip("Cuando es true, los materiales aumentan su DissolveAmount hasta 1; cuando false, vuelven a 0.")]
//    public bool dissolveActive = false;
//    [Tooltip("Velocidad a la que varía el DissolveAmount (por segundo).")]
//    public float dissolveSpeed = 1f;
//    [Tooltip("Nombre de la propiedad en el shader.")]
//    public string dissolveProperty = "_DissolveAmount";

//    private SnakeController snake;
//    private List<Material> segmentMaterials = new List<Material>();
//    private float currentTarget = 0f;

//    void Awake()
//    {
//        snake = GetComponent<SnakeController>();
//    }

//    void Start()
//    {
//        // Asumiendo que SnakeController ya ha instanciado todos los segmentos en Awake,
//        // podemos destruir membranas y recolectar materiales inmediatamente.
//        DestroyMembranas();
//        CollectMaterials();
//    }

//    private void DestroyMembranas()
//    {
//        foreach (Transform segmento in snake.Segmentos)
//        {
//            // Incluye hijos inactivos
//            Transform[] allChildren = segmento.GetComponentsInChildren<Transform>(true);
//            foreach (Transform child in allChildren)
//            {
//                if (child.name == "Membrana" || child.name == "Membrana2")
//                {
//                    Destroy(child.gameObject);
//                }
//            }
//        }
//    }

//    private void CollectMaterials()
//    {
//        segmentMaterials.Clear();

//        foreach (Transform segmento in snake.Segmentos)
//        {
//            Renderer[] rends = segmento.GetComponentsInChildren<Renderer>(includeInactive: true);
//            foreach (Renderer rend in rends)
//            {
//                foreach (var mat in rend.materials)
//                {
//                    if (mat.HasProperty(dissolveProperty))
//                        segmentMaterials.Add(mat);
//                    else
//                        Debug.LogWarning($"Material '{mat.name}' no tiene propiedad '{dissolveProperty}'");
//                }
//            }
//        }

//        // Inicializar al valor actual para evitar saltos
//        currentTarget = dissolveActive ? 1f : 0f;
//        foreach (var mat in segmentMaterials)
//            mat.SetFloat(dissolveProperty, currentTarget);
//    }

//    void Update()
//    {
//        float desired = dissolveActive ? 1f : 0f;
//        if (!Mathf.Approximately(desired, currentTarget))
//            currentTarget = desired;

//        float delta = dissolveSpeed * Time.deltaTime;
//        foreach (var mat in segmentMaterials)
//        {
//            float c = mat.GetFloat(dissolveProperty);
//            float n = Mathf.MoveTowards(c, currentTarget, delta);
//            mat.SetFloat(dissolveProperty, n);
//        }
//    }
//}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]
[RequireComponent(typeof(SnakeController))]
public class SnakeDissolveController : MonoBehaviour
{
    [Header("Configuración de Dissolve")]
    public bool dissolveActive = false;
    public float dissolveDuration = 1f;
    public string dissolveProperty = "_DissolveAmount";

    private SnakeController snake;
    private List<Material> segmentMaterials = new List<Material>();
    private float currentTarget = 0f;

    void Awake()
    {
        snake = GetComponent<SnakeController>();
    }

    void OnEnable()
    {
        // Al activar el script, recaba materiales y arranca desde el estado actual (0 o 1)
        ResetAndRecollect();
    }

    private IEnumerator InitWhenReady()
    {
        yield return new WaitUntil(() => snake.Segmentos != null && snake.Segmentos.Count > 0);
        DestroyMembranas();
        CollectMaterials();
    }

    private void DestroyMembranas()
    {
        foreach (Transform segmento in snake.Segmentos)
        {
            foreach (Transform child in segmento.GetComponentsInChildren<Transform>(true))
                if (child.name == "Membrana" || child.name == "Membrana2")
                    Destroy(child.gameObject);
        }
    }

    private void CollectMaterials()
    {
        segmentMaterials.Clear();
        foreach (Transform segmento in snake.Segmentos)
        {
            Renderer[] rends = segmento.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer rend in rends)
                foreach (var mat in rend.materials)
                    if (mat.HasProperty(dissolveProperty))
                        segmentMaterials.Add(mat);
                    else
                        Debug.LogWarning($"Material '{mat.name}' sin propiedad '{dissolveProperty}'");
        }

        // Inicializa currentTarget al valor actual del shader (0 si nunca se disolvió)
        if (segmentMaterials.Count > 0)
            currentTarget = segmentMaterials[0].GetFloat(dissolveProperty);
    }

    void Update()
    {
        float desired = dissolveActive ? 1f : 0f;
        if (!Mathf.Approximately(desired, currentTarget))
        {
            float delta = Time.deltaTime / Mathf.Max(0.0001f, dissolveDuration);
            currentTarget = Mathf.MoveTowards(currentTarget, desired, delta);
            foreach (var mat in segmentMaterials)
                mat.SetFloat(dissolveProperty, currentTarget);
        }
    }

    public void ResetAndRecollect()
    {
        StopAllCoroutines();
        StartCoroutine(InitWhenReady());
    }
}
