// SequentialSliderFill.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SequentialSliderFill : MonoBehaviour
{
    [Header("Referencias a los sliders")]
    public Slider slider1;
    public Slider slider2;
    public Slider slider3;

    [Header("Referencia a la imagen (debe tener Image componente)")]
    public Image targetImage;

    [Header("Duración de llenado de cada slider (segundos)")]
    public float fillDuration = 1f;

    [Header("Retraso entre sliders (segundos)")]
    public float delayBetween = 0.2f;

    [Header("Curva de impacto (bounce)")]
    public AnimationCurve impactCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Duración del fade-in de la imagen (segundos)")]
    public float imageFadeDuration = 1f;

    // Indicador de que ya terminó toda la animación
    [HideInInspector] public bool sequenceComplete = false;
    // Evento para notificar a otros scripts
    public Action OnSequenceComplete;

    private void Start()
    {
        // Inicializamos sliders e imagen
        if (slider1 != null) slider1.value = 0f;
        if (slider2 != null) slider2.value = 0f;
        if (slider3 != null) slider3.value = 0f;

        if (targetImage != null)
        {
            var c = targetImage.color;
            c.a = 0f;
            targetImage.color = c;
        }

        StartCoroutine(FillSlidersSequentially());
    }

    private IEnumerator FillSlidersSequentially()
    {
        Slider[] sliders = new Slider[] { slider1, slider2, slider3 };

        foreach (var s in sliders)
        {
            if (s != null)
                yield return StartCoroutine(FillSlider(s));
            yield return new WaitForSeconds(delayBetween);
        }

        if (targetImage != null)
            yield return StartCoroutine(FadeInImage());

        // Marcamos y disparamos evento
        sequenceComplete = true;
        OnSequenceComplete?.Invoke();
    }

    private IEnumerator FillSlider(Slider s)
    {
        float elapsed = 0f;
        while (elapsed < fillDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fillDuration);
            s.value = impactCurve.Evaluate(t);
            yield return null;
        }
        s.value = 1f;
    }

    private IEnumerator FadeInImage()
    {
        float elapsed = 0f;
        Color c = targetImage.color;
        while (elapsed < imageFadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / imageFadeDuration);
            c.a = t;
            targetImage.color = c;
            yield return null;
        }
        c.a = 1f;
        targetImage.color = c;
    }
}







//using System.Collections;
//using UnityEngine;
//using UnityEngine.UI;

//public class SequentialSliderFill : MonoBehaviour
//{
//    [Header("Referencias a los sliders")]
//    public Slider slider1;
//    public Slider slider2;
//    public Slider slider3;

//    [Header("Referencia a la imagen (debe tener Image componente)")]
//    public Image targetImage;

//    [Header("Duración de llenado de cada slider (segundos)")]
//    public float fillDuration = 1f;

//    [Header("Retraso entre sliders (segundos)")]
//    public float delayBetween = 0.2f;

//    [Header("Curva de impacto (bounce)")]
//    public AnimationCurve impactCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

//    [Header("Duración del fade-in de la imagen (segundos)")]
//    public float imageFadeDuration = 1f;

//    private void Start()
//    {
//        // Slider a 0
//        slider1.value = slider2.value = slider3.value = 0;
//        // Imagen invisible al inicio
//        if (targetImage != null)
//        {
//            Color c = targetImage.color;
//            c.a = 0;
//            targetImage.color = c;
//        }

//        StartCoroutine(FillSlidersSequentially());
//    }

//    private IEnumerator FillSlidersSequentially()
//    {
//        Slider[] sliders = new Slider[] { slider1, slider2, slider3 };

//        foreach (var s in sliders)
//        {
//            yield return StartCoroutine(FillSlider(s));
//            yield return new WaitForSeconds(delayBetween);
//        }

//        // Cuando terminen los 3, hacemos el fade-in
//        if (targetImage != null)
//            yield return StartCoroutine(FadeInImage());
//    }

//    private IEnumerator FillSlider(Slider s)
//    {
//        float elapsed = 0f;
//        while (elapsed < fillDuration)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / fillDuration);
//            s.value = impactCurve.Evaluate(t);
//            yield return null;
//        }
//        s.value = 1f;
//    }

//    private IEnumerator FadeInImage()
//    {
//        float elapsed = 0f;
//        Color c = targetImage.color;
//        while (elapsed < imageFadeDuration)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / imageFadeDuration);
//            c.a = t;
//            targetImage.color = c;
//            yield return null;
//        }
//        c.a = 1f;
//        targetImage.color = c;
//    }
//}
