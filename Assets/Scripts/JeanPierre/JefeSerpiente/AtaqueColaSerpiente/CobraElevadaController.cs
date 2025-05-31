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

    void Start()
    {
        serpent = GetComponent<SnakeController>();
        GameObject p = GameObject.FindWithTag("Player");
        if (p == null)
        {
            Debug.LogError("No se encontró Player.");
        }
        else
        {
            jugador = p.transform;
        }

        if (poseAlInicio)
        {
            // Solo intentamos entrar en pose si ya tenemos segmentos cargados
            if (serpent.Segmentos != null && serpent.Segmentos.Count > 0)
                EntrarPose();
        }
    }

    void Update()
    {
        // Si no hay SnakeController o si aún no se han generado los segmentos, no hacemos nada
        if (serpent == null || serpent.Segmentos == null || serpent.Segmentos.Count == 0 || jugador == null)
            return;

        // 1) Si estamos Inactivos, revisamos distancia al jugador
        if (estado == Estado.Inactivo)
        {
            // Accedemos a la cabeza (primer elemento de la lista)
            Vector3 cabezaPos = serpent.Segmentos[0].position;
            float distAlJugador = Vector3.Distance(cabezaPos, jugador.position);

            if (distAlJugador <= distanciaActivacionPose)
            {
                EntrarPose();
                return; // Salimos para no procesar nada más este frame
            }
        }
        // 2) Si estamos en Pose, vamos sumando tiempo y aplicamos pose. Cuando se cumpla el tiempo, lanzamos coletazo.
        else if (estado == Estado.Pose)
        {
            // Contamos el tiempo que llevamos en Pose
            timerPose += Time.deltaTime;

            // Si ya pasaron los segundos necesarios y aún no se hizo el coletazo, lo iniciamos
            if (timerPose >= tiempoEsperaColetazo && !coletazoRealizado)
            {
                IniciarColetazo();
            }

            // Aplicamos la pose en cada frame mientras estemos en este estado
            AplicarPose();
        }
        // 3) Si estamos en Coletazo, aplicamos la animación correspondiente
        else if (estado == Estado.Coletazo)
        {
            AplicarColetazo();
        }
        // 4) En cualquier otro caso (Inactivo), dejamos el SnakeController habilitado para que siga moviéndose normalmente
        else
        {
            serpent.enabled = true;
        }
    }

    void EntrarPose()
    {
        // Preparamos la lista de segmentos (solo la primera vez, o en cada entrada a Pose)
        segmentos = serpent.Segmentos;

        // Nos aseguramos de que el máximo cuello sea al menos 2 para evitar divisiones por cero
        int maxNeck = Mathf.Max(2, segmentos.Count - 1);
        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, maxNeck);

        estado = Estado.Pose;
        serpent.enabled = false;

        // Reiniciamos temporizadores internos
        timerPose = 0f;
        coletazoRealizado = false;
    }

    void SalirPose()
    {
        estado = Estado.Inactivo;
        serpent.enabled = true;
        coletazoRealizado = false;
        // No es necesario reiniciar timerPose aquí; se reinicia al entrar en pose nuevamente
    }

    void IniciarColetazo()
    {
        // Preparamos las direcciones originales de cada segmento para el coletazo “tipo soga”
        coletazoDirections = new List<Vector3>(segmentos.Count);
        for (int i = 0; i < segmentos.Count; i++)
        {
            if (i == 0)
            {
                coletazoDirections.Add(Vector3.zero);
            }
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
        if (segmentos == null || segmentos.Count == 0)
            return;

        // Calculamos la posición de la cabeza en 2D (ignorando Y)
        Vector3 cabeza2D = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
        Vector3 jug2D = new Vector3(jugador.position.x, 0, jugador.position.z);

        // Dirección desde la cabeza hacia el jugador (en XZ)
        Vector3 dir = (jug2D - cabeza2D).sqrMagnitude < 0.01f
                          ? transform.forward
                          : (jug2D - cabeza2D).normalized;

        // El objetivo en Y es alturaMaxima sobre el suelo
        Vector3 objetivo = cabeza2D + Vector3.up * alturaMaxima;

        SeguirCadena(objetivo, dir, velocidadSuavizado);
    }

    void AplicarColetazo()
    {
        timerColetazo += Time.deltaTime;
        float deltaAngle = (360f / duracionColetazo) * Time.deltaTime;
        angleAcumulado += deltaAngle;

        // Para cada segmento a partir de numSegmentosCuello, giramos en círculo según colectazoDirections
        for (int i = numSegmentosCuello; i < segmentos.Count; i++)
        {
            Vector3 prevPos = segmentos[i - 1].position;
            Vector3 rotDir = Quaternion.Euler(0, angleAcumulado, 0) * coletazoDirections[i];
            segmentos[i].position = prevPos + rotDir * distanciaColetazo;
        }

        // Cuando termine el coletazo, salimos de la pose y volvemos a Inactivo
        if (timerColetazo >= duracionColetazo)
        {
            SalirPose();
        }
    }

    void SeguirCadena(Vector3 cabezaObj, Vector3 dirXZ, float velocidad)
    {
        float separacion = serpent.separacionSegmentos;
        Vector3 derecha = Vector3.Cross(dirXZ, Vector3.up).normalized;

        // Mover suavizado de la cabeza hacia el objetivo (hacia arriba sobre el jugador)
        segmentos[0].position = Vector3.Lerp(
            segmentos[0].position,
            cabezaObj,
            velocidad * Time.deltaTime
        );

        Vector3 anterior = segmentos[0].position;
        for (int i = 1; i < segmentos.Count; i++)
        {
            // Si estamos en el cuello (i < numSegmentosCuello), aplicamos curva y altura; en cola es línea recta
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

            // Orientar cada segmento para que mire hacia el anterior (sin afectar la Y)
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

//    [Header("Coletazo")]
//    public float duracionColetazo = 2f;      // Duración total del coletazo
//    public float distanciaColetazo = 1f;     // Radio del coletazo

//    private SnakeController serpent;
//    private List<Transform> segmentos;
//    private Estado estado = Estado.Inactivo;
//    private Transform jugador;

//    // Para el coletazo tipo “soga unida”
//    private List<Vector3> coletazoDirections;
//    private float angleAcumulado;
//    private float timerColetazo;
//    private bool coletazoRealizado;

//    void Start()
//    {
//        serpent = GetComponent<SnakeController>();
//        GameObject p = GameObject.FindWithTag("Player");
//        if (p == null) Debug.LogError("No se encontró Player.");
//        else jugador = p.transform;

//        if (poseAlInicio) EntrarPose();
//    }

//    void Update()
//    {
//        if (jugador == null) return;

//        if (Input.GetKeyDown(KeyCode.L))
//        {
//            if (estado == Estado.Inactivo) EntrarPose();
//            else SalirPose();
//        }

//        if (Input.GetKeyDown(KeyCode.K) && estado == Estado.Pose && !coletazoRealizado)
//            IniciarColetazo();

//        if (estado == Estado.Pose) AplicarPose();
//        else if (estado == Estado.Coletazo) AplicarColetazo();
//        else serpent.enabled = true;
//    }

//    void EntrarPose()
//    {
//        segmentos = serpent.Segmentos;
//        numSegmentosCuello = Mathf.Clamp(numSegmentosCuello, 2, segmentos.Count - 1);
//        estado = Estado.Pose;
//        serpent.enabled = false;
//    }

//    void SalirPose()
//    {
//        estado = Estado.Inactivo;
//        serpent.enabled = true;
//        coletazoRealizado = false;
//    }

//    void IniciarColetazo()
//    {
//        // Guarda la dirección desde cada segmento al anterior
//        coletazoDirections = new List<Vector3>(segmentos.Count);
//        for (int i = 0; i < segmentos.Count; i++)
//        {
//            if (i == 0) coletazoDirections.Add(Vector3.zero);
//            else
//            {
//                Vector3 dir = (segmentos[i].position - segmentos[i - 1].position).normalized;
//                coletazoDirections.Add(dir);
//            }
//        }
//        angleAcumulado = 0f;
//        timerColetazo = 0f;
//        coletazoRealizado = true;
//        estado = Estado.Coletazo;
//    }

//    void AplicarPose()
//    {
//        if (segmentos == null) return;
//        Vector3 cabeza2D = new Vector3(segmentos[0].position.x, 0, segmentos[0].position.z);
//        Vector3 jug2D = new Vector3(jugador.position.x, 0, jugador.position.z);
//        Vector3 dir = (jug2D - cabeza2D).sqrMagnitude < 0.01f
//                          ? transform.forward : (jug2D - cabeza2D).normalized;
//        Vector3 objetivo = cabeza2D + Vector3.up * alturaMaxima;
//        SeguirCadena(objetivo, dir, velocidadSuavizado);
//    }

//    void AplicarColetazo()
//    {
//        timerColetazo += Time.deltaTime;
//        float deltaAngle = (360f / duracionColetazo) * Time.deltaTime;
//        angleAcumulado += deltaAngle;

//        // Para cada segmento de cola, rota su dirección original y aplica distancia
//        for (int i = numSegmentosCuello; i < segmentos.Count; i++)
//        {
//            Vector3 prevPos = segmentos[i - 1].position;
//            Vector3 rotDir = Quaternion.Euler(0, angleAcumulado, 0) * coletazoDirections[i];
//            segmentos[i].position = prevPos + rotDir * distanciaColetazo;
//        }

//        if (timerColetazo >= duracionColetazo)
//            SalirPose();
//    }

//    void SeguirCadena(Vector3 cabezaObj, Vector3 dirXZ, float velocidad)
//    {
//        float separacion = serpent.separacionSegmentos;
//        Vector3 derecha = Vector3.Cross(dirXZ, Vector3.up).normalized;

//        segmentos[0].position = Vector3.Lerp(
//            segmentos[0].position, cabezaObj, velocidad * Time.deltaTime
//        );

//        Vector3 anterior = segmentos[0].position;
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float t = i < numSegmentosCuello
//                      ? (float)i / (numSegmentosCuello - 1)
//                      : 1f;
//            Vector3 curva = i < numSegmentosCuello
//                            ? Quaternion.AngleAxis(anguloCurvo * t, derecha) * dirXZ
//                            : dirXZ;
//            float altura = i < numSegmentosCuello
//                           ? Mathf.Sin((1f - t) * Mathf.PI * 0.5f) * alturaMaxima
//                           : 0f;
//            Vector3 dir = new Vector3(curva.x, 0, curva.z).normalized;
//            Vector3 meta = anterior - dir * separacion;
//            meta.y = altura;
//            segmentos[i].position = Vector3.Lerp(
//                segmentos[i].position, meta, velocidad * Time.deltaTime
//            );
//            Vector3 mira = anterior - segmentos[i].position;
//            mira.y = 0;
//            if (mira.sqrMagnitude > 0.001f)
//                segmentos[i].rotation = Quaternion.Slerp(
//                    segmentos[i].rotation,
//                    Quaternion.LookRotation(mira),
//                    velocidad * Time.deltaTime
//                );
//            anterior = segmentos[i].position;
//        }
//    }
//}













