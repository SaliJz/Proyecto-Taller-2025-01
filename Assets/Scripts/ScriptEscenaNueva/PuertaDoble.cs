using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PuertaDoble : MonoBehaviour
{
    public Transform puertaIzquierda;
    public Transform puertaDerecha;
    public float velocidad = 2f;
    public float distanciaApertura = 3f;
    public string nombreEscena;

    private Vector3 posInicialIzquierda;
    private Vector3 posInicialDerecha;
    private bool abrir = false;
    private bool cambiarEscena = false;

    void Start()
    {
        posInicialIzquierda = puertaIzquierda.position;
        posInicialDerecha = puertaDerecha.position;
    }

    
    void Update()
    {
        if (abrir)
        {
            Debug.Log("Las puertas se están abriendo...");
            Debug.Log("Posición izquierda: " + puertaIzquierda.position);
            Debug.Log("Posición derecha: " + puertaDerecha.position);

            puertaIzquierda.position = Vector3.MoveTowards(
               puertaIzquierda.position,
               posInicialIzquierda + Vector3.left * distanciaApertura,
               velocidad * Time.deltaTime);

            puertaDerecha.position = Vector3.MoveTowards(
                puertaDerecha.position,
                posInicialDerecha + Vector3.right * distanciaApertura,
                velocidad * Time.deltaTime);

            if (Vector3.Distance(puertaIzquierda.position, posInicialIzquierda + Vector3.left * distanciaApertura) < 0.01f &&
               Vector3.Distance(puertaDerecha.position, posInicialDerecha + Vector3.right * distanciaApertura) < 0.01f)
            {
                if (!cambiarEscena)
                {
                    cambiarEscena = true;
                    Invoke("CambiarEscena", 1f); 
                }
            }

        }
    }

    public void ActivarPuerta()
    {
        abrir = true;
    }

    private void CambiarEscena()
    {
        SceneManager.LoadScene(nombreEscena);
    }
}
