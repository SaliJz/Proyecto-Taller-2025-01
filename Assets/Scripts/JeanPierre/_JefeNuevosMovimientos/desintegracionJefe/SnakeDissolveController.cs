using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(100)]  // Asegura que este script corra después de SnakeController
[RequireComponent(typeof(SnakeController))]
public class SnakeDissolveController : MonoBehaviour
{
    [Header("Configuración de Dissolve")]
    [Tooltip("Cuando es true, los materiales aumentan su DissolveAmount hasta 1; cuando false, vuelven a 0.")]
    public bool dissolveActive = false;
    [Tooltip("Velocidad a la que varía el DissolveAmount (por segundo).")]
    public float dissolveSpeed = 1f;
    [Tooltip("Nombre de la propiedad en el shader.")]
    public string dissolveProperty = "_DissolveAmount";

    private SnakeController snake;
    private List<Material> segmentMaterials = new List<Material>();
    private float currentTarget = 0f;

    void Awake()
    {
        snake = GetComponent<SnakeController>();
    }

    void Start()
    {
        // Espera hasta que SnakeController haya instanciado segmentos,
        // destruye las membranas y luego recopila materiales.
        StartCoroutine(InitWhenReady());
    }

    private IEnumerator InitWhenReady()
    {
        // Espera hasta que al menos la cabeza esté instanciada
        yield return new WaitUntil(() => snake.Segmentos != null && snake.Segmentos.Count > 0);

        DestroyMembranas();
        CollectMaterials();
    }

    /// <summary>
    /// Busca en cada segmento y sus hijos los GameObjects llamados
    /// "Membrana" o "Membrana2" y los destruye.
    /// </summary>
    private void DestroyMembranas()
    {
        foreach (Transform segmento in snake.Segmentos)
        {
            // Incluye hijos inactivos
            Transform[] allChildren = segmento.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                if (child.name == "Membrana" || child.name == "Membrana2")
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }

    private void CollectMaterials()
    {
        segmentMaterials.Clear();

        foreach (Transform segmento in snake.Segmentos)
        {
            Renderer[] rends = segmento.GetComponentsInChildren<Renderer>(includeInactive: true);
            foreach (Renderer rend in rends)
            {
                foreach (var mat in rend.materials)
                {
                    if (mat.HasProperty(dissolveProperty))
                        segmentMaterials.Add(mat);
                    else
                        Debug.LogWarning($"Material '{mat.name}' no tiene propiedad '{dissolveProperty}'");
                }
            }
        }

        // Inicializar al valor actual para evitar saltos
        currentTarget = dissolveActive ? 1f : 0f;
        foreach (var mat in segmentMaterials)
            mat.SetFloat(dissolveProperty, currentTarget);
    }

    void Update()
    {
        float desired = dissolveActive ? 1f : 0f;
        if (!Mathf.Approximately(desired, currentTarget))
            currentTarget = desired;

        float delta = dissolveSpeed * Time.deltaTime;
        foreach (var mat in segmentMaterials)
        {
            float c = mat.GetFloat(dissolveProperty);
            float n = Mathf.MoveTowards(c, currentTarget, delta);
            mat.SetFloat(dissolveProperty, n);
        }
    }

    /// <summary>
    /// Llamar si se añaden/remueven segmentos dinámicamente
    /// </summary>
    public void ResetAndRecollect()
    {
        StartCoroutine(InitWhenReady());
    }
}
