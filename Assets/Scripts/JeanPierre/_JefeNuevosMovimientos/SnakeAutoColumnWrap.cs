using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SnakeController))]
[RequireComponent(typeof(ColumnWrapOnInput))]
public class SnakeAutoColumnWrap : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Controlador de la serpiente")]
    public SnakeController snakeController;
    [Tooltip("Componente que envuelve y eleva la serpiente en la columna")]
    public ColumnWrapOnInput columnWrapper;

    [Header("Columna a envolver")]
    [Tooltip("Transform de la columna alrededor de la cual se enrollará la serpiente")]
    public Transform columna;

    [Header("Ajustes de espera")]
    [Tooltip("Tiempo máximo de espera para que aparezcan los segmentos (segundos)")]
    public float timeoutSeconds = 5f;
    [Tooltip("Intervalo entre comprobaciones de segmentos (segundos)")]
    public float checkInterval = 0.1f;

    private void Awake()
    {
        // Asignar componentes si no están arrastrados en el Inspector
        if (snakeController == null) snakeController = GetComponent<SnakeController>();
        if (columnWrapper == null) columnWrapper = GetComponent<ColumnWrapOnInput>();

        // Asignar la columna al ColumnWrapOnInput
        if (columna != null)
        {
            columnWrapper.columna = columna;
        }
        else
        {
            Debug.LogWarning("SnakeAutoColumnWrap: no se ha asignado ninguna columna en el Inspector.");
        }
    }

    private void Start()
    {
        StartCoroutine(WaitSegmentsThenWrap());
    }

    private IEnumerator WaitSegmentsThenWrap()
    {
        float elapsed = 0f;

        // 1) Esperar hasta que SnakeController haya instanciado sus segmentos
        while ((snakeController.Segmentos == null || snakeController.Segmentos.Count == 0)
               && elapsed < timeoutSeconds)
        {
            elapsed += checkInterval;
            yield return new WaitForSeconds(checkInterval);
        }

        if (snakeController.Segmentos == null || snakeController.Segmentos.Count == 0)
        {
            Debug.LogError($"SnakeAutoColumnWrap: No se detectaron segmentos tras esperar {timeoutSeconds}s.");
            yield break;
        }

        // 2) Detener movimiento libre de la serpiente
        snakeController.enabled = false;

        // 3) Disparar automáticamente el wrap y elevación
        columnWrapper.TriggerWrap();
    }
}
