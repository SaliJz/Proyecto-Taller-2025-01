using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorDisparo : MonoBehaviour
{
    public GameObject rayoPrefab;
    public Transform puntoDisparo; 
    public float tiempoEntreDisparos = 0.2f;
     


  

    private float temporizadorDisparo = 0f;

    void Update()
    {
        temporizadorDisparo += Time.deltaTime;

        if (Input.GetButton("Fire1") && temporizadorDisparo >= tiempoEntreDisparos) 
        {
            DispararRayo();
            temporizadorDisparo = 0f;
        }
    }

    void DispararRayo()
    {

        GameObject rayo = Instantiate(rayoPrefab, puntoDisparo.position, puntoDisparo.rotation);
        Debug.Log("¡Disparo generado!");

        Collider rayoCollider = rayo.GetComponent<Collider>();
        Collider jugadorCollider = GetComponent<Collider>();
        if (rayoCollider != null && jugadorCollider != null)
        {
            Physics.IgnoreCollision(rayoCollider, jugadorCollider);
        }

    }




}
