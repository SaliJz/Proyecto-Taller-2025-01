using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaAutomatica : MonoBehaviour
{
    public Transform puertaIzquierda;
    public Transform puertaDerecha;

    public Transform posicionAbiertaIzquierda;
    public Transform posicionAbiertaDerecha;
    public Transform posicionCerradaIzquierda;
    public Transform posicionCerradaDerecha;

    public float velocidad = 2f;

    private bool enZonaA = false;
    private bool puertaBloqueada = false;
    private bool abriendo = true;

    void Update()
    {
        if (puertaBloqueada) return;

        if (enZonaA)
        {
            if (abriendo)
            {
                puertaIzquierda.position = Vector3.MoveTowards(puertaIzquierda.position, posicionAbiertaIzquierda.position, velocidad * Time.deltaTime);
                puertaDerecha.position = Vector3.MoveTowards(puertaDerecha.position, posicionAbiertaDerecha.position, velocidad * Time.deltaTime);

                if (Vector3.Distance(puertaIzquierda.position, posicionAbiertaIzquierda.position) < 0.01f)
                {
                    abriendo = false;
                }
            }
            else
            {
                puertaIzquierda.position = Vector3.MoveTowards(puertaIzquierda.position, posicionCerradaIzquierda.position, velocidad * Time.deltaTime);
                puertaDerecha.position = Vector3.MoveTowards(puertaDerecha.position, posicionCerradaDerecha.position, velocidad * Time.deltaTime);

                if (Vector3.Distance(puertaIzquierda.position, posicionCerradaIzquierda.position) < 0.01f)
                {
                    abriendo = true;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (gameObject.name == "ZonaA")
            {
                enZonaA = true;
            }
            else if (gameObject.name == "ZonaB")
            {
                puertaBloqueada = true;
                enZonaA = false;

               
                puertaIzquierda.position = posicionCerradaIzquierda.position;
                puertaDerecha.position = posicionCerradaDerecha.position;
            }
        }
    }
}
