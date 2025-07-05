using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmsIdle : MonoBehaviour
{
    private Vector3 originalLocalPos;

    [Header("Par�metros del Movimiento")]
    [Tooltip("La altura m�xima que alcanzar� el objeto desde su posici�n inicial.")]
    public float amplitude = 0.025f;

    [Tooltip("La velocidad del movimiento. Valores m�s altos lo hacen m�s r�pido.")]
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