using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Agrega comportamiento de cobra encima de SnakeController
[RequireComponent(typeof(SnakeController))]
public class ComportamientoCobraSerpiente : MonoBehaviour
{
    [Header("Configuracion Cobra")]
    public float distanciaDeteccion = 5f;
    public float duracionLevantarse = 1f;
    public float duracionAtaque = 0.5f;
    public float duracionRegreso = 1f;
    public float distanciaAtaque = 2f;
    public float distanciaRetirada = 8f;

    [Header("Ajuste de pose cobra")]
    public float alturaS = 1f;
    public float expansionCapucha = 0.5f;

    private SnakeController controladorSerpiente;
    private Transform jugadorReal;
    private List<Transform> segmentos;
    private List<Vector3> posicionesOriginales;

    private enum Estado { Inactivo, Levantandose, Atacando, Recuperando, Retirandose }
    private Estado estadoActual = Estado.Inactivo;

    void Awake()
    {
        controladorSerpiente = GetComponent<SnakeController>();
        jugadorReal = controladorSerpiente.jugador;
    }

    IEnumerator Start()
    {
        // Espera un frame para que SnakeController instancie hijos
        yield return null;

        // Cachear todos los segmentos (hijos directos): cabeza + cuerpo + cola
        int count = transform.childCount;
        segmentos = new List<Transform>(count);
        posicionesOriginales = new List<Vector3>(count);
        for (int i = 0; i < count; i++)
        {
            var seg = transform.GetChild(i);
            segmentos.Add(seg);
            posicionesOriginales.Add(seg.localPosition);
        }
    }

    void Update()
    {
        if (estadoActual == Estado.Inactivo && segmentos != null)
        {
            float d = Vector3.Distance(segmentos[0].position, jugadorReal.position);
            if (d <= distanciaDeteccion)
                StartCoroutine(EjecutarAtaqueCobra());
        }
    }

    IEnumerator EjecutarAtaqueCobra()
    {
        estadoActual = Estado.Levantandose;
        controladorSerpiente.enabled = false;

        Quaternion rotOriginal = Quaternion.identity;
        Quaternion rotObjetivo = Quaternion.Euler(-45f, 0f, 0f);

        // Levantarse: animar cada segmento con curva S
        float t = 0f;
        int n = segmentos.Count;
        while (t < duracionLevantarse)
        {
            float norm = t / duracionLevantarse;
            float sCurve = Mathf.Sin(norm * Mathf.PI);

            for (int i = 0; i < n; i++)
            {
                float frac = 1f - (float)i / (n - 1); // cabeza = 1, cola = 0
                var seg = segmentos[i];
                // Elevar
                seg.localPosition = posicionesOriginales[i] + Vector3.up * (sCurve * alturaS * frac);
                // Rotar
                seg.localRotation = Quaternion.Slerp(rotOriginal, rotObjetivo, norm * frac);
                // Escalar capucha sólo en cabeza
                if (i == 0)
                {
                    float scaleFactor = 1f + expansionCapucha * sCurve;
                    seg.localScale = Vector3.one * scaleFactor;
                }
            }

            t += Time.deltaTime;
            yield return null;
        }

        // Fijar valores finales de levantamiento
        for (int i = 0; i < n; i++)
        {
            float frac = 1f - (float)i / (n - 1);
            var seg = segmentos[i];
            seg.localPosition = posicionesOriginales[i] + Vector3.up * (alturaS * frac);
            seg.localRotation = Quaternion.Slerp(rotOriginal, rotObjetivo, frac);
            if (i == 0)
                seg.localScale = Vector3.one * (1f + expansionCapucha);
        }

        // Ataque: mover cabeza manteniendo forma S
        estadoActual = Estado.Atacando;
        t = 0f;
        Vector3 posHeadOriginal = posicionesOriginales[0] + Vector3.up * alturaS;
        Vector3 dirAtaque = (jugadorReal.position - segmentos[0].position).normalized;
        Vector3 posHeadAtaque = posHeadOriginal + dirAtaque * distanciaAtaque;
        while (t < duracionAtaque)
        {
            float normA = t / duracionAtaque;
            segmentos[0].localPosition = Vector3.Lerp(posHeadOriginal, posHeadAtaque, normA);
            t += Time.deltaTime;
            yield return null;
        }
        segmentos[0].localPosition = posHeadAtaque;

        // Recuperar: inverso levantarse
        estadoActual = Estado.Recuperando;
        t = 0f;
        while (t < duracionRegreso)
        {
            float normR = t / duracionRegreso;
            float sCurve = Mathf.Sin((1f - normR) * Mathf.PI);
            for (int i = 0; i < n; i++)
            {
                float frac = 1f - (float)i / (n - 1);
                var seg = segmentos[i];
                seg.localPosition = posicionesOriginales[i] + Vector3.up * (sCurve * alturaS * frac);
                seg.localRotation = Quaternion.Slerp(rotObjetivo, rotOriginal, normR * frac);
                if (i == 0)
                {
                    float scaleFactor = 1f + expansionCapucha * sCurve;
                    seg.localScale = Vector3.one * scaleFactor;
                }
            }
            t += Time.deltaTime;
            yield return null;
        }

        // Restaurar completamente
        for (int i = 0; i < n; i++)
        {
            var seg = segmentos[i];
            seg.localPosition = posicionesOriginales[i];
            seg.localRotation = rotOriginal;
            if (i == 0)
                seg.localScale = Vector3.one;
        }

        // Retirada
        estadoActual = Estado.Retirandose;
        GameObject objRetirada = new GameObject("ObjetivoRetirada");
        objRetirada.transform.position = transform.position + (transform.position - jugadorReal.position).normalized * distanciaRetirada;
        controladorSerpiente.jugador = objRetirada.transform;
        controladorSerpiente.enabled = true;

        // esperar a retirarse
        while (Vector3.Distance(transform.position, objRetirada.transform.position) > 0.1f)
            yield return null;

        controladorSerpiente.jugador = jugadorReal;
        Destroy(objRetirada);
        estadoActual = Estado.Inactivo;
    }
}
