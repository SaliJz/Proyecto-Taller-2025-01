using UnityEngine;
using System.Collections;

[AddComponentMenu("Custom/ScaleUpOnCreate")]
public class ScaleUpOnCreate : MonoBehaviour
{
    [Header("Scale Settings")]
    [Tooltip("Target maximum scale on each axis")]
    public Vector3 maxScale = new Vector3(1f, 1f, 1f);

    [Tooltip("Speed at which to scale towards the target")]
    public float scaleSpeed = 1f;

    [Tooltip("If true, scaling begins automatically in Start()")]
    public bool autoStart = true;

    [Header("Fade Settings")]
    [Tooltip("Duration of fade-out after scaling")]
    public float fadeDuration = 1f;

    // Internal state
    private Vector3 _initialScale;
    private bool _isScaling = false;

    // Rendering components for fading
    private Renderer _renderer;
    private SpriteRenderer _spriteRenderer;
    private CanvasGroup _canvasGroup;

    void Awake()
    {
        // Cache original scale
        _initialScale = transform.localScale;
        // Cache rendering components
        _renderer = GetComponent<Renderer>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _canvasGroup = GetComponent<CanvasGroup>();
    }

    void Start()
    {
        if (autoStart)
        {
            BeginScaling();
        }
    }

    /// <summary>
    /// Manually begin scaling at runtime.
    /// </summary>
    public void BeginScaling()
    {
        if (!_isScaling)
        {
            _isScaling = true;
            StopAllCoroutines();
            StartCoroutine(ScaleRoutine());
        }
    }

    private IEnumerator ScaleRoutine()
    {
        float progress = 0f;
        Vector3 startScale = _initialScale;
        Vector3 targetScale = maxScale;

        if (scaleSpeed <= 0f)
        {
            Debug.LogWarning("Scale speed must be greater than zero.");
            yield break;
        }

        while (progress < 1f)
        {
            progress += Time.deltaTime * scaleSpeed;
            float eased = Mathf.SmoothStep(0f, 1f, progress);
            transform.localScale = Vector3.Lerp(startScale, targetScale, eased);
            yield return null;
        }

        // Ensure exact final scale
        transform.localScale = targetScale;
        _isScaling = false;

        // Begin fade-out and destroy sequence
        StartCoroutine(FadeAndDestroy());
    }

    private IEnumerator FadeAndDestroy()
    {
        float elapsed = 0f;
        // Determine initial alpha values
        float startAlpha = 1f;

        if (_canvasGroup != null)
        {
            startAlpha = _canvasGroup.alpha;
        }
        else if (_spriteRenderer != null)
        {
            startAlpha = _spriteRenderer.color.a;
        }
        else if (_renderer != null && _renderer.material.HasProperty("_Color"))
        {
            startAlpha = _renderer.material.color.a;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeDuration);

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = alpha;
            }
            else if (_spriteRenderer != null)
            {
                Color c = _spriteRenderer.color;
                c.a = alpha;
                _spriteRenderer.color = c;
            }
            else if (_renderer != null && _renderer.material.HasProperty("_Color"))
            {
                Color c = _renderer.material.color;
                c.a = alpha;
                _renderer.material.color = c;
            }

            yield return null;
        }

        // Ensure fully transparent before destroying
        if (_canvasGroup != null)
            _canvasGroup.alpha = 0f;
        else if (_spriteRenderer != null)
        {
            Color c = _spriteRenderer.color;
            c.a = 0f;
            _spriteRenderer.color = c;
        }
        else if (_renderer != null && _renderer.material.HasProperty("_Color"))
        {
            Color c = _renderer.material.color;
            c.a = 0f;
            _renderer.material.color = c;
        }

        Destroy(gameObject);
    }
}
