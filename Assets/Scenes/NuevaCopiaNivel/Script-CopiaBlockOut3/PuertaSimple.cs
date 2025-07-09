using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaSimple : MonoBehaviour
{
    public Transform puertaIzquierda;
    public Transform puertaDerecha;

    public Vector3 posicionAbiertaIzquierda;
    public Vector3 posicionAbiertaDerecha;
    public Vector3 posicionCerradaIzquierda;
    public Vector3 posicionCerradaDerecha;

    public float velocidad = 2f;
    public bool estaEnZonaA = false;
    public bool puertaBloqueada = false;
    private bool abriendo = true;

    void Update()
    {
        if (puertaBloqueada) return;

        if (estaEnZonaA)
        {
            if (abriendo)
            {
                puertaIzquierda.position = Vector3.MoveTowards(puertaIzquierda.position, posicionAbiertaIzquierda, velocidad * Time.deltaTime);
                puertaDerecha.position = Vector3.MoveTowards(puertaDerecha.position, posicionAbiertaDerecha, velocidad * Time.deltaTime);

                if (Vector3.Distance(puertaIzquierda.position, posicionAbiertaIzquierda) < 0.01f)
                    abriendo = false;
            }
            else
            {
                puertaIzquierda.position = Vector3.MoveTowards(puertaIzquierda.position, posicionCerradaIzquierda, velocidad * Time.deltaTime);
                puertaDerecha.position = Vector3.MoveTowards(puertaDerecha.position, posicionCerradaDerecha, velocidad * Time.deltaTime);

                if (Vector3.Distance(puertaIzquierda.position, posicionCerradaIzquierda) < 0.01f)
                    abriendo = true;
            }
        }
    }

    public void ActivarZonaA() => estaEnZonaA = true;
    public void ActivarZonaB()

    {
        estaEnZonaA = false;
        puertaBloqueada = true;

        puertaIzquierda.position = posicionCerradaIzquierda;
        puertaDerecha.position = posicionCerradaDerecha;
    }

}
