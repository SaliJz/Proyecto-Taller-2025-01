using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JugadorLava : MonoBehaviour
{
    [Header("Movimiento")]
    public float velocidad = 5f;
    public float fuerzaSalto = 7f;

    [Header("Salud")]
    public int saludMaxima = 100;
    private int saludActual;

    private Rigidbody rb;
    private bool enSuelo = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        saludActual = saludMaxima;
    }

    void Update()
    {
        float moverX = Input.GetAxis("Horizontal");
        float moverZ = Input.GetAxis("Vertical");

        Vector3 movimiento = new Vector3(moverX, 0f, moverZ) * velocidad;
        Vector3 nuevaVelocidad = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        rb.velocity = nuevaVelocidad;

        // Saltar
        if (Input.GetButtonDown("Jump") && enSuelo)
        {
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
            enSuelo = false;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            enSuelo = true;
        }
    }

    public void TakeDamage(int damage)
    {
        saludActual -= damage;
        Debug.Log("Vida actual: " + saludActual);

        if (saludActual <= 0)
        {
            Debug.Log("¡Jugador ha muerto!");
        }
    }

}
