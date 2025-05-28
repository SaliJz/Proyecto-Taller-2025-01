// CobraElevadaController.cs
using System.Collections;
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

    private SnakeController serpent;
    private List<Transform> segmentos;
    private Estado estado = Estado.Inactivo;
    private Transform jugador;

    // Para el coletazo tipo “soga unida”
    private List<Vector3> coletazoDirections;
    private float angleAcumulado;
    private float timerColetazo;
    private bool coletazoRealizado;

    void Start()
    {
        serpent = GetComponent<SnakeController>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null) Debug.LogError("No se encontró Player.");
        else jugador = p.transform;

        if (poseAlInicio) EntrarPose();
    }

    void Update()
    {
        if (jugador == null) return;

        if (Input.GetKeyDown(KeyCode.L))
        {
            if (estado == Estado.Inactivo) EntrarPose();
            else SalirPose();
        }

        if (Input.GetKeyDown(KeyCode.K) && estado == Estado.Pose && !coletazoRealizado)
            IniciarColetazo();

        if (estado == Estado.Pose) AplicarPose();
        else if (estado == Estado.Coletazo) AplicarColetazo();
        else serpent.enabled = true;
    }

    void EntrarPose()
    {
        segmentos = serpent.Segmentos;
        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, segmentos.Count - 1);
        estado = Estado.Pose;
        serpent.enabled = false;
    }

    void SalirPose()
    {
        estado = Estado.Inactivo;
        serpent.enabled = true;
        coletazoRealizado = false;
    }

    void IniciarColetazo()
    {
        // Guarda la dirección desde cada segmento al anterior
        coletazoDirections = new List<Vector3>(segmentos.Count);
        for (int i = 0; i < segmentos.Count; i++)
        {
            if (i == 0) coletazoDirections.Add(Vector3.zero);
            else
            {
                Vector3 dir = (segmentos[i].position - segmentos[i - 1].position).normalized;
                coletazoDirections.Add(dir);
            }
        }
        angleAcumulado = 0f;
        timerColetazo = 0f;
        coletazoRealizado = true;
        estado = Estado.Coletazo;
    }

    void AplicarPose()
    {
        if (segmentos == null) return;
        Vector3 cabeza2D = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 jug2D = new Vector3(jugador.position.x, 0, jugador.position.z);
        Vector3 dir = (jug2D - cabeza2D).sqrMagnitude < 0.01f
                          ? transform.forward : (jug2D - cabeza2D).normalized;
        Vector3 objetivo = cabeza2D + Vector3.up * alturaMaxima;
        SeguirCadena(objetivo, dir, velocidadSuavizado);
    }

    void AplicarColetazo()
    {
        timerColetazo += Time.deltaTime;
        float deltaAngle = (360f / duracionColetazo) * Time.deltaTime;
        angleAcumulado += deltaAngle;

        // Para cada segmento de cola, rota su dirección original y aplica distancia
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
            segmentos[0].position, cabezaObj, velocidad * Time.deltaTime
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
                segmentos[i].position, meta, velocidad * Time.deltaTime
            );
            Vector3 mira = anterior - segmentos[i].position;
            mira.y = 0;
            if (mira.sqrMagnitude > 0.001f)
                segmentos[i].rotation = Quaternion.Slerp(
                    segmentos[i].rotation,
                    Quaternion.LookRotation(mira),
                    velocidad * Time.deltaTime
                );
            anterior = segmentos[i].position;
        }
    }
}




















//// CobraElevadaController.cs
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraElevadaController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose, Coletazo }

//    [Header("Pose de cobra")]
//    public bool poseAlInicio = false;
//    public float alturaMaxima = 2f;
//    [Range(0, 180)] public float anguloCurvo = 90f;
//    [Min(2)] public int numSegmentosCuello = 8;
//    public float velocidadSuavizado = 10f;
//    public float duracionColetazo = 2f; // Duración del coletazo

//    private SnakeController serpent;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform jugador;
//    private float timerColetazo = 0f;
//    private bool coletazoRealizado = false;

//    void Start()
//    {
//        serpent = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null)
//            Debug.LogError("No se encontro ningun GameObject con tag \"Player\".");
//        else
//            jugador = p.transform;

//        if (poseAlInicio)
//            EntrarPose();
//    }

//    void Update()
//    {
//        if (jugador == null)
//            return;

//        // Tecla para alternar pose (pararse): L
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            if (estado == Estado.Inactivo)
//                EntrarPose();
//            else
//                SalirPose();
//        }

//        // Tecla para coletazo: K (solo en pose y si no se ha hecho)
//        if (Input.GetKeyDown(KeyCode.K) && estado == Estado.Pose && !coletazoRealizado)
//        {
//            IniciarColetazo();
//        }

//        // Lógica por estado
//        if (estado == Estado.Pose)
//            AplicarPose();
//        else if (estado == Estado.Coletazo)
//            AplicarColetazo();
//        else
//            serpent.enabled = true;
//    }

//    void EntrarPose()
//    {
//        segmentos = serpent.Segmentos;
//        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        serpent.enabled = false;  // detiene el movimiento normal
//    }

//    void SalirPose()
//    {
//        estado = Estado.Inactivo;
//        serpent.enabled = true;   // reanuda movimiento normal
//        coletazoRealizado = false;  // resetear la posibilidad de coletazo
//    }

//    void IniciarColetazo()
//    {
//        estado = Estado.Coletazo;
//        timerColetazo = 0f;
//        coletazoRealizado = true;  // marcar que el coletazo se está realizando
//    }

//    void AplicarPose()
//    {
//        if (segmentos == null) return;

//        Vector3 cabezaPlano = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 jugadorPlano = new Vector3(jugador.position.x, 0, jugador.position.z);
//        Vector3 direccion = (jugadorPlano - cabezaPlano).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (jugadorPlano - cabezaPlano).normalized;
//        Vector3 objetivoCabeza = cabezaPlano + Vector3.up * alturaMaxima;
//        SeguirCadena(objetivoCabeza, direccion, velocidadSuavizado);
//    }

//    void AplicarColetazo()
//    {
//        timerColetazo += Time.deltaTime;
//        float t = Mathf.Min(timerColetazo / duracionColetazo, 1f);
//        float rotacion = Mathf.Lerp(0, 360, t);

//        // Girar segmentos del cuerpo (excepto cabeza y cuello)
//        for (int i = numSegmentosCuello; i < segmentos.Count; i++)
//        {
//            segmentos[i].rotation = Quaternion.Euler(0, rotacion, 0);
//        }

//        // Al terminar el coletazo, volver a la pose normal
//        if (t >= 1f)
//        {
//            SalirPose();
//        }
//    }

//    void SeguirCadena(Vector3 cabezaObj, Vector3 dirXZ, float velocidad)
//    {
//        float separacion = serpent.separacionSegmentos;
//        Vector3 derecha = Vector3.Cross(dirXZ, Vector3.up).normalized;

//        segmentos[0].position = Vector3.Lerp(
//            segmentos[0].position,
//            cabezaObj,
//            velocidad * Time.deltaTime
//        );

//        Vector3 anterior = segmentos[0].position;

//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < numSegmentosCuello)
//                ? (float)i / (numSegmentosCuello - 1)
//                : 1f;

//            Vector3 curvatura = (i < numSegmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurvo * t, derecha) * dirXZ
//                : dirXZ;

//            float altura = (i < numSegmentosCuello)
//                ? Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * alturaMaxima
//                : 0f;

//            Vector3 curvXZ = new Vector3(curvatura.x, 0, curvatura.z).normalized;
//            Vector3 objetivo = anterior - curvXZ * separacion;
//            objetivo.y = altura;

//            segmentos[i].position = Vector3.Lerp(
//                segmentos[i].position,
//                objetivo,
//                velocidad * Time.deltaTime
//            );

//            Vector3 mirar = anterior - segmentos[i].position;
//            mirar.y = 0;
//            if (mirar.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(mirar),
//                    velocidad * Time.deltaTime
//                );

//            anterior = segmentos[i].position;
//        }
//    }
//}
































//// CobraElevadaController.cs
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//[RequireComponent(typeof(SnakeController))]
//public class CobraElevadaController : MonoBehaviour
//{
//    public enum Estado { Inactivo, Pose }

//    [Header("Pose de cobra")]
//    public bool poseAlInicio = false;
//    public float alturaMaxima = 2f;
//    [Range(0, 180)] public float anguloCurvo = 90f;
//    [Min(2)] public int numSegmentosCuello = 8;
//    public float velocidadSuavizado = 10f;

//    private SnakeController serpent;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform jugador;

//    void Start()
//    {
//        serpent = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null)
//            Debug.LogError("No se encontro ningun GameObject con tag \"Player\".");
//        else
//            jugador = p.transform;

//        if (poseAlInicio)
//            EntrarPose();
//    }

//    void Update()
//    {
//        if (jugador == null)
//            return;

//        // Toggle al presionar 'L'
//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            if (estado == Estado.Inactivo)
//                EntrarPose();
//            else
//                SalirPose();
//        }

//        if (estado == Estado.Pose)
//            AplicarPose();
//        else
//            serpent.enabled = true;
//    }

//    void EntrarPose()
//    {
//        segmentos = serpent.Segmentos;
//        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        serpent.enabled = false;  // detiene el movimiento normal
//    }

//    void SalirPose()
//    {
//        estado = Estado.Inactivo;
//        serpent.enabled = true;   // reanuda movimiento normal
//    }

//    void AplicarPose()
//    {
//        if (segmentos == null) return;

//        Vector3 cabezaPlano = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 jugadorPlano = new Vector3(jugador.position.x, 0, jugador.position.z);
//        Vector3 direccion = (jugadorPlano - cabezaPlano).sqrMagnitude < 0.01f
//            ? transform.forward
//            : (jugadorPlano - cabezaPlano).normalized;
//        Vector3 objetivoCabeza = cabezaPlano + Vector3.up * alturaMaxima;
//        SeguirCadena(objetivoCabeza, direccion, velocidadSuavizado);
//    }

//    void SeguirCadena(Vector3 cabezaObj, Vector3 dirXZ, float velocidad)
//    {
//        float separacion = serpent.separacionSegmentos;
//        Vector3 derecha = Vector3.Cross(dirXZ, Vector3.up).normalized;

//        // Mueve la cabeza
//        segmentos[0].position = Vector3.Lerp(
//            segmentos[0].position,
//            cabezaObj,
//            velocidad * Time.deltaTime
//        );

//        Vector3 anterior = segmentos[0].position;

//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = (i < numSegmentosCuello)
//                ? (float)i / (numSegmentosCuello - 1)
//                : 1f;

//            Vector3 curvatura = (i < numSegmentosCuello)
//                ? Quaternion.AngleAxis(anguloCurvo * t, derecha) * dirXZ
//                : dirXZ;

//            float altura = (i < numSegmentosCuello)
//                ? Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * alturaMaxima
//                : 0f;

//            Vector3 curvXZ = new Vector3(curvatura.x, 0, curvatura.z).normalized;
//            Vector3 objetivo = anterior - curvXZ * separacion;
//            objetivo.y = altura;

//            segmentos[i].position = Vector3.Lerp(
//                segmentos[i].position,
//                objetivo,
//                velocidad * Time.deltaTime
//            );

//            Vector3 mirar = anterior - segmentos[i].position;
//            mirar.y = 0;
//            if (mirar.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(mirar),
//                    velocidad * Time.deltaTime
//                );

//            anterior = segmentos[i].position;
//        }
//    }
//}

