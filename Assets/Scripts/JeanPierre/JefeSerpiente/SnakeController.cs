using System.Collections.Generic;
using UnityEngine;

// Controla el comportamiento de una serpiente que sigue al jugador con movimiento serpenteante
public class SnakeController : MonoBehaviour
{
    [Header("Prefabs de la serpiente")]
    public GameObject prefabCabeza;
    public GameObject prefabCuerpo;
    public GameObject prefabCola;

    [Header("Configuración de la serpiente")]
    public int cantidadSegmentosCuerpo = 10;             // Número de segmentos de cuerpo
    public float distanciaCabezaCuerpo = 0.5f;           // Distancia entre cabeza y primer segmento
    public float separacionSegmentos = 0.5f;             // Distancia entre demás segmentos de cuerpo
    public float separacionCola = 0.5f;                  // Distancia entre el último segmento de cuerpo y la cola
    public float velocidad = 5f;                         // Velocidad de movimiento de la cabeza
    public float amplitudSerpenteo = 0.5f;               // Amplitud del movimiento serpenteante
    public float frecuenciaSerpenteo = 2f;               // Frecuencia del movimiento serpenteante
    public float ejeY = 1f;                              // Altura constante de la serpiente
    public float umbralDetencion = 0.1f;                 // Distancia mínima al jugador para detenerse

    [Header("Referencia al jugador")]
    public Transform jugador;

    // Lista interna de segmentos: cabeza, cuerpos y cola
    private List<Transform> segmentos = new List<Transform>();
    public List<Transform> Segmentos => segmentos;      // Expuesta para uso externo

    // Historial de posiciones de la cabeza para desplazar el cuerpo
    private List<Vector3> posicionesCabeza = new List<Vector3>();
    private int maxRegistroPosiciones;

    void Start()
    {
        Vector3 posInicial = new Vector3(transform.position.x, ejeY, transform.position.z);
        maxRegistroPosiciones = Mathf.CeilToInt(
            (distanciaCabezaCuerpo + separacionSegmentos * (cantidadSegmentosCuerpo - 1) + separacionCola) * 2
        );

        // Instancia cabeza
        Transform cabeza = Instantiate(prefabCabeza, posInicial, Quaternion.identity, transform).transform;
        // Añadir SphereCollider a la cabeza
        if (cabeza.GetComponent<SphereCollider>() == null)
        {
            cabeza.gameObject.AddComponent<SphereCollider>();
        }
        segmentos.Add(cabeza);

        // Instancia segmentos de cuerpo
        for (int i = 0; i < cantidadSegmentosCuerpo; i++)
        {
            Transform cuerpo = Instantiate(prefabCuerpo, posInicial, Quaternion.identity, transform).transform;
            // Añadir BoxCollider a cada segmento de cuerpo
            if (cuerpo.GetComponent<BoxCollider>() == null)
            {
                cuerpo.gameObject.AddComponent<BoxCollider>();
            }
            segmentos.Add(cuerpo);
        }

        // Instancia cola
        Transform cola = Instantiate(prefabCola, posInicial, Quaternion.identity, transform).transform;
        // Añadir BoxCollider a la cola
        if (cola.GetComponent<BoxCollider>() == null)
        {
            cola.gameObject.AddComponent<BoxCollider>();
        }
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
            float distanciaAtras = (i == segmentos.Count - 1)
                ? distanciaCabezaCuerpo + separacionSegmentos * (cantidadSegmentosCuerpo - 1) + separacionCola
                : distanciaCabezaCuerpo + separacionSegmentos * (i - 1);

            Vector3 posObjetivo = ObtenerPosicionEnTrayectoria(distanciaAtras);
            Transform seg = segmentos[i];
            seg.position = Vector3.Lerp(seg.position, posObjetivo, Time.deltaTime * velocidad);
            seg.LookAt(new Vector3(posObjetivo.x, ejeY, posObjetivo.z));
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
                return posicionesCabeza[i + 1];
        }
        return posicionesCabeza[posicionesCabeza.Count - 1];
    }
}
































