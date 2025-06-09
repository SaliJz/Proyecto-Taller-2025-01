using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlataformaCae : MonoBehaviour
{

    private Rigidbody rb;
    [SerializeField] private float delayAntesDeCaer = 0.5f;

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
                
                Debug.Log("¡Jugador encima! Plataforma caerá después de " + delayAntesDeCaer + " segundos.");
                StartCoroutine(CaerConRetardo());
            }
        }
    }

    private IEnumerator CaerConRetardo()
    {
        yield return new WaitForSeconds(delayAntesDeCaer);
        rb.isKinematic = false;
    }
}
