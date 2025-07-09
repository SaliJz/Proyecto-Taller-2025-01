using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZonaBTrigger : MonoBehaviour
{
    public PuertaSimple puerta;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            puerta.ActivarZonaB();
        }
    }
}
