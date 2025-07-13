









using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CobraElevadaController))]
public class Fase3Vida : MonoBehaviour
{
    [Header("Vida total de la Cobra Elevada")]
    [Tooltip("Vida acumulada de todas las interacciones.")]
    public int vida = 300;

    [Header("Slider de vida")]
    [Tooltip("Muestra la vida restante de la Cobra Elevada.")]
    public Slider vidaSlider;

    [Header("Parpadeo al recibir daño")]
    public float blinkDuration = 0.2f;
    public float returnDuration = 0.1f;
    public float blinkFrequency = 10f;

    [Header("Daño por tipo de bala")]
    public int danioAmetralladora = 15;
    public int danioPistola = 20;
    public int danioEscopeta = 30;

    [Header("Scripts a activar al morir")]
    public MonoBehaviour[] scriptsToActivate;
    [Header("Scripts a eliminar al morir")]
    public MonoBehaviour[] scriptsToRemove;

    private bool isDead = false;
    private CobraElevadaController cobraController;
    private TipoColorController tipoColorController;

    private Image fillImage;
    private Color originalColor;
    private bool blinkInProgress = false;
    private float blinkTimer = 0f;

    void Start()
    {
        cobraController = GetComponent<CobraElevadaController>();
        if (cobraController == null)
            Debug.LogError("[Fase3Vida] Falta CobraElevadaController.");

        tipoColorController = GetComponent<TipoColorController>();
        if (tipoColorController == null)
            Debug.LogWarning("[Fase3Vida] Falta TipoColorController (parpadeo no funcionará).");

        if (vidaSlider != null)
        {
            vidaSlider.maxValue = vida;
            vidaSlider.value = vida;
            if (vidaSlider.fillRect != null)
            {
                fillImage = vidaSlider.fillRect.GetComponent<Image>();
                originalColor = fillImage.color;
            }
            else
            {
                Debug.LogWarning("[Fase3Vida] Slider.fillRect es null.");
            }
        }
        else
        {
            Debug.LogError("[Fase3Vida] Slider 'vidaSlider' NO está asignado en el Inspector.");
        }
    }

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tipoBala)
    {

        Debug.Log("[Fase3Vida] RecibirDanioPorBala INVOCADO");
        // Depuración: ver si entra al método
        Debug.Log($"[Fase3Vida] Llamado RecibirDanioPorBala con {tipoBala}. Vida antes: {vida}");

        if (isDead)
        {
            Debug.Log("[Fase3Vida] Ya está muerto, ignoro daño.");
            return;
        }

        // Comprueba color
        var balaComoEnemigo = (TipoColorController.TipoEnemigo)System.Enum.Parse(
            typeof(TipoColorController.TipoEnemigo),
            tipoBala.ToString()
        );
        if (tipoColorController != null && tipoColorController.CurrentTipo == balaComoEnemigo)
        {
            Debug.Log("[Fase3Vida] ¡Mismo color! No aplico daño.");
            return;
        }

        // Calcula daño
        int danioAplicado = tipoBala switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioPistola,
            BalaPlayer.TipoBala.Escopeta => danioEscopeta,
            _ => 0
        };

        vida = Mathf.Max(0, vida - danioAplicado);
        Debug.Log($"[Fase3Vida] Daño aplicado: {danioAplicado}. Vida ahora: {vida}");

        // Actualiza slider
        if (vidaSlider != null)
        {
            vidaSlider.value = vida;
        }

        // Parpadeo
        blinkTimer = 0f;
        if (!blinkInProgress && fillImage != null)
            StartCoroutine(BlinkRoutine());

        if (vida <= 0)
            Die();
    }

    private IEnumerator BlinkRoutine()
    {
        blinkInProgress = true;
        float total = blinkDuration + returnDuration;

        while (blinkTimer < total)
        {
            blinkTimer += Time.deltaTime;
            if (blinkTimer <= blinkDuration)
            {
                float t = Mathf.PingPong(blinkTimer * blinkFrequency, 1f);
                fillImage.color = Color.Lerp(originalColor, Color.white, t);
            }
            else
            {
                float t2 = (blinkTimer - blinkDuration) / returnDuration;
                fillImage.color = Color.Lerp(Color.white, originalColor, t2);
            }
            yield return null;
        }

        fillImage.color = originalColor;
        blinkInProgress = false;
    }

    private void Die()
    {
        isDead = true;
        Debug.Log("[Fase3Vida] ¡La cobra ha muerto!");

        foreach (var s in scriptsToActivate)
            if (s != null) s.enabled = true;
        foreach (var s in scriptsToRemove)
            if (s != null) Destroy(s);

        if (cobraController != null)
            cobraController.enabled = false;

        if (vidaSlider != null)
            Destroy(vidaSlider.gameObject);

        Destroy(gameObject);
    }

    public bool IsDead() => isDead;
}


