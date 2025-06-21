// TipoColorHDRController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TipoColorHDRController : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

    [Header("Configuración")]
    public float intervaloCambio = 10f;
    public float duracionTransicionColor = 1f;
    public int blinkCount = 4;
    public float blinkInterval = 0.1f;
    public float delayInicial = 0.6f; // Espera antes de asignar SkinnedMeshRenderers

    private TipoEnemigo currentTipo;
    public TipoEnemigo CurrentTipo => currentTipo;

    private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
    private Coroutine colorRoutine;
    private bool isBlinking = false;

    void Start()
    {
        StartCoroutine(InicializarConRetraso());
    }

    private IEnumerator InicializarConRetraso()
    {
        yield return new WaitForSeconds(delayInicial);

        // Recolecta todos los SkinnedMeshRenderer en este objeto y sus hijos
        skinnedRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());

        ActualizarTipoYColor();
        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
    }

    private void ActualizarTipoYColor()
    {
        int rand = Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
        currentTipo = (TipoEnemigo)rand;

        Color targetColor = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
                          : currentTipo == TipoEnemigo.Pistola ? Color.red
                          : Color.green;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
    }

    private IEnumerator CambiarColorSuave(Color targetColor)
    {
        float elapsed = 0f;
        Color startColor = skinnedRenderers.Count > 0 ? skinnedRenderers[0].sharedMaterial.color : Color.white;

        while (elapsed < duracionTransicionColor)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
            Color lerpCol = Color.Lerp(startColor, targetColor, t);
            foreach (var rend in skinnedRenderers)
            {
                // Asegúrate de usar instancias si modificas material en tiempo de ejecución
                rend.material.color = lerpCol;
            }
            yield return null;
        }

        foreach (var rend in skinnedRenderers)
            rend.material.color = targetColor;
    }

    public void RecibirDanio(float d)
    {
        if (isBlinking) return;
        StartCoroutine(Parpadeo());
        // Procesar vida si es necesario
    }

    private IEnumerator Parpadeo()
    {
        isBlinking = true;

        List<Color> baseColors = new List<Color>();
        List<Color> baseHDR = new List<Color>();

        foreach (var rend in skinnedRenderers)
        {
            baseColors.Add(rend.material.color);
            if (rend.material.HasProperty("_EmissionColor"))
                baseHDR.Add(rend.material.GetColor("_EmissionColor"));
            else
                baseHDR.Add(Color.black);
        }

        float half = blinkInterval * 0.5f;

        for (int i = 0; i < blinkCount; i++)
        {
            // Blanco
            foreach (var rend in skinnedRenderers)
            {
                rend.material.color = Color.white;
                if (rend.material.HasProperty("_EmissionColor"))
                    rend.material.SetColor("_EmissionColor", Color.white);
            }
            yield return new WaitForSeconds(half);

            // Restaurar
            for (int idx = 0; idx < skinnedRenderers.Count; idx++)
            {
                var rend = skinnedRenderers[idx];
                rend.material.color = baseColors[idx];
                if (rend.material.HasProperty("_EmissionColor"))
                    rend.material.SetColor("_EmissionColor", baseHDR[idx]);
            }
            yield return new WaitForSeconds(half);
        }

        isBlinking = false;
    }
}


//// TipoColorHDRController.cs
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class TipoColorHDRController : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración")]
//    public float intervaloCambio = 10f;
//    public float duracionTransicionColor = 1f;
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;
//    public float delayInicial = 0.6f; // Espera antes de asignar MeshRenderers

//    // currentTipo se mantiene privado, pero se expone mediante una propiedad pública de solo lectura
//    private TipoEnemigo currentTipo;
//    public TipoEnemigo CurrentTipo => currentTipo;

//    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
//    private Coroutine colorRoutine;
//    private bool isBlinking = false;

//    void Start()
//    {
//        // Inicia inicialización retardada
//        StartCoroutine(InicializarConRetraso());
//    }

//    private IEnumerator InicializarConRetraso()
//    {
//        // Espera delayInicial segundos antes de recolectar MeshRenderers y comenzar
//        yield return new WaitForSeconds(delayInicial);

//        // Recolecta todos los MeshRenderer en este objeto y sus hijos
//        meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());

//        // Inicializa color basado en tipo inicial y programa cambios periódicos
//        ActualizarTipoYColor();
//        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
//    }

//    private void ActualizarTipoYColor()
//    {
//        // Selecciona un tipo aleatorio
//        int rand = Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
//        currentTipo = (TipoEnemigo)rand;

//        // Determina color destino según tipo
//        Color targetColor = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
//                          : currentTipo == TipoEnemigo.Pistola ? Color.red
//                          : Color.green;

//        // Inicia transición suave
//        if (colorRoutine != null)
//            StopCoroutine(colorRoutine);
//        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
//    }

//    private IEnumerator CambiarColorSuave(Color targetColor)
//    {
//        float elapsed = 0f;
//        Color startColor = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;

//        while (elapsed < duracionTransicionColor)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
//            Color lerpCol = Color.Lerp(startColor, targetColor, t);
//            foreach (var rend in meshRenderers)
//                rend.material.color = lerpCol;
//            yield return null;
//        }

//        // Asegura color final exacto
//        foreach (var rend in meshRenderers)
//            rend.material.color = targetColor;
//    }

//    public void RecibirDanio(float d)
//    {
//        if (isBlinking) return;
//        StartCoroutine(Parpadeo());
//        // Aquí podrías procesar la resta de vida si fuera necesario
//    }

//    private IEnumerator Parpadeo()
//    {
//        isBlinking = true;

//        // Guardamos los colores base (difuso) y HDR (emisión) de cada MeshRenderer
//        List<Color> baseColors = new List<Color>();
//        List<Color> baseHDR = new List<Color>();

//        foreach (var rend in meshRenderers)
//        {
//            // Color difuso
//            baseColors.Add(rend.material.color);

//            // Color HDR (emisión); si no existe la propiedad, guardamos negro
//            if (rend.material.HasProperty("_EmissionColor"))
//                baseHDR.Add(rend.material.GetColor("_EmissionColor"));
//            else
//                baseHDR.Add(Color.black);
//        }

//        float half = blinkInterval * 0.5f;

//        for (int i = 0; i < blinkCount; i++)
//        {
//            // 1) Ponemos difuso = blanco y HDR = blanco
//            foreach (var rend in meshRenderers)
//            {
//                rend.material.color = Color.white;
//                if (rend.material.HasProperty("_EmissionColor"))
//                    rend.material.SetColor("_EmissionColor", Color.white);
//            }
//            yield return new WaitForSeconds(half);

//            // 2) Restauramos difuso y HDR originales
//            for (int idx = 0; idx < meshRenderers.Count; idx++)
//            {
//                var rend = meshRenderers[idx];
//                rend.material.color = baseColors[idx];
//                if (rend.material.HasProperty("_EmissionColor"))
//                    rend.material.SetColor("_EmissionColor", baseHDR[idx]);
//            }
//            yield return new WaitForSeconds(half);
//        }

//        isBlinking = false;
//    }
//}
