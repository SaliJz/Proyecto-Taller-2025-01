// CobraElevadaController.cs
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class CobraElevadaController : MonoBehaviour
{
    public enum Estado { Inactivo, Pose, Coletazo }

    [Header("Pose de cobra")]
    public bool poseAlInicio = false;
    public float alturaMaxima = 2f;
    [Range(0, 180)] public float anguloCurvo = 90f;
    [Min(2)] public int numSegmentosCuello = 8;
    public float velocidadSuavizado = 10f;

    [Header("Coletazo")]
    public float duracionColetazo = 2f;      // Duración total del coletazo
    public float distanciaColetazo = 1f;     // Radio del coletazo

    [Header("Automático")]
    [Tooltip("Distancia mínima (en unidades) al jugador para entrar en pose automáticamente.")]
    public float distanciaActivacionPose = 5f;
    [Tooltip("Tiempo (en segundos) que permaneces en pose antes de iniciar el coletazo.")]
    public float tiempoEsperaColetazo = 2f;

    private SnakeController serpent;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform jugador;

    // Para el coletazo tipo “soga unida”
    private List<Vector3> coletazoDirections;
    private float angleAcumulado;
    private float timerColetazo;
    private bool coletazoRealizado;

    // Temporizador interno para controlar el tiempo en Pose antes de iniciar el Coletazo
    private float timerPose;

    // Nueva referencia al activador de efectos
    private ActivadorEfectos efectosActivator;

    void Start()
    {
        serpent = GetComponent<SnakeController>();
        // Buscamos el ActivadorEfectos en la escena
        efectosActivator = FindObjectOfType<ActivadorEfectos>();

        GameObject p = GameObject.FindWithTag("Player");
        if (p == null)
        {
            Debug.LogError("No se encontró Player.");
        }
        else
        {
            jugador = p.transform;
        }

        if (poseAlInicio && serpent.Segmentos != null && serpent.Segmentos.Count > 0)
        {
            // Apagamos efectos al entrar en pose al inicio
            if (efectosActivator != null) efectosActivator.activar = false;
            EntrarPose();
        }
    }

    void Update()
    {
        if (serpent == null || serpent.Segmentos == null || serpent.Segmentos.Count == 0 || jugador == null)
            return;

        if (estado == Estado.Inactivo)
        {
            // Activamos efectos al quedar inactivo
            if (efectosActivator != null) efectosActivator.activar = true;

            float distAlJugador = Vector3.Distance(serpent.Segmentos[0].position, jugador.position);
            if (distAlJugador <= distanciaActivacionPose)
            {
                // Apagamos efectos antes de cambiar a pose
                if (efectosActivator != null) efectosActivator.activar = false;
                EntrarPose();
                return;
            }
        }
        else if (estado == Estado.Pose)
        {
            timerPose += Time.deltaTime;
            if (timerPose >= tiempoEsperaColetazo && !coletazoRealizado)
                IniciarColetazo();
            AplicarPose();
        }
        else if (estado == Estado.Coletazo)
        {
            AplicarColetazo();
        }
        else
        {
            serpent.enabled = true;
        }
    }

    void EntrarPose()
    {
        segmentos = serpent.Segmentos;
        int maxNeck = Mathf.Max(2, segmentos.Count - 1);
        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, maxNeck);
        estado = Estado.Pose;
        serpent.enabled = false;
        timerPose = 0f;
        coletazoRealizado = false;

        // Aseguramos que los efectos queden apagados
        if (efectosActivator != null) efectosActivator.activar = false;
    }

    void SalirPose()
    {
        estado = Estado.Inactivo;
        serpent.enabled = true;
        coletazoRealizado = false;
        // Reactivamos efectos al salir de pose
        if (efectosActivator != null) efectosActivator.activar = true;
    }

    void IniciarColetazo()
    {
        coletazoDirections = new List<Vector3>(segmentos.Count);
        for (int i = 0; i < segmentos.Count; i++)
        {
            if (i == 0)
                coletazoDirections.Add(Vector3.zero);
            else
                coletazoDirections.Add((segmentos[i].position - segmentos[i - 1].position).normalized);
        }
        angleAcumulado = 0f;
        timerColetazo = 0f;
        coletazoRealizado = true;
        estado = Estado.Coletazo;
    }

    void AplicarPose()
    {
        if (segmentos == null || segmentos.Count == 0)
            return;

        Vector3 cabeza2D = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 jug2D = new Vector3(jugador.position.x, 0, jugador.position.z);

        Vector3 dir = (jug2D - cabeza2D).sqrMagnitude < 0.01f
                          ? transform.forward
                          : (jug2D - cabeza2D).normalized;

        Vector3 objetivo = cabeza2D + Vector3.up * alturaMaxima;

        SeguirCadena(objetivo, dir, velocidadSuavizado);
    }

    void AplicarColetazo()
    {
        // Durante el ataque, la cabeza siempre mira al jugador
        Transform cabeza = segmentos[0];
        Vector3 lookTarget = new Vector3(jugador.position.x, cabeza.position.y, jugador.position.z);
        cabeza.LookAt(lookTarget);

        timerColetazo += Time.deltaTime;
        float deltaAngle = (360f / duracionColetazo) * Time.deltaTime;
        angleAcumulado += deltaAngle;

        for (int i = numSegmentosCuello; i < segmentos.Count; i++)
        {
            Vector3 prevPos = segmentos[i - 1].position;
            Vector3 rotDir = Quaternion.Euler(0, angleAcumulado, 0) * coletazoDirections[i];
            segmentos[i].position = prevPos + rotDir * distanciaColetazo;
        }

        if (timerColetazo >= duracionColetazo)
            SalirPose();
    }

    void SeguirCadena(Vector3 cabezaObj, Vector3 dirXZ, float velocidad)
    {
        float separacion = serpent.separacionSegmentos;
        Vector3 derecha = Vector3.Cross(dirXZ, Vector3.up).normalized;

        segmentos[0].position = Vector3.Lerp(
            segmentos[0].position,
            cabezaObj,
            velocidad * Time.deltaTime
        );

        Vector3 anterior = segmentos[0].position;
        for (int i = 1; i < segmentos.Count; i++)
        {
            float t = i < numSegmentosCuello
                      ? (float)i / (numSegmentosCuello - 1)
                      : 1f;

            Vector3 curva = i < numSegmentosCuello
                            ? Quaternion.AngleAxis(anguloCurvo * t, derecha) * dirXZ
                            : dirXZ;
            float altura = i < numSegmentosCuello
                           ? Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * alturaMaxima
                           : 0f;

            Vector3 dir = new Vector3(curva.x, 0, curva.z).normalized;
            Vector3 meta = anterior - dir * separacion;
            meta.y = altura;

            segmentos[i].position = Vector3.Lerp(
                segmentos[i].position,
                meta,
                velocidad * Time.deltaTime
            );

            Vector3 mira = anterior - segmentos[i].position;
            mira.y = 0;
            if (mira.sqrMagnitude > 0.001f)
            {
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(mira),
                    velocidad * Time.deltaTime
                );
            }

            anterior = segmentos[i].position;
        }
    }
}







