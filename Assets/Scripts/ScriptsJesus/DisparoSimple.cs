using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisparoSimple : MonoBehaviour
{
    public GameObject balaPrefab;         
    public float velocidadBala = 20f;
    public float tiempoEntreDisparos = 0.2f;

    private float temporizador;
    private bool estaSaltando;

    void Update()
    {
        temporizador += Time.deltaTime;
        estaSaltando = Mathf.Abs(GetComponent<Rigidbody>().velocity.y) > 0.1f;

        if (Input.GetButton("Fire1") && temporizador >= tiempoEntreDisparos)
        {
            Disparar();
            temporizador = 0f;
        }
    }

    void Disparar()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        Vector3 direccionDisparo = transform.forward;

        if (Physics.Raycast(ray, out hit, 100f))
        {
            
            direccionDisparo = (hit.point - transform.position).normalized;
        }

        else
        {
           
            direccionDisparo = (ray.direction).normalized;
        }

        Vector3 posicionDisparo = transform.position + transform.forward * 1f;

        // Instancia la bala
        GameObject bala = Instantiate(balaPrefab, posicionDisparo, Quaternion.LookRotation(direccionDisparo));
        bala.tag = "Laser";


        Rigidbody rb = bala.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = direccionDisparo * velocidadBala;  
        }

        Debug.Log("¡Disparo realizado!");
    }



}
