// TipoColorHDRController.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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

        skinnedRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());

        ActualizarTipoYColor();
        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
    }

    private void ActualizarTipoYColor()
    {
        int rand = Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
        currentTipo = (TipoEnemigo)rand;

        // Ametralladora = azul, Pistola = verde, Escopeta = rojo
        Color targetColor = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
                          : currentTipo == TipoEnemigo.Pistola ? Color.green
                          : /* Escopeta */                            Color.red;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
    }

    private IEnumerator CambiarColorSuave(Color targetColor)
    {
        // Determina qué renderizadores soportan la propiedad "_Color"
        var supportsColor = skinnedRenderers
            .Select(r => r.material.HasProperty("_Color"))
            .ToList();

        // Obtiene colores iniciales por renderer
        var baseColors = skinnedRenderers
            .Select((r, i) => supportsColor[i]
                ? r.material.GetColor("_Color")
                : Color.white)
            .ToList();

        // Color de inicio para lerp
        Color startColor = baseColors.FirstOrDefault();

        float elapsed = 0f;
        while (elapsed < duracionTransicionColor)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
            Color lerpCol = Color.Lerp(startColor, targetColor, t);

            for (int i = 0; i < skinnedRenderers.Count; i++)
            {
                var rend = skinnedRenderers[i];
                if (supportsColor[i])
                    rend.material.SetColor("_Color", lerpCol);
            }
            yield return null;
        }

        // Asegura color final
        for (int i = 0; i < skinnedRenderers.Count; i++)
        {
            var rend = skinnedRenderers[i];
            if (supportsColor[i])
                rend.material.SetColor("_Color", targetColor);
        }
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

        // Detecta soporte para _Color y _EmissionColor
        var supportsColor = skinnedRenderers
            .Select(r => r.material.HasProperty("_Color"))
            .ToList();
        var supportsHDR = skinnedRenderers
            .Select(r => r.material.HasProperty("_EmissionColor"))
            .ToList();

        // Guarda valores originales
        var baseColors = skinnedRenderers
            .Select((r, i) => supportsColor[i]
                ? r.material.GetColor("_Color")
                : Color.white)
            .ToList();
        var baseHDR = skinnedRenderers
            .Select((r, i) => supportsHDR[i]
                ? r.material.GetColor("_EmissionColor")
                : Color.black)
            .ToList();

        float half = blinkInterval * 0.5f;

        for (int i = 0; i < blinkCount; i++)
        {
            // Parpadeo a blanco
            for (int j = 0; j < skinnedRenderers.Count; j++)
            {
                var rend = skinnedRenderers[j];
                if (supportsColor[j])
                    rend.material.SetColor("_Color", Color.white);
                if (supportsHDR[j])
                    rend.material.SetColor("_EmissionColor", Color.white);
            }
            yield return new WaitForSeconds(half);

            // Restaurar valores originales
            for (int j = 0; j < skinnedRenderers.Count; j++)
            {
                var rend = skinnedRenderers[j];
                if (supportsColor[j])
                    rend.material.SetColor("_Color", baseColors[j]);
                if (supportsHDR[j])
                    rend.material.SetColor("_EmissionColor", baseHDR[j]);
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
//using System.Linq;

//public class TipoColorHDRController : MonoBehaviour
//{
//    public enum TypeIvulnerability { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración")]
//    public float intervaloCambio = 10f;
//    public float duracionTransicionColor = 1f;
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;
//    public float delayInicial = 0.6f; // Espera antes de asignar SkinnedMeshRenderers

//    private TypeIvulnerability currentTipo;
//    public TypeIvulnerability CurrentTipo => currentTipo;

//    private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
//    private Coroutine colorRoutine;
//    private bool isBlinking = false;

//    void Start()
//    {
//        StartCoroutine(InicializarConRetraso());
//    }

//    private IEnumerator InicializarConRetraso()
//    {
//        yield return new WaitForSeconds(delayInicial);

//        skinnedRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());

//        ActualizarTipoYColor();
//        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
//    }

//    private void ActualizarTipoYColor()
//    {
//        int rand = Random.Range(0, System.Enum.GetValues(typeof(TypeIvulnerability)).Length);
//        currentTipo = (TypeIvulnerability)rand;

//        // Ametralladora = azul, Pistola = verde, Escopeta = rojo
//        Color targetColor = currentTipo == TypeIvulnerability.Ametralladora ? Color.blue
//                          : currentTipo == TypeIvulnerability.Pistola ? Color.green
//                          : /* Escopeta */                            Color.red;

//        if (colorRoutine != null)
//            StopCoroutine(colorRoutine);
//        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
//    }

//    private IEnumerator CambiarColorSuave(Color targetColor)
//    {
//        // Determina qué renderizadores soportan la propiedad "_Color"
//        var supportsColor = skinnedRenderers
//            .Select(r => r.material.HasProperty("_Color"))
//            .ToList();

//        // Obtiene colores iniciales por renderer
//        var baseColors = skinnedRenderers
//            .Select((r, i) => supportsColor[i]
//                ? r.material.GetColor("_Color")
//                : Color.white)
//            .ToList();

//        // Color de inicio para lerp
//        Color startColor = baseColors.FirstOrDefault();

//        float elapsed = 0f;
//        while (elapsed < duracionTransicionColor)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
//            Color lerpCol = Color.Lerp(startColor, targetColor, t);

//            for (int i = 0; i < skinnedRenderers.Count; i++)
//            {
//                var rend = skinnedRenderers[i];
//                if (supportsColor[i])
//                    rend.material.SetColor("_Color", lerpCol);
//            }
//            yield return null;
//        }

//        // Asegura color final
//        for (int i = 0; i < skinnedRenderers.Count; i++)
//        {
//            var rend = skinnedRenderers[i];
//            if (supportsColor[i])
//                rend.material.SetColor("_Color", targetColor);
//        }
//    }

//    public void RecibirDanio(float d)
//    {
//        if (isBlinking) return;
//        StartCoroutine(Parpadeo());
//        // Procesar vida si es necesario
//    }

//    private IEnumerator Parpadeo()
//    {
//        isBlinking = true;

//        // Detecta soporte para _Color y _EmissionColor
//        var supportsColor = skinnedRenderers
//            .Select(r => r.material.HasProperty("_Color"))
//            .ToList();
//        var supportsHDR = skinnedRenderers
//            .Select(r => r.material.HasProperty("_EmissionColor"))
//            .ToList();

//        // Guarda valores originales
//        var baseColors = skinnedRenderers
//            .Select((r, i) => supportsColor[i]
//                ? r.material.GetColor("_Color")
//                : Color.white)
//            .ToList();
//        var baseHDR = skinnedRenderers
//            .Select((r, i) => supportsHDR[i]
//                ? r.material.GetColor("_EmissionColor")
//                : Color.black)
//            .ToList();

//        float half = blinkInterval * 0.5f;

//        for (int i = 0; i < blinkCount; i++)
//        {
//            // Parpadeo a blanco
//            for (int j = 0; j < skinnedRenderers.Count; j++)
//            {
//                var rend = skinnedRenderers[j];
//                if (supportsColor[j])
//                    rend.material.SetColor("_Color", Color.white);
//                if (supportsHDR[j])
//                    rend.material.SetColor("_EmissionColor", Color.white);
//            }
//            yield return new WaitForSeconds(half);

//            // Restaurar valores originales
//            for (int j = 0; j < skinnedRenderers.Count; j++)
//            {
//                var rend = skinnedRenderers[j];
//                if (supportsColor[j])
//                    rend.material.SetColor("_Color", baseColors[j]);
//                if (supportsHDR[j])
//                    rend.material.SetColor("_EmissionColor", baseHDR[j]);
//            }
//            yield return new WaitForSeconds(half);
//        }

//        isBlinking = false;
//    }
//}
















































//// TipoColorHDRController.cs
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class TipoColorHDRController : MonoBehaviour
//{
//    public enum TypeIvulnerability { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración")]
//    public float intervaloCambio = 10f;
//    public float duracionTransicionColor = 1f;
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;
//    public float delayInicial = 0.6f; // Espera antes de asignar SkinnedMeshRenderers

//    private TypeIvulnerability currentTipo;
//    public TypeIvulnerability CurrentTipo => currentTipo;

//    private List<SkinnedMeshRenderer> skinnedRenderers = new List<SkinnedMeshRenderer>();
//    private Coroutine colorRoutine;
//    private bool isBlinking = false;

//    void Start()
//    {
//        StartCoroutine(InicializarConRetraso());
//    }

//    private IEnumerator InicializarConRetraso()
//    {
//        yield return new WaitForSeconds(delayInicial);

//        // Recolecta todos los SkinnedMeshRenderer en este objeto y sus hijos
//        skinnedRenderers.AddRange(GetComponentsInChildren<SkinnedMeshRenderer>());

//        ActualizarTipoYColor();
//        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
//    }

//    private void ActualizarTipoYColor()
//    {
//        int rand = Random.Range(0, System.Enum.GetValues(typeof(TypeIvulnerability)).Length);
//        currentTipo = (TypeIvulnerability)rand;

//        // Ametralladora = azul, Pistola = verde, Escopeta = rojo
//        Color targetColor = currentTipo == TypeIvulnerability.Ametralladora ? Color.blue
//                          : currentTipo == TypeIvulnerability.Pistola ? Color.green
//                          : /* Escopeta */                         Color.red;

//        if (colorRoutine != null)
//            StopCoroutine(colorRoutine);
//        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
//    }

//    private IEnumerator CambiarColorSuave(Color targetColor)
//    {
//        float elapsed = 0f;
//        Color startColor = skinnedRenderers.Count > 0 ? skinnedRenderers[0].sharedMaterial.color : Color.white;

//        while (elapsed < duracionTransicionColor)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
//            Color lerpCol = Color.Lerp(startColor, targetColor, t);
//            foreach (var rend in skinnedRenderers)
//            {
//                // Asegúrate de usar instancias si modificas material en tiempo de ejecución
//                rend.material.color = lerpCol;
//            }
//            yield return null;
//        }

//        foreach (var rend in skinnedRenderers)
//            rend.material.color = targetColor;
//    }

//    public void RecibirDanio(float d)
//    {
//        if (isBlinking) return;
//        StartCoroutine(Parpadeo());
//        // Procesar vida si es necesario
//    }

//    private IEnumerator Parpadeo()
//    {
//        isBlinking = true;

//        List<Color> baseColors = new List<Color>();
//        List<Color> baseHDR = new List<Color>();

//        foreach (var rend in skinnedRenderers)
//        {
//            baseColors.Add(rend.material.color);
//            if (rend.material.HasProperty("_EmissionColor"))
//                baseHDR.Add(rend.material.GetColor("_EmissionColor"));
//            else
//                baseHDR.Add(Color.black);
//        }

//        float half = blinkInterval * 0.5f;

//        for (int i = 0; i < blinkCount; i++)
//        {
//            // Blanco
//            foreach (var rend in skinnedRenderers)
//            {
//                rend.material.color = Color.white;
//                if (rend.material.HasProperty("_EmissionColor"))
//                    rend.material.SetColor("_EmissionColor", Color.white);
//            }
//            yield return new WaitForSeconds(half);

//            // Restaurar
//            for (int idx = 0; idx < skinnedRenderers.Count; idx++)
//            {
//                var rend = skinnedRenderers[idx];
//                rend.material.color = baseColors[idx];
//                if (rend.material.HasProperty("_EmissionColor"))
//                    rend.material.SetColor("_EmissionColor", baseHDR[idx]);
//            }
//            yield return new WaitForSeconds(half);
//        }

//        isBlinking = false;
//    }
//}











