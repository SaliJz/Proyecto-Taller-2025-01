// TipoColorController.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TipoColorController : MonoBehaviour
{
    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

    [Header("Configuración")]
    public float intervaloCambio = 10f;
    public float duracionTransicionColor = 1f;
    public int blinkCount = 4;
    public float blinkInterval = 0.1f;
    public float delayInicial = 0.6f;
    [Tooltip("Intensidad para HDR.")]
    public float emissionIntensity = 2f;

    private TipoEnemigo currentTipo;
    public TipoEnemigo CurrentTipo => currentTipo;

    private List<Renderer> renderers = new List<Renderer>();
    private Coroutine colorRoutine;
    private bool isBlinking = false;

    void Start()
    {
        StartCoroutine(InicializarConRetraso());
    }

    private IEnumerator InicializarConRetraso()
    {
        yield return new WaitForSeconds(delayInicial);

        // 1) Todos los Renderers hijos de este GameObject
        renderers.AddRange(GetComponentsInChildren<Renderer>());

        // 2) También busca en toda la escena cualquier objeto llamado "Holograma_1"
        foreach (var t in FindObjectsOfType<Transform>())
        {
            if (t.name == "Holograma_1")
                renderers.AddRange(t.GetComponentsInChildren<Renderer>());
        }

        // Habilita emisión en todos los materiales que tengan la propiedad
        foreach (var rend in renderers)
        {
            var mat = rend.material;
            if (mat.HasProperty("_EmissionColor"))
                mat.EnableKeyword("_EMISSION");
        }

        ActualizarTipoYColor();
        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
    }

    private void ActualizarTipoYColor()
    {
        currentTipo = (TipoEnemigo)Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);

        Color baseCol = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
                        : currentTipo == TipoEnemigo.Pistola ? Color.red
                                                              : Color.green;
        Color hdrCol = baseCol * emissionIntensity;

        if (colorRoutine != null)
            StopCoroutine(colorRoutine);

        colorRoutine = StartCoroutine(CambiarColorSuave(baseCol, hdrCol));
    }

    private IEnumerator CambiarColorSuave(Color targetBase, Color targetHDR)
    {
        float elapsed = 0f;
        // Captura colores iniciales de cada renderer
        var startBaseColors = new List<Color>(renderers.Count);
        var startHDRColors = new List<Color>(renderers.Count);

        foreach (var rend in renderers)
        {
            var mat = rend.material;
            Color sb = mat.HasProperty("_BaseColor") ? mat.GetColor("_BaseColor")
                   : mat.HasProperty("_Color") ? mat.GetColor("_Color")
                   : Color.white;
            startBaseColors.Add(sb);

            Color sh = mat.HasProperty("_EmissionColor") ? mat.GetColor("_EmissionColor")
                   : mat.HasProperty("_FresnelColor") ? mat.GetColor("_FresnelColor")
                   : sb * emissionIntensity;
            startHDRColors.Add(sh);
        }

        // Interpolación suave
        while (elapsed < duracionTransicionColor)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);

            for (int i = 0; i < renderers.Count; i++)
            {
                var rend = renderers[i];
                var mat = rend.material;

                Color lerpBase = Color.Lerp(startBaseColors[i], targetBase, t);
                Color lerpHDR = Color.Lerp(startHDRColors[i], targetHDR, t);

                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", lerpBase);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", lerpBase);

                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", lerpHDR);
                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", lerpHDR);
            }
            yield return null;
        }

        // Asegura valores finales exactos
        foreach (var rend in renderers)
        {
            var mat = rend.material;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", targetBase);
            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", targetBase);

            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", targetHDR);
            if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", targetHDR);
        }

        // Marca fin de coroutine
        colorRoutine = null;
    }

    public void RecibirDanio(float d)
    {
        if (isBlinking) return;
        StartCoroutine(Parpadeo());
    }

    private IEnumerator Parpadeo()
    {
        isBlinking = true;

        // Guarda colores actuales antes de parpadear
        var mat0 = renderers[0].material;
        Color currBase = mat0.HasProperty("_BaseColor") ? mat0.GetColor("_BaseColor")
                       : mat0.HasProperty("_Color") ? mat0.GetColor("_Color")
                                                    : Color.white;
        Color currHDR = mat0.HasProperty("_EmissionColor") ? mat0.GetColor("_EmissionColor")
                       : mat0.HasProperty("_FresnelColor") ? mat0.GetColor("_FresnelColor")
                                                            : currBase * emissionIntensity;

        float half = blinkInterval * 0.5f;
        for (int i = 0; i < blinkCount; i++)
        {
            // Blanco fuerte
            foreach (var rend in renderers)
            {
                var mat = rend.material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.white * emissionIntensity);
                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", Color.white * emissionIntensity);
            }
            yield return new WaitForSeconds(half);

            // Restablece a color previo
            foreach (var rend in renderers)
            {
                var mat = rend.material;
                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", currBase);
                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", currBase);

                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", currHDR);
                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", currHDR);
            }
            yield return new WaitForSeconds(half);
        }

        isBlinking = false;
    }
}














//// TipoColorController.cs
//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;

//public class TipoColorController : MonoBehaviour
//{
//    public enum TipoEnemigo { Ametralladora, Pistola, Escopeta }

//    [Header("Configuración")]
//    public float intervaloCambio = 10f;
//    public float duracionTransicionColor = 1f;
//    public int blinkCount = 4;
//    public float blinkInterval = 0.1f;
//    public float delayInicial = 0.6f;
//    [Tooltip("Intensidad para HDR.")]
//    public float emissionIntensity = 2f;

//    private TipoEnemigo currentTipo;
//    public TipoEnemigo CurrentTipo => currentTipo;

//    private List<Renderer> renderers = new List<Renderer>();
//    private Coroutine colorRoutine;
//    private bool isBlinking = false;

//    void Start()
//    {
//        StartCoroutine(InicializarConRetraso());
//    }

//    private IEnumerator InicializarConRetraso()
//    {
//        yield return new WaitForSeconds(delayInicial);

//        // 1) Todos los Renderers hijos de este GameObject
//        renderers.AddRange(GetComponentsInChildren<Renderer>());

//        // 2) También busca en toda la escena cualquier objeto llamado "Holograma_1"
//        foreach (var t in FindObjectsOfType<Transform>())
//        {
//            if (t.name == "Holograma_1")
//                renderers.AddRange(t.GetComponentsInChildren<Renderer>());
//        }

//        // Habilita emisión en todos los materiales que tengan la propiedad
//        foreach (var rend in renderers)
//        {
//            var mat = rend.material;
//            if (mat.HasProperty("_EmissionColor"))
//                mat.EnableKeyword("_EMISSION");
//        }

//        ActualizarTipoYColor();
//        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
//    }

//    private void ActualizarTipoYColor()
//    {
//        currentTipo = (TipoEnemigo)Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);

//        Color baseCol = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
//                        : currentTipo == TipoEnemigo.Pistola ? Color.red
//                                                                     : Color.green;
//        Color hdrCol = baseCol * emissionIntensity;

//        if (colorRoutine != null)
//            StopCoroutine(colorRoutine);

//        colorRoutine = StartCoroutine(CambiarColorSuave(baseCol, hdrCol));
//    }

//    private IEnumerator CambiarColorSuave(Color targetBase, Color targetHDR)
//    {
//        float elapsed = 0f;

//        var firstMat = renderers[0].material;
//        Color startBase = firstMat.HasProperty("_BaseColor") ? firstMat.GetColor("_BaseColor")
//                         : firstMat.HasProperty("_Color") ? firstMat.GetColor("_Color")
//                                                                  : Color.white;
//        Color startHDR = firstMat.HasProperty("_EmissionColor") ? firstMat.GetColor("_EmissionColor")
//                         : firstMat.HasProperty("_FresnelColor") ? firstMat.GetColor("_FresnelColor")
//                                                                  : startBase;

//        while (elapsed < duracionTransicionColor)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);

//            Color lerpBase = Color.Lerp(startBase, targetBase, t);
//            Color lerpHDR = Color.Lerp(startHDR, targetHDR, t);

//            foreach (var rend in renderers)
//            {
//                var mat = rend.material;
//                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", lerpBase);
//                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", lerpBase);

//                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", lerpHDR);
//                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", lerpHDR);
//            }
//            yield return null;
//        }

//        // Valores finales
//        foreach (var rend in renderers)
//        {
//            var mat = rend.material;
//            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", targetBase);
//            else if (mat.HasProperty("_Color")) mat.SetColor("_Color", targetBase);

//            if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", targetHDR);
//            if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", targetHDR);
//        }
//    }

//    public void RecibirDanio(float d)
//    {
//        if (isBlinking) return;
//        StartCoroutine(Parpadeo());
//    }

//    private IEnumerator Parpadeo()
//    {
//        isBlinking = true;

//        var mat0 = renderers[0].material;
//        Color currBase = mat0.HasProperty("_BaseColor") ? mat0.GetColor("_BaseColor")
//                       : mat0.HasProperty("_Color") ? mat0.GetColor("_Color")
//                                                            : Color.white;
//        Color currHDR = mat0.HasProperty("_EmissionColor") ? mat0.GetColor("_EmissionColor")
//                       : mat0.HasProperty("_FresnelColor") ? mat0.GetColor("_FresnelColor")
//                                                            : currBase * emissionIntensity;

//        float half = blinkInterval * 0.5f;
//        for (int i = 0; i < blinkCount; i++)
//        {
//            foreach (var rend in renderers)
//            {
//                var mat = rend.material;
//                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", Color.white);
//                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);

//                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", Color.white * emissionIntensity);
//                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", Color.white * emissionIntensity);
//            }
//            yield return new WaitForSeconds(half);

//            foreach (var rend in renderers)
//            {
//                var mat = rend.material;
//                if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", currBase);
//                else if (mat.HasProperty("_Color")) mat.SetColor("_Color", currBase);

//                if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", currHDR);
//                if (mat.HasProperty("_FresnelColor")) mat.SetColor("_FresnelColor", currHDR);
//            }
//            yield return new WaitForSeconds(half);
//        }

//        isBlinking = false;
//    }
//}









































