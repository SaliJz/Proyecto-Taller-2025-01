// ColumnWrapController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
[DefaultExecutionOrder(100)]
public class ColumnWrapController : MonoBehaviour
{
    [Header("Referencias")]
    public Transform columna;                    // Transform de la columna

    private SnakeController snake;               // Referencia al controlador de la serpiente
    private Transform cabeza;                    // La cabeza de la serpiente
    private bool isWrapping = false;             // Si está en proceso de enrollado

    [Header("Parámetros de llegada")]
    public float umbralLlegada = 0.2f;           // Distancia para iniciar enrollado

    [Header("Configuración de enrollado")]
    public int vueltasCompletas = 3;             // Número total de vueltas
    public float offsetRadio = 0.2f;             // Espacio extra respecto al radio de la columna
    public float velocidadEnrollado = 1f;        // Duración del enrollado en segundos
    public int segmentosCuello = 1;              // Cantidad de segmentos que miran al jugador

    private void OnEnable()
    {
        // Cada vez que habilites este componente, empieza todo de nuevo
        StartCoroutine(InitAndWrapRoutine());
    }

    private IEnumerator InitAndWrapRoutine()
    {
        // Espera un frame para que SnakeController inicialice segmentos
        yield return null;

        // (Re)inicializa referencias
        snake = GetComponent<SnakeController>();
        if (snake == null || snake.Segmentos.Count == 0)
        {
            Debug.LogError("[ColumnWrap] SnakeController no inicializado o sin segmentos.");
            yield break;
        }
        cabeza = snake.Segmentos[0];

        if (columna == null)
        {
            Debug.LogError("[ColumnWrap] Columna no asignada.");
            yield break;
        }

        // Activa el control de movimiento hacia la columna
        snake.enabled = true;
        snake.jugador = columna;

        // Espera a llegar al umbral
        while (!isWrapping)
        {
            float dist = Vector3.Distance(
                new Vector3(cabeza.position.x, 0, cabeza.position.z),
                new Vector3(columna.position.x, 0, columna.position.z)
            );
            if (dist <= umbralLlegada)
            {
                // Empieza a enrollar
                StartCoroutine(EnrollarAlrededor());
                yield break;
            }
            yield return null;
        }
    }

    private IEnumerator EnrollarAlrededor()
    {
        isWrapping = true;

        // Desactiva el control manual mientras enrolla
        snake.enabled = false;

        int segCount = snake.Segmentos.Count;
        List<Vector3> startPos = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            startPos.Add(snake.Segmentos[i].position);

        // Calcula las posiciones objetivo
        List<Vector3> targetPos = new List<Vector3>(segCount);
        float radio = GetColumnRadius();
        float alturaTotal = snake.distanciaCabezaCuerpo
                          + snake.separacionSegmentos * (segCount - 2)
                          + snake.separacionCola;

        for (int i = 0; i < segCount; i++)
        {
            float t = 1f - ((float)i / (segCount - 1));
            float ang = t * vueltasCompletas * 2 * Mathf.PI;
            float alt = t * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            Vector3 p = columna.position + dir * (radio + offsetRadio) + Vector3.up * alt;
            targetPos.Add(p);
        }

        // Animación de lerp
        float elapsed = 0f;
        while (elapsed < velocidadEnrollado)
        {
            float frac = elapsed / velocidadEnrollado;
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = snake.Segmentos[i];
                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

                // Ajusta la orientación
                if (i <= segmentosCuello)
                    seg.LookAt(new Vector3(snake.jugador.position.x, seg.position.y, snake.jugador.position.z));
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

        // Posición y orientación finales
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = snake.Segmentos[i];
            seg.position = targetPos[i];
            if (i <= segmentosCuello)
                seg.LookAt(new Vector3(snake.jugador.position.x, seg.position.y, snake.jugador.position.z));
            else
            {
                Vector3 lookT = (i < segCount - 1)
                    ? targetPos[i + 1]
                    : columna.position + Vector3.up * (targetPos[i].y + 0.1f);
                seg.LookAt(lookT);
            }
        }

        // Finalizado: desactiva para permitir futuras reactivaciones
        isWrapping = false;
        this.enabled = false;
    }

    private float GetColumnRadius()
    {
        var col = columna.GetComponent<Collider>();
        if (col is CapsuleCollider cap)
            return cap.radius * Mathf.Max(columna.localScale.x, columna.localScale.z);
        if (col is SphereCollider sph)
            return sph.radius * Mathf.Max(columna.localScale.x, columna.localScale.z);
        return Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
    }
}






