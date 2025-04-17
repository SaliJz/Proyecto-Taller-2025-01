using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaVertical : MonoBehaviour
{
    public float velocidad = 2f;
    public float altura = 3f;

    private Vector3 posicionInicial;
    private bool subiendo = true;

    void Start()
    {
        posicionInicial = transform.position;
    }

    
    void Update()
    {
        if(subiendo) 
        {
            transform.position += Vector3.up * velocidad * Time.deltaTime;
            if(transform.position.y >= posicionInicial.y + altura)
            {
                subiendo = false;
            }
        }
        else
        {
            transform.position += Vector3.down * velocidad * Time.deltaTime;
            if(transform.position.y <= posicionInicial.y - altura)
            {
                subiendo = true;
            }
        }


    }
}
