using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaMovil : MonoBehaviour
{
    public float altura = 3f;
    public float velocidad = 2f;

    private Vector3 posicionInicial;

    void Start()
    {
        posicionInicial = transform.position;
    }

    void Update()
    {
        float desplazamiento = Mathf.PingPong(Time.time * velocidad, altura);
        transform.position = posicionInicial + Vector3.up * desplazamiento;
    }
}
