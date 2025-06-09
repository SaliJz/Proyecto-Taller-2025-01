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
    private Vector3 posFinalIzquierda;
    private Vector3 posFinalDerecha;
    private bool abrir = false;
    private bool cambiarEscena = false;

    void Start()
    {
        posInicialIzquierda = puertaIzquierda.localPosition;
        posInicialDerecha = puertaDerecha.localPosition;

     
        posFinalIzquierda = posInicialIzquierda + puertaIzquierda.right * -distanciaApertura;
        posFinalDerecha = posInicialDerecha + puertaDerecha.right * distanciaApertura;
    }

    
    void Update()
    {
        if (abrir && !cambiarEscena)
        {
            Debug.Log("Las puertas se están abriendo...");
            Debug.Log($"Posición izquierda actual: {puertaIzquierda.localPosition} / objetivo: {posFinalIzquierda}");
            Debug.Log($"Posición derecha actual: {puertaDerecha.localPosition} / objetivo: {posFinalDerecha}");

            puertaIzquierda.localPosition = Vector3.MoveTowards(
               puertaIzquierda.localPosition,
               posFinalIzquierda,
               velocidad * Time.deltaTime);

            puertaDerecha.localPosition = Vector3.MoveTowards(
                puertaDerecha.localPosition,
                posFinalDerecha,
                velocidad * Time.deltaTime);

            if (Vector3.Distance(puertaIzquierda.localPosition, posFinalIzquierda) < 0.01f &&
               Vector3.Distance(puertaDerecha.localPosition, posFinalDerecha) < 0.01f)
            {
                Debug.Log("Puertas completamente abiertas - Cambiando escena");
                cambiarEscena = true;
                Invoke("CambiarEscena", 1f);
            }

        }
    }

    public void ActivarPuerta()
    {
        if (!abrir)
        {
            Debug.Log("Activando apertura de puertas");
            abrir = true;
        }
    }

    private void CambiarEscena()
    {
        Debug.Log($"Cargando escena: {nombreEscena}");
        SceneManager.LoadScene(nombreEscena);
    }
}
