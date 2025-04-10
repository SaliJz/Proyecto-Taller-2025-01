using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorMovimiento : MonoBehaviour
{
    [SerializeField] private float velocidad = 5f;
    [SerializeField] private float fuerzaSalto = 7f;
    private Rigidbody rb;
    private bool enSuelo;

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

        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    // Detectar contacto con el suelo
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            enSuelo = true;
        }
    }

    void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Suelo"))
        {
            enSuelo = false;
        }
    }

}
