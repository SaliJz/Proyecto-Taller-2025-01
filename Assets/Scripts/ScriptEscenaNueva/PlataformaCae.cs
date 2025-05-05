using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaCae : MonoBehaviour
{

    private Rigidbody rb;

    private void Start()
    {
        
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; 
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("PLAYER"))
        {

            if (collision.contacts[0].point.y > transform.position.y + 0.2f)
            {
                rb.isKinematic = false;
                Debug.Log("¡Jugador encima! Plataforma caerá.");
            }
        }
    }
}
