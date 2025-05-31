// ColumnWrapOnInput.cs
// Controla la serpiente, viaja hacia la columna y la enrolla al presionar 'I'

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
public class ColumnWrapOnInput : MonoBehaviour
{
    [Header("Referencias")]
    public Transform columna;

    [Header("Parámetros de llegada")]
    public float umbralLlegada = 0.2f;

    [Header("Configuración de enrollado")]
    public int vueltasCompletas = 3;
    public float offsetRadio = 0.2f;
    public float velocidadEnrollado = 1f;
    public int segmentosCuello = 1;

    [Header("Altura de elevación")]
    public float alturaTotal; // Ahora pública, ajustable en Inspector

    private SnakeController snake;
    private Transform cabeza;
    private bool isWrapping = false;

    void Awake()
    {
        snake = GetComponent<SnakeController>();
        if (snake == null)
            Debug.LogError("SnakeController no encontrado.");

        // Desactiva el movimiento hasta que se presione 'I'
        snake.enabled = false;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I) && !isWrapping)
        {
            TriggerWrap();
        }
    }

    // Método público para invocar el wrap desde este o desde otro script
    public void TriggerWrap()
    {
        if (columna == null)
        {
            Debug.LogError("Columna no asignada.");
            return;
        }
        if (!isWrapping)
            StartCoroutine(InitAndWrap());
    }

    private IEnumerator InitAndWrap()
    {
        // Activa el control serpiente para dirigirse a la columna
        snake.enabled = true;
        snake.jugador = columna;
        yield return null;

        if (snake.Segmentos.Count == 0)
        {
            Debug.LogError("SnakeController sin segmentos.");
            snake.enabled = false;
            yield break;
        }
        cabeza = snake.Segmentos[0];

        // Espera hasta llegar al umbral
        while (!isWrapping)
        {
            float dist = Vector3.Distance(
                new Vector3(cabeza.position.x, 0, cabeza.position.z),
                new Vector3(columna.position.x, 0, columna.position.z)
            );

            if (dist <= umbralLlegada)
            {
                StartCoroutine(Enrollar());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator Enrollar()
    {
        isWrapping = true;
        snake.enabled = false;

        int segCount = snake.Segmentos.Count;

        // Calcula y asigna la alturaTotal antes de animar
        alturaTotal = snake.distanciaCabezaCuerpo
                    + snake.separacionSegmentos * (segCount - 2)
                    + snake.separacionCola;

        // Guarda posiciones iniciales
        List<Vector3> startPos = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            startPos.Add(snake.Segmentos[i].position);

        // Calcula posiciones objetivo usando la alturaTotal pública
        List<Vector3> targetPos = new List<Vector3>(segCount);
        float radio = GetColumnRadius();

        for (int i = 0; i < segCount; i++)
        {
            float t = 1f - ((float)i / (segCount - 1));
            float ang = t * vueltasCompletas * 2 * Mathf.PI;
            float alt = t * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 p = columna.position + dir * (radio + offsetRadio) + Vector3.up * alt;
            targetPos.Add(p);
        }

        // Animación de interpolación y orientación
        float elapsed = 0f;
        while (elapsed < velocidadEnrollado)
        {
            float frac = elapsed / velocidadEnrollado;
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = snake.Segmentos[i];
                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

                if (i <= segmentosCuello)
                    seg.LookAt(new Vector3(columna.position.x, seg.position.y, columna.position.z));
                else
                {
                    Vector3 lookT = (i < segCount - 1)
                        ? targetPos[i + 1]
                        : columna.position + Vector3.up * (targetPos[i].y + 0.1f);
                    seg.LookAt(lookT);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = snake.Segmentos[i];
            seg.position = targetPos[i];
            if (i <= segmentosCuello)
                seg.LookAt(new Vector3(columna.position.x, seg.position.y, columna.position.z));
            else
            {
                Vector3 lookT = (i < segCount - 1)
                    ? targetPos[i + 1]
                    : columna.position + Vector3.up * (targetPos[i].y + 0.1f);
                seg.LookAt(lookT);
            }
        }

        isWrapping = false;
    }

    // Cambiado a público para poder reutilizar desde otro script
    public float GetColumnRadius()
    {
        var col = columna.GetComponent<Collider>();
        if (col is CapsuleCollider cap)
            return cap.radius * Mathf.Max(columna.localScale.x, columna.localScale.z);
        if (col is SphereCollider sph)
            return sph.radius * Mathf.Max(columna.localScale.x, columna.localScale.z);
        return Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
    }
}










