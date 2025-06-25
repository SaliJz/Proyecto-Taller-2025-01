// ColumnWrapOnInput.cs
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
    [Tooltip("Distancia extra que sobresale la cabeza de la columna")]
    public float headOffset = 0.5f;
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
            TriggerWrap();
    }

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
        isWrapping = true;
        snake.enabled = true;
        snake.jugador = columna;
        yield return null;

        if (snake.Segmentos.Count == 0)
        {
            Debug.LogError("SnakeController sin segmentos.");
            snake.enabled = false;
            isWrapping = false;
            yield break;
        }
        cabeza = snake.Segmentos[0];

        // Mover hasta la columna
        Transform player = GameObject.FindWithTag("Player")?.transform;
        while (true)
        {
            // Orientar cabeza hacia el jugador mientras avanza
            if (player != null)
            {
                Vector3 lookPos = new Vector3(player.position.x, cabeza.position.y, player.position.z);
                cabeza.LookAt(lookPos);
            }

            float dist = Vector3.Distance(
                new Vector3(cabeza.position.x, 0, cabeza.position.z),
                new Vector3(columna.position.x, 0, columna.position.z)
            );
            if (dist <= umbralLlegada)
                break;
            yield return null;
        }

        // Enrollar
        yield return StartCoroutine(Enrollar());
        isWrapping = false;
    }

    private IEnumerator Enrollar()
    {
        snake.enabled = false;

        Transform player = GameObject.FindWithTag("Player")?.transform;
        if (player == null)
            Debug.LogError("Player no encontrado para orientar la cabeza.");

        int segCount = snake.Segmentos.Count;
        alturaTotal = snake.distanciaCabezaCuerpo
                    + snake.separacionSegmentos * (segCount - 2)
                    + snake.separacionCola;

        // Guardar posiciones iniciales
        List<Vector3> startPos = new List<Vector3>(segCount);
        for (int i = 0; i < segCount; i++)
            startPos.Add(snake.Segmentos[i].position);

        // Calcular posiciones en espiral alrededor de la columna
        List<Vector3> targetPos = new List<Vector3>(segCount);
        float radio = GetColumnRadius();
        for (int i = 0; i < segCount; i++)
        {
            float t = 1f - ((float)i / (segCount - 1));
            float ang = t * vueltasCompletas * 2 * Mathf.PI;
            float alt = t * alturaTotal;
            Vector3 dir = new Vector3(Mathf.Cos(ang), 0, Mathf.Sin(ang));
            float extra = (i == 0) ? headOffset : 0f;
            Vector3 p = columna.position + dir * (radio + offsetRadio + extra) + Vector3.up * alt;
            targetPos.Add(p);
        }

        // Animar enrollado
        float elapsed = 0f;
        while (elapsed < velocidadEnrollado)
        {
            float frac = elapsed / velocidadEnrollado;
            for (int i = 0; i < segCount; i++)
            {
                Transform seg = snake.Segmentos[i];
                seg.position = Vector3.Lerp(startPos[i], targetPos[i], frac);

                // Orientación de la cabeza al jugador durante el enrollado
                if (i == 0 && player != null)
                {
                    Vector3 lookPos = new Vector3(player.position.x, seg.position.y, player.position.z);
                    seg.LookAt(lookPos);
                }
                else if (i <= segmentosCuello)
                {
                    seg.LookAt(new Vector3(columna.position.x, seg.position.y, columna.position.z));
                }
                else
                {
                    Vector3 next = (i < segCount - 1)
                        ? targetPos[i + 1]
                        : columna.position + Vector3.up * (targetPos[i].y + 0.1f);
                    seg.LookAt(next);
                }
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Ajuste final de posiciones y rotaciones
        for (int i = 0; i < segCount; i++)
        {
            Transform seg = snake.Segmentos[i];
            seg.position = targetPos[i];
            if (i == 0 && player != null)
            {
                Vector3 lookPos = new Vector3(player.position.x, seg.position.y, player.position.z);
                seg.LookAt(lookPos);
            }
            else if (i <= segmentosCuello)
            {
                seg.LookAt(new Vector3(columna.position.x, seg.position.y, columna.position.z));
            }
            else
            {
                Vector3 next = (i < segCount - 1)
                    ? targetPos[i + 1]
                    : columna.position + Vector3.up * (targetPos[i].y + 0.1f);
                seg.LookAt(next);
            }
        }

        yield break;
    }

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






