using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torreta: MonoBehaviour
{
    public Transform cabezaTorreta;
    public Transform puntoDisparo;
    public GameObject balaPrefab;
    public float rangoDeteccion = 15f;
    public float tiempoEntreDisparos = 0.5f;
    public int vida = 100;

    private float temporizadorDisparo = 0f;

    public int disparosAntesDeSobrecalentamiento = 5;
    public float tiempoEnfriamiento = 3f;
    private int contadorDisparos = 0;
    private bool sobrecalentado = false;
    private float temporizadorEnfriamiento = 0f;
    private Renderer rendererCabeza;

    private Transform jugador;

    void Start()
    {
        jugador = GameObject.FindGameObjectWithTag("PLAYER").transform;
        rendererCabeza = cabezaTorreta.GetComponent<Renderer>();
    }


    void Update()
    {
        if (jugador == null) return;

        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= rangoDeteccion)
        {
            // Apuntar al jugador
            Vector3 direccion = (jugador.position - cabezaTorreta.position).normalized;
            Quaternion rotacion = Quaternion.LookRotation(direccion);
            cabezaTorreta.rotation = Quaternion.Lerp(cabezaTorreta.rotation, rotacion, Time.deltaTime * 5f);

            // Disparar
            if (!sobrecalentado)
            {
                temporizadorDisparo += Time.deltaTime;
                if (temporizadorDisparo >= tiempoEntreDisparos)
                {
                    Disparar();
                    temporizadorDisparo = 0f;
                }
            }

            else
            {
                temporizadorEnfriamiento += Time.deltaTime;
                if (temporizadorEnfriamiento >= tiempoEnfriamiento)
                {
                    sobrecalentado = false;
                    contadorDisparos = 0;
                    rendererCabeza.material.color = Color.white;
                }
            }

        }
    }

    void Disparar()
    {
        Instantiate(balaPrefab, puntoDisparo.position, puntoDisparo.rotation);
        contadorDisparos++;
        if (contadorDisparos >= disparosAntesDeSobrecalentamiento)
        {
            sobrecalentado = true;
            temporizadorEnfriamiento = 0f;
            rendererCabeza.material.color = Color.red;
        }
    }

    public void TomarDaño(int cantidad)
    {
        vida -= cantidad;
        if (vida <= 0)
        {
            Destroy(gameObject);
        }
    }

}
