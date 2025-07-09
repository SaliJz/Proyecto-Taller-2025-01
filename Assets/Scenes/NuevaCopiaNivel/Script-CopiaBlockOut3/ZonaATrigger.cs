using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonaATrigger : MonoBehaviour
{
    public PuertaSimple puerta;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puerta.ActivarZonaA();
        }
    }
}
