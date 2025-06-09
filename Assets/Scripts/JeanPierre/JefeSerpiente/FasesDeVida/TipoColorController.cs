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
    public float delayInicial = 0.6f; // Espera antes de asignar MeshRenderers

    // currentTipo se mantiene privado, pero se expone mediante una propiedad pública de solo lectura
    private TipoEnemigo currentTipo;
    public TipoEnemigo CurrentTipo => currentTipo;

    private List<MeshRenderer> meshRenderers = new List<MeshRenderer>();
    private Coroutine colorRoutine;
    private bool isBlinking = false;

    void Start()
    {
        // Inicia inicialización retardada
        StartCoroutine(InicializarConRetraso());
    }

    private IEnumerator InicializarConRetraso()
    {
        // Espera delayInicial segundos antes de recolectar MeshRenderers y comenzar
        yield return new WaitForSeconds(delayInicial);

        // Recolecta todos los MeshRenderer en este objeto y sus hijos
        meshRenderers.AddRange(GetComponentsInChildren<MeshRenderer>());

        // Inicializa color basado en tipo inicial y programa cambios periódicos
        ActualizarTipoYColor();
        InvokeRepeating(nameof(ActualizarTipoYColor), intervaloCambio, intervaloCambio);
    }

    private void ActualizarTipoYColor()
    {
        // Selecciona un tipo aleatorio
        int rand = Random.Range(0, System.Enum.GetValues(typeof(TipoEnemigo)).Length);
        currentTipo = (TipoEnemigo)rand;

        // Determina color destino según tipo
        Color targetColor = currentTipo == TipoEnemigo.Ametralladora ? Color.blue
                          : currentTipo == TipoEnemigo.Pistola ? Color.red
                          : Color.green;

        // Inicia transición suave
        if (colorRoutine != null)
            StopCoroutine(colorRoutine);
        colorRoutine = StartCoroutine(CambiarColorSuave(targetColor));
    }

    private IEnumerator CambiarColorSuave(Color targetColor)
    {
        float elapsed = 0f;
        Color startColor = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;

        while (elapsed < duracionTransicionColor)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duracionTransicionColor);
            Color lerpCol = Color.Lerp(startColor, targetColor, t);
            foreach (var rend in meshRenderers)
                rend.material.color = lerpCol;
            yield return null;
        }

        // Asegura color final exacto
        foreach (var rend in meshRenderers)
            rend.material.color = targetColor;
    }

    public void RecibirDanio(float d)
    {
        if (isBlinking) return;
        StartCoroutine(Parpadeo());
        // Aquí podrías procesar la resta de vida si fuera necesario
    }

    private IEnumerator Parpadeo()
    {
        isBlinking = true;
        Color baseColor = meshRenderers.Count > 0 ? meshRenderers[0].material.color : Color.white;
        float half = blinkInterval * 0.5f;

        for (int i = 0; i < blinkCount; i++)
        {
            foreach (var rend in meshRenderers)
                rend.material.color = Color.white;
            yield return new WaitForSeconds(half);
            foreach (var rend in meshRenderers)
                rend.material.color = baseColor;
            yield return new WaitForSeconds(half);
        }

        isBlinking = false;
    }
}



















