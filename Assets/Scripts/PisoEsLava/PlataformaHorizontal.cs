using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaHorizontal : MonoBehaviour
{
    public float distancia = 3f;
    public float velocidad = 2f;

    private Vector3 posicionInicial;
    private bool haciaDerecha = true;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float desplazamiento = Mathf.PingPong(Time.time * velocidad, distancia);
        transform.position = posicionInicial + Vector3.right * desplazamiento;
    }
}
