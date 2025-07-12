// AnimacionAnticipacionCola.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class AnimacionAnticipacionCola : MonoBehaviour
{
    [Header("Parametros de anticipacion")]
    [Tooltip("Cuantos segmentos desde la cola se animaran")]
    public int segmentosAnticipacion = 4;
    [Tooltip("Offset lateral maximo (unidades)")]
    public float offsetLateral = 0.3f;
    [Tooltip("Offset hacia atras maximo (unidades)")]
    public float offsetAtras = 0.5f;
    [Tooltip("Duracion de la fase de carga (s)")]
    public float duracionCarga = 0.5f;
    [Tooltip("Duracion de la fase de pulso rapido (s)")]
    public float duracionPulso = 0.2f;
    [Tooltip("Retraso tras la anticipacion antes de seguir (s)")]
    public float retrasoTrasAnticipacion = 0.05f;

    private SnakeController snake;
    private bool enRutina = false;

    void Awake()
    {
        snake = GetComponent<SnakeController>();
        if (snake == null)
            Debug.LogError("AnimacionAnticipacionCola: falta SnakeController.");
    }

    /// <summary>
    /// Ejecuta la rutina de anticipacion: recarga cola hacia un lado y atras, pulsa y restaura.
    /// </summary>
    public IEnumerator RutinaAnticipacion()
    {
        if (enRutina || snake == null || snake.Segmentos.Count == 0)
            yield break;

        enRutina = true;
        List<Transform> segs = snake.Segmentos;
        int total = segs.Count;
        int inicio = Mathf.Max(0, total - segmentosAnticipacion);

        // Guardar posiciones originales
        Vector3[] posOriginal = new Vector3[total - inicio];
        for (int i = inicio; i < total; i++)
            posOriginal[i - inicio] = segs[i].position;

        // Vectores globales de desplazamiento
        Vector3 lateral = transform.right * offsetLateral;
        Vector3 atras = -transform.forward * offsetAtras;

        float t = 0f;
        // 1) Carga: mover suavemente hacia un lado y atras
        while (t < duracionCarga)
        {
            float f = Mathf.SmoothStep(0f, 1f, t / duracionCarga);
            Vector3 desp = (lateral + atras) * Mathf.Sin(f * Mathf.PI * 0.5f);
            for (int i = inicio; i < total; i++)
                segs[i].position = posOriginal[i - inicio] + desp;
            t += Time.deltaTime;
            yield return null;
        }

        // 2) Pulso rapido: un pequeño tiron de regreso y rebote
        t = 0f;
        while (t < duracionPulso)
        {
            float pulse = Mathf.Sin(t / duracionPulso * Mathf.PI * 2f) * 0.1f;
            Vector3 desp = lateral + atras + (transform.forward * pulse);
            for (int i = inicio; i < total; i++)
                segs[i].position = posOriginal[i - inicio] + desp;
            t += Time.deltaTime;
            yield return null;
        }

        // 3) Restaurar posiciones originales
        for (int i = inicio; i < total; i++)
            segs[i].position = posOriginal[i - inicio];

        // 4) Retraso final antes de continuar
        yield return new WaitForSeconds(retrasoTrasAnticipacion);
        enRutina = false;
    }
}
