using System.Collections.Generic;
using UnityEngine;

// Controla el comportamiento de una serpiente que sigue al jugador con movimiento serpenteante
public class SnakeController : MonoBehaviour
{
    [Header("Prefabs de la serpiente")]
    public GameObject prefabCabeza;
    public GameObject[] prefabsCuerpo;    // Array de prefabs de cuerpo
    public GameObject prefabCola;

    [Header("Configuraci�n de la serpiente")]
    public int cantidadSegmentosCuerpo = 10;           // N�mero de segmentos de cuerpo
    public float distanciaCabezaCuerpo = 0.5f;         // Distancia entre cabeza y primer segmento
    public float separacionCola = 0.5f;                // Distancia entre el �ltimo segmento de cuerpo y la cola
    [Tooltip("Mantener para compatibilidad con otros scripts")]
    public float separacionSegmentos = 0.5f;           // fallback para scripts externos
    [Tooltip("Separaci�n individual por cada prefab de cuerpo")]
    public float[] separacionesSegmentosCuerpo;        // Distancias espec�ficas entre cada segmento

    public float velocidad = 5f;                       // Velocidad de movimiento de la cabeza
    public float amplitudSerpenteo = 0.5f;             // Amplitud del movimiento serpenteante
    public float frecuenciaSerpenteo = 2f;             // Frecuencia del movimiento serpenteante
    public float ejeY = 1f;                            // Altura constante de la serpiente
    public float umbralDetencion = 0.1f;               // Distancia m�nima al jugador para detenerse
    public float velocidadRotacionCabeza = 10f;        // Velocidad de rotaci�n de la cabeza

    [Header("Referencia al jugador")]
    public Transform jugador;

    // Lista interna de segmentos: cabeza, cuerpos y cola
    private List<Transform> segmentos = new List<Transform>();
    public List<Transform> Segmentos => segmentos;     // Expuesta para uso externo

    // Historial de posiciones de la cabeza para desplazar el cuerpo
    private List<Vector3> posicionesCabeza = new List<Vector3>();
    private int maxRegistroPosiciones;

    void Start()
    {
        Vector3 posInicial = new Vector3(transform.position.x, ejeY, transform.position.z);
        maxRegistroPosiciones = Mathf.CeilToInt(
            (distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola) * 5f
        );

        // --- Instancia cabeza ---
        Transform cabeza = Instantiate(prefabCabeza, posInicial, Quaternion.identity, transform).transform;
        AsegurarColliderYHijos(cabeza);
        segmentos.Add(cabeza);

        // --- Instancia segmentos de cuerpo ---
        for (int i = 0; i < cantidadSegmentosCuerpo; i++)
        {
            GameObject prefab = (i < prefabsCuerpo.Length)
                ? prefabsCuerpo[i]
                : prefabsCuerpo[prefabsCuerpo.Length - 1];

            Transform cuerpo = Instantiate(prefab, posInicial, Quaternion.identity, transform).transform;
            AsegurarColliderYHijos(cuerpo);
            segmentos.Add(cuerpo);
        }

        // --- Instancia cola ---
        Transform cola = Instantiate(prefabCola, posInicial, Quaternion.identity, transform).transform;
        AsegurarColliderYHijos(cola);
        segmentos.Add(cola);

        // Rellenar historial de posiciones
        for (int i = 0; i < maxRegistroPosiciones; i++)
            posicionesCabeza.Add(posInicial);
    }

    void Update()
    {
        Transform cabeza = segmentos[0];
        Vector3 targetPos = new Vector3(jugador.position.x, ejeY, jugador.position.z);
        float distancia = Vector3.Distance(cabeza.position, targetPos);

        if (distancia > umbralDetencion)
            MoverCabeza(targetPos);
        else
            DetenerCabeza(targetPos);

        SeguirCuerpo();
    }

    void MoverCabeza(Vector3 objetivo)
    {
        Transform cabeza = segmentos[0];
        Vector3 dir = (objetivo - cabeza.position).normalized;
        dir.y = 0;

        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
        }

        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
        float serp = Mathf.Sin(Time.time * frecuenciaSerpenteo) * amplitudSerpenteo;
        Vector3 nuevaPos = cabeza.position + dir * velocidad * Time.deltaTime + perp * serp;
        nuevaPos.y = ejeY;
        cabeza.position = nuevaPos;

        RegistrarPosicion(nuevaPos);
    }

    void DetenerCabeza(Vector3 posicion)
    {
        Transform cabeza = segmentos[0];
        cabeza.position = posicion;

        Vector3 dir = (jugador.position - cabeza.position).normalized;
        dir.y = 0;
        if (dir != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
        }

        RegistrarPosicion(posicion);
    }

    void SeguirCuerpo()
    {
        for (int i = 1; i < segmentos.Count; i++)
        {
            bool esCola = (i == segmentos.Count - 1);
            float distanciaAtras = esCola
                ? distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola
                : distanciaCabezaCuerpo + SumaSeparacionesHasta(i - 1);

            Vector3 posObjetivo = ObtenerPosicionEnTrayectoria(distanciaAtras);
            Transform seg = segmentos[i];
            seg.position = Vector3.Lerp(seg.position, posObjetivo, Time.deltaTime * velocidad);

            Vector3 lookDirection = (posObjetivo - seg.position).normalized;
            lookDirection.y = 0;
            if (lookDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
                seg.rotation = Quaternion.Slerp(seg.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
            }
        }
    }

    Vector3 ObtenerPosicionEnTrayectoria(float distanciaAtras)
    {
        if (distanciaAtras <= 0f)
            return posicionesCabeza[0];

        float acumulado = 0f;
        for (int i = 0; i < posicionesCabeza.Count - 1; i++)
        {
            float d = Vector3.Distance(posicionesCabeza[i], posicionesCabeza[i + 1]);
            acumulado += d;
            if (acumulado >= distanciaAtras)
            {
                float overShoot = acumulado - distanciaAtras;
                float t = 1 - (overShoot / d);
                return Vector3.Lerp(posicionesCabeza[i], posicionesCabeza[i + 1], t);
            }
        }
        return posicionesCabeza[posicionesCabeza.Count - 1];
    }

    private float ObtenerDistanciaTotalCuerpo()
    {
        if (separacionesSegmentosCuerpo.Length == 0)
            return separacionSegmentos * cantidadSegmentosCuerpo;

        float suma = 0f;
        int count = Mathf.Min(cantidadSegmentosCuerpo, separacionesSegmentosCuerpo.Length);
        for (int i = 0; i < count; i++)
            suma += separacionesSegmentosCuerpo[i];
        if (cantidadSegmentosCuerpo > separacionesSegmentosCuerpo.Length)
            suma += (cantidadSegmentosCuerpo - separacionesSegmentosCuerpo.Length) * separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
        return suma;
    }

    private float SumaSeparacionesHasta(int index)
    {
        float suma = 0f;
        for (int k = 0; k <= index; k++)
        {
            if (k < separacionesSegmentosCuerpo.Length)
                suma += separacionesSegmentosCuerpo[k];
            else if (separacionesSegmentosCuerpo.Length > 0)
                suma += separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
            else
                suma += separacionSegmentos;
        }
        return suma;
    }

    /// <summary>
    /// A�ade al transform indicado un collider apropiado (Sphere o Box) si no lo tiene,
    /// y recorre tambi�n todos sus hijos para a�adirles el mismo tipo y tama�o de collider.
    /// </summary>
    void AsegurarColliderYHijos(Transform t)
    {
        // Primero el propio GameObject
        Collider padre = t.GetComponent<Collider>();
        if (padre == null)
        {
            // Por defecto BoxCollider
            padre = t.gameObject.AddComponent<BoxCollider>();
        }

        // Recorre hijos
        foreach (Transform hijo in t.GetComponentsInChildren<Transform>())
        {
            if (hijo == t) continue;
            if (hijo.GetComponent<Collider>() != null) continue;

            if (padre is SphereCollider sc)
            {
                SphereCollider nh = hijo.gameObject.AddComponent<SphereCollider>();
                nh.radius = sc.radius;
                nh.center = sc.center;
            }
            else if (padre is BoxCollider bc)
            {
                BoxCollider nh = hijo.gameObject.AddComponent<BoxCollider>();
                nh.size = bc.size;
                nh.center = bc.center;
            }
            else
            {
                // fallback gen�rico
                hijo.gameObject.AddComponent<BoxCollider>();
            }
        }
    }

    void RegistrarPosicion(Vector3 pos)
    {
        posicionesCabeza.Insert(0, pos);
        if (posicionesCabeza.Count > maxRegistroPosiciones)
            posicionesCabeza.RemoveAt(posicionesCabeza.Count - 1);
    }

    /// <summary>
    /// Limpia el historial de posiciones de la cabeza y lo rellena con la posici�n actual
    /// </summary>
    public void ResetPositionHistory()
    {
        if (segmentos == null || segmentos.Count == 0)
            return;

        Vector3 currentHeadPos = segmentos[0].position;
        posicionesCabeza.Clear();
        for (int i = 0; i < maxRegistroPosiciones; i++)
            posicionesCabeza.Add(currentHeadPos);
    }
}































//// SnakeController.cs
//using System.Collections.Generic;
//using UnityEngine;

//// Controla el comportamiento de una serpiente que sigue al jugador con movimiento serpenteante
//public class SnakeController : MonoBehaviour
//{
//    [Header("Prefabs de la serpiente")]
//    public GameObject prefabCabeza;
//    public GameObject[] prefabsCuerpo;    // Array de prefabs de cuerpo
//    public GameObject prefabCola;

//    [Header("Configuraci�n de la serpiente")]
//    public int cantidadSegmentosCuerpo = 10;           // N�mero de segmentos de cuerpo
//    public float distanciaCabezaCuerpo = 0.5f;       // Distancia entre cabeza y primer segmento
//    public float separacionCola = 0.5f;              // Distancia entre el �ltimo segmento de cuerpo y la cola
//    [Tooltip("Mantener para compatibilidad con otros scripts")]
//    public float separacionSegmentos = 0.5f;         // fallback para scripts externos
//    [Tooltip("Separaci�n individual por cada prefab de cuerpo")]
//    public float[] separacionesSegmentosCuerpo;        // Distancias espec�ficas entre cada segmento

//    public float velocidad = 5f;                       // Velocidad de movimiento de la cabeza
//    public float amplitudSerpenteo = 0.5f;           // Amplitud del movimiento serpenteante
//    public float frecuenciaSerpenteo = 2f;             // Frecuencia del movimiento serpenteante
//    public float ejeY = 1f;                            // Altura constante de la serpiente
//    public float umbralDetencion = 0.1f;               // Distancia m�nima al jugador para detenerse
//    public float velocidadRotacionCabeza = 10f;        // Velocidad de rotaci�n de la cabeza

//    [Header("Referencia al jugador")]
//    public Transform jugador;

//    // Lista interna de segmentos: cabeza, cuerpos y cola
//    private List<Transform> segmentos = new List<Transform>();
//    public List<Transform> Segmentos => segmentos;     // Expuesta para uso externo

//    // Historial de posiciones de la cabeza para desplazar el cuerpo
//    private List<Vector3> posicionesCabeza = new List<Vector3>();
//    private int maxRegistroPosiciones;

//    void Start()
//    {
//        Vector3 posInicial = new Vector3(transform.position.x, ejeY, transform.position.z);
//        // MODIFICACI�N CLAVE AQU�: Aumentamos el multiplicador para asegurar suficiente historial de posiciones.
//        maxRegistroPosiciones = Mathf.CeilToInt(
//            (distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola) * 5f // Multiplicador ajustado a 5f
//        );

//        // Instancia cabeza
//        Transform cabeza = Instantiate(prefabCabeza, posInicial, Quaternion.identity, transform).transform;
//        if (cabeza.GetComponent<SphereCollider>() == null)
//            cabeza.gameObject.AddComponent<SphereCollider>();
//        segmentos.Add(cabeza);

//        // Instancia segmentos de cuerpo usando array de prefabs
//        for (int i = 0; i < cantidadSegmentosCuerpo; i++)
//        {
//            GameObject prefab = (i < prefabsCuerpo.Length)
//                ? prefabsCuerpo[i]
//                : prefabsCuerpo[prefabsCuerpo.Length - 1];

//            Transform cuerpo = Instantiate(prefab, posInicial, Quaternion.identity, transform).transform;
//            if (cuerpo.GetComponent<Collider>() == null)
//                cuerpo.gameObject.AddComponent<BoxCollider>();
//            segmentos.Add(cuerpo);
//        }

//        // Instancia cola
//        Transform cola = Instantiate(prefabCola, posInicial, Quaternion.identity, transform).transform;
//        if (cola.GetComponent<BoxCollider>() == null)
//            cola.gameObject.AddComponent<BoxCollider>();
//        segmentos.Add(cola);

//        // Rellena historial de posiciones con la posici�n inicial
//        for (int i = 0; i < maxRegistroPosiciones; i++)
//            posicionesCabeza.Add(posInicial);
//    }

//    void Update()
//    {
//        Transform cabeza = segmentos[0];
//        Vector3 targetPos = new Vector3(jugador.position.x, ejeY, jugador.position.z);
//        float distancia = Vector3.Distance(cabeza.position, targetPos);

//        if (distancia > umbralDetencion)
//            MoverCabeza(targetPos);
//        else
//            DetenerCabeza(targetPos);

//        SeguirCuerpo();
//    }

//    void MoverCabeza(Vector3 objetivo)
//    {
//        Transform cabeza = segmentos[0];
//        Vector3 dir = (objetivo - cabeza.position).normalized;
//        dir.y = 0;

//        if (dir != Vector3.zero)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(dir);
//            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
//        }

//        Vector3 perp = Vector3.Cross(dir, Vector3.up).normalized;
//        float serp = Mathf.Sin(Time.time * frecuenciaSerpenteo) * amplitudSerpenteo;
//        Vector3 nuevaPos = cabeza.position + dir * velocidad * Time.deltaTime + perp * serp;
//        nuevaPos.y = ejeY;
//        cabeza.position = nuevaPos;

//        RegistrarPosicion(nuevaPos);
//    }

//    void DetenerCabeza(Vector3 posicion)
//    {
//        Transform cabeza = segmentos[0];
//        cabeza.position = posicion;

//        Vector3 dir = (jugador.position - cabeza.position).normalized;
//        dir.y = 0;
//        if (dir != Vector3.zero)
//        {
//            Quaternion targetRotation = Quaternion.LookRotation(dir);
//            cabeza.rotation = Quaternion.Slerp(cabeza.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
//        }

//        RegistrarPosicion(posicion);
//    }

//    void RegistrarPosicion(Vector3 pos)
//    {
//        posicionesCabeza.Insert(0, pos);
//        if (posicionesCabeza.Count > maxRegistroPosiciones)
//            posicionesCabeza.RemoveAt(posicionesCabeza.Count - 1);
//    }

//    void SeguirCuerpo()
//    {
//        for (int i = 1; i < segmentos.Count; i++)
//        {
//            float distanciaAtras;
//            bool esCola = (i == segmentos.Count - 1);
//            if (esCola)
//            {
//                distanciaAtras = distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola;
//            }
//            else
//            {
//                float suma = 0f;
//                int cuerpoIndex = i - 1;
//                for (int k = 0; k < cuerpoIndex; k++)
//                {
//                    if (k < separacionesSegmentosCuerpo.Length)
//                        suma += separacionesSegmentosCuerpo[k];
//                    else if (separacionesSegmentosCuerpo.Length > 0)
//                        suma += separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
//                    else
//                        suma += separacionSegmentos;
//                }
//                distanciaAtras = distanciaCabezaCuerpo + suma;
//            }

//            Vector3 posObjetivo = ObtenerPosicionEnTrayectoria(distanciaAtras);
//            Transform seg = segmentos[i];
//            seg.position = Vector3.Lerp(seg.position, posObjetivo, Time.deltaTime * velocidad);

//            // Rotaci�n de segmento
//            Vector3 lookDirection = (posObjetivo - seg.position).normalized;
//            lookDirection.y = 0;
//            if (lookDirection != Vector3.zero)
//            {
//                Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
//                seg.rotation = Quaternion.Slerp(seg.rotation, targetRotation, Time.deltaTime * velocidadRotacionCabeza);
//            }
//        }
//    }

//    Vector3 ObtenerPosicionEnTrayectoria(float distanciaAtras)
//    {
//        if (distanciaAtras <= 0f)
//            return posicionesCabeza[0];

//        float acumulado = 0f;
//        for (int i = 0; i < posicionesCabeza.Count - 1; i++)
//        {
//            float d = Vector3.Distance(posicionesCabeza[i], posicionesCabeza[i + 1]);
//            acumulado += d;
//            if (acumulado >= distanciaAtras)
//            {
//                float overShoot = acumulado - distanciaAtras;
//                float segmentLength = d;
//                float t = 1 - (overShoot / segmentLength);
//                return Vector3.Lerp(posicionesCabeza[i], posicionesCabeza[i + 1], t);
//            }
//        }
//        return posicionesCabeza[posicionesCabeza.Count - 1];
//    }

//    private float ObtenerDistanciaTotalCuerpo()
//    {
//        float suma = 0f;
//        int count = Mathf.Min(cantidadSegmentosCuerpo, separacionesSegmentosCuerpo.Length);
//        for (int i = 0; i < count; i++)
//            suma += separacionesSegmentosCuerpo[i];
//        if (cantidadSegmentosCuerpo > separacionesSegmentosCuerpo.Length && separacionesSegmentosCuerpo.Length > 0)
//            suma += (cantidadSegmentosCuerpo - separacionesSegmentosCuerpo.Length) * separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
//        if (separacionesSegmentosCuerpo.Length == 0)
//            suma = separacionSegmentos * cantidadSegmentosCuerpo;
//        return suma;
//    }

//    /// <summary>
//    /// Limpia el historial de posiciones de la cabeza y lo rellena con la posici�n actual
//    /// </summary>
//    public void ResetPositionHistory()
//    {
//        if (segmentos == null || segmentos.Count == 0)
//            return;

//        Vector3 currentHeadPos = segmentos[0].position;
//        posicionesCabeza.Clear();
//        for (int i = 0; i < maxRegistroPosiciones; i++)
//            posicionesCabeza.Add(currentHeadPos);
//    }
//}


























