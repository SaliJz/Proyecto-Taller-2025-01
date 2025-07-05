


// SnakeController.cs
using System.Collections.Generic;
using UnityEngine;

// Controla el comportamiento de una serpiente que sigue al jugador con movimiento serpenteante
public class SnakeController : MonoBehaviour
{
    [Header("Prefabs de la serpiente")]
    public GameObject prefabCabeza;
    public GameObject[] prefabsCuerpo;    // Array de prefabs de cuerpo
    public GameObject prefabCola;

    [Header("Configuración de la serpiente")]
    public int cantidadSegmentosCuerpo = 10;           // Número de segmentos de cuerpo
    public float distanciaCabezaCuerpo = 0.5f;       // Distancia entre cabeza y primer segmento
    public float separacionCola = 0.5f;              // Distancia entre el último segmento de cuerpo y la cola
    [Tooltip("Mantener para compatibilidad con otros scripts")]
    public float separacionSegmentos = 0.5f;         // fallback para scripts externos
    [Tooltip("Separación individual por cada prefab de cuerpo")]
    public float[] separacionesSegmentosCuerpo;        // Distancias específicas entre cada segmento

    public float velocidad = 5f;                       // Velocidad de movimiento de la cabeza
    public float amplitudSerpenteo = 0.5f;           // Amplitud del movimiento serpenteante
    public float frecuenciaSerpenteo = 2f;             // Frecuencia del movimiento serpenteante
    public float ejeY = 1f;                            // Altura constante de la serpiente
    public float umbralDetencion = 0.1f;               // Distancia mínima al jugador para detenerse
    public float velocidadRotacionCabeza = 10f;        // Velocidad de rotación de la cabeza

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
        // MODIFICACIÓN CLAVE AQUÍ: Aumentamos el multiplicador para asegurar suficiente historial de posiciones.
        maxRegistroPosiciones = Mathf.CeilToInt(
            (distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola) * 5f // Multiplicador ajustado a 5f
        );

        // Instancia cabeza
        Transform cabeza = Instantiate(prefabCabeza, posInicial, Quaternion.identity, transform).transform;
        if (cabeza.GetComponent<SphereCollider>() == null)
            cabeza.gameObject.AddComponent<SphereCollider>();
        segmentos.Add(cabeza);

        // Instancia segmentos de cuerpo usando array de prefabs
        for (int i = 0; i < cantidadSegmentosCuerpo; i++)
        {
            GameObject prefab = (i < prefabsCuerpo.Length)
                ? prefabsCuerpo[i]
                : prefabsCuerpo[prefabsCuerpo.Length - 1];

            Transform cuerpo = Instantiate(prefab, posInicial, Quaternion.identity, transform).transform;
            if (cuerpo.GetComponent<Collider>() == null)
                cuerpo.gameObject.AddComponent<BoxCollider>();
            segmentos.Add(cuerpo);
        }

        // Instancia cola
        Transform cola = Instantiate(prefabCola, posInicial, Quaternion.identity, transform).transform;
        if (cola.GetComponent<BoxCollider>() == null)
            cola.gameObject.AddComponent<BoxCollider>();
        segmentos.Add(cola);

        // Rellena historial de posiciones con la posición inicial
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

    void RegistrarPosicion(Vector3 pos)
    {
        posicionesCabeza.Insert(0, pos);
        if (posicionesCabeza.Count > maxRegistroPosiciones)
            posicionesCabeza.RemoveAt(posicionesCabeza.Count - 1);
    }

    void SeguirCuerpo()
    {
        for (int i = 1; i < segmentos.Count; i++)
        {
            float distanciaAtras;
            bool esCola = (i == segmentos.Count - 1);
            if (esCola)
            {
                distanciaAtras = distanciaCabezaCuerpo + ObtenerDistanciaTotalCuerpo() + separacionCola;
            }
            else
            {
                float suma = 0f;
                int cuerpoIndex = i - 1;
                for (int k = 0; k < cuerpoIndex; k++)
                {
                    if (k < separacionesSegmentosCuerpo.Length)
                        suma += separacionesSegmentosCuerpo[k];
                    else if (separacionesSegmentosCuerpo.Length > 0)
                        suma += separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
                    else
                        suma += separacionSegmentos;
                }
                distanciaAtras = distanciaCabezaCuerpo + suma;
            }

            Vector3 posObjetivo = ObtenerPosicionEnTrayectoria(distanciaAtras);
            Transform seg = segmentos[i];
            seg.position = Vector3.Lerp(seg.position, posObjetivo, Time.deltaTime * velocidad);

            // Rotación de segmento
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
                float segmentLength = d;
                float t = 1 - (overShoot / segmentLength);
                return Vector3.Lerp(posicionesCabeza[i], posicionesCabeza[i + 1], t);
            }
        }
        return posicionesCabeza[posicionesCabeza.Count - 1];
    }

    private float ObtenerDistanciaTotalCuerpo()
    {
        float suma = 0f;
        int count = Mathf.Min(cantidadSegmentosCuerpo, separacionesSegmentosCuerpo.Length);
        for (int i = 0; i < count; i++)
            suma += separacionesSegmentosCuerpo[i];
        if (cantidadSegmentosCuerpo > separacionesSegmentosCuerpo.Length && separacionesSegmentosCuerpo.Length > 0)
            suma += (cantidadSegmentosCuerpo - separacionesSegmentosCuerpo.Length) * separacionesSegmentosCuerpo[separacionesSegmentosCuerpo.Length - 1];
        if (separacionesSegmentosCuerpo.Length == 0)
            suma = separacionSegmentos * cantidadSegmentosCuerpo;
        return suma;
    }

    /// <summary>
    /// Limpia el historial de posiciones de la cabeza y lo rellena con la posición actual
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







































