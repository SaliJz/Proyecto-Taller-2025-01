using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorMovimiento : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float fuerzaSalto = 7f;
    private Rigidbody rb;
   

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Movimiento en horizontal y vertical 

        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(movX, 0f, movZ) * velocidad;
        Vector3 nuevaVelocidad = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        rb.velocity = nuevaVelocidad;

        //Saltar cuando esta en el suelo 

        if (Input.GetButtonDown("Jump") && EstaEnSuelo())
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private bool EstaEnSuelo()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f); // Puedes ajustar el 1.1f si salta en el aire
    }


    private void OnCollisionEnter(Collision collision)
    {
        // Si tocamos una plataforma, nos volvemos hijos de ella
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Cuando ya no estamos en la plataforma, dejamos de ser su hijo
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            transform.parent = null;
        }
    }
}
