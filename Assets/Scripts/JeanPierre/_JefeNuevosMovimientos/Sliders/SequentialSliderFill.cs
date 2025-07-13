// SequentialSliderFill.cs
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;  // Para TextMeshPro

public class SequentialSliderFill : MonoBehaviour
{
    [Header("Referencias a los sliders")]
    public Slider slider1;
    public Slider slider2;
    public Slider slider3;

    [Header("Referencia a la imagen principal (debe tener Image componente)")]
    public Image targetImage;

    [Header("Referencia a la imagen extra (fade-in junto al texto)")]
    public Image extraImage;

    [Header("Referencia al texto del nombre del jefe (TextMeshPro)")]
    public TextMeshProUGUI bossNameText;

    [Header("Duración de llenado de cada slider (segundos)")]
    public float fillDuration = 1f;
    [Header("Retraso entre sliders (segundos)")]
    public float delayBetween = 0.2f;
    [Header("Curva de impacto (bounce) para sliders")]
    public AnimationCurve impactCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Duración del fade-in de la imagen principal (segundos)")]
    public float imageFadeDuration = 1f;

    [Header("Animación del texto del jefe y imagen extra")]
    [Tooltip("Duración combinada de fade y escala")]
    public float textAnimDuration = 1f;
    [Tooltip("Curva de animación para fade y escala del texto y fade de la imagen extra")]
    public AnimationCurve textAnimCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [Tooltip("Escala inicial del texto antes de hacer bounce")]
    public float textInitialScale = 0.8f;
    [Tooltip("Escala final del texto")]
    public float textFinalScale = 1f;

    [HideInInspector] public bool sequenceComplete = false;
    public Action OnSequenceComplete;

    private void Start()
    {
        // Inicializar sliders
        if (slider1 != null) slider1.value = 0f;
        if (slider2 != null) slider2.value = 0f;
        if (slider3 != null) slider3.value = 0f;

        // Inicializar imagen principal transparente
        if (targetImage != null)
        {
            var c = targetImage.color;
            c.a = 0f;
            targetImage.color = c;
        }

        // Inicializar imagen extra transparente
        if (extraImage != null)
        {
            var c = extraImage.color;
            c.a = 0f;
            extraImage.color = c;
        }

        // Inicializar texto del jefe: transparente y a escala inicial
        if (bossNameText != null)
        {
            var tc = bossNameText.color;
            tc.a = 0f;
            bossNameText.color = tc;
            bossNameText.transform.localScale = Vector3.one * textInitialScale;
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

        // Animar simultáneamente el texto y la imagen extra
        if (bossNameText != null || extraImage != null)
            yield return StartCoroutine(AnimateTextAndExtraImage());

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

    private IEnumerator AnimateTextAndExtraImage()
    {
        float elapsed = 0f;
        Color textColor = bossNameText != null ? bossNameText.color : Color.white;
        Vector3 initialScale = Vector3.one * textInitialScale;
        Vector3 finalScale = Vector3.one * textFinalScale;

        Color imgColor = extraImage != null ? extraImage.color : Color.white;

        while (elapsed < textAnimDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / textAnimDuration);
            float curveT = textAnimCurve.Evaluate(t);

            // Fade-in de opacidad del texto
            if (bossNameText != null)
            {
                textColor.a = curveT;
                bossNameText.color = textColor;
                // Bounce de escala del texto
                bossNameText.transform.localScale = Vector3.LerpUnclamped(initialScale, finalScale, curveT);
            }

            // Fade-in de opacidad de la imagen extra
            if (extraImage != null)
            {
                imgColor.a = curveT;
                extraImage.color = imgColor;
            }

            yield return null;
        }

        // Valores finales
        if (bossNameText != null)
        {
            textColor.a = 1f;
            bossNameText.color = textColor;
            bossNameText.transform.localScale = finalScale;
        }
        if (extraImage != null)
        {
            imgColor.a = 1f;
            extraImage.color = imgColor;
        }
    }
}


//// SequentialSliderFill.cs
//using System;
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

//    // Indicador de que ya terminó toda la animación
//    [HideInInspector] public bool sequenceComplete = false;
//    // Evento para notificar a otros scripts
//    public Action OnSequenceComplete;

//    private void Start()
//    {
//        // Inicializamos sliders e imagen
//        if (slider1 != null) slider1.value = 0f;
//        if (slider2 != null) slider2.value = 0f;
//        if (slider3 != null) slider3.value = 0f;

//        if (targetImage != null)
//        {
//            var c = targetImage.color;
//            c.a = 0f;
//            targetImage.color = c;
//        }

//        StartCoroutine(FillSlidersSequentially());
//    }

//    private IEnumerator FillSlidersSequentially()
//    {
//        Slider[] sliders = new Slider[] { slider1, slider2, slider3 };

//        foreach (var s in sliders)
//        {
//            if (s != null)
//                yield return StartCoroutine(FillSlider(s));
//            yield return new WaitForSeconds(delayBetween);
//        }

//        if (targetImage != null)
//            yield return StartCoroutine(FadeInImage());

//        // Marcamos y disparamos evento
//        sequenceComplete = true;
//        OnSequenceComplete?.Invoke();
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






