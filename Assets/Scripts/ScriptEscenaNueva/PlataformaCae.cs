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
            
            rb.isKinematic = false;
            Debug.Log("Plataforma ahora caerá");
        }
    }
}
