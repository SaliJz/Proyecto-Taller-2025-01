using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsIdle : MonoBehaviour
{
    private Vector3 originalLocalPos;

    [Header("Parámetros del Movimiento")]
    [Tooltip("La altura máxima que alcanzará el objeto desde su posición inicial.")]
    public float amplitude = 0.025f;

    [Tooltip("La velocidad del movimiento. Valores más altos lo hacen más rápido.")]
    public float frequency = 2.5f;

    void Start()
    {
        originalLocalPos = transform.localPosition;
    }

    void Update()
    {
        float desplazamientoY = Mathf.Sin(Time.time * frequency) * amplitude;
        transform.localPosition = originalLocalPos + new Vector3(0, desplazamientoY, 0);
    }
}