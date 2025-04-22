using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Mover : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float velocidad = 8f;
    [SerializeField] private float fuerzaSalto = 7f;
    [SerializeField] private float deteccionSueloDistancia = 1.2f;

    [Header("Vida")]
    public int vida = 100;

    private Rigidbody rb;
    private Vector3 direccionMovimiento;
    private bool estaEnSuelo;

    [Header("Material físico sin fricción")]
    [SerializeField] private PhysicMaterial sinFriccion;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;

        
        Collider col = GetComponent<Collider>();
        if (col != null && sinFriccion != null)
        {
            col.material = sinFriccion;
        }
    }

    private void Update()
    {
        float movX = Input.GetAxis("Horizontal");
        float movZ = Input.GetAxis("Vertical");

        direccionMovimiento = new Vector3(movX, 0f, movZ).normalized;

        estaEnSuelo = Physics.Raycast(transform.position, Vector3.down, deteccionSueloDistancia);

        if (Input.GetButtonDown("Jump") && estaEnSuelo)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(Vector3.up * fuerzaSalto, ForceMode.Impulse);
        }
    }

    private void FixedUpdate()
    {
        Vector3 movimiento = direccionMovimiento * velocidad;
        Vector3 nuevaVelocidad = new Vector3(movimiento.x, rb.velocity.y, movimiento.z);
        rb.velocity = nuevaVelocidad;
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            transform.parent = collision.transform;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Plataforma"))
        {
            transform.parent = null;
        }
    }


    public void TomarDaño(int daño)
    {
        vida -= daño;
        Debug.Log("Vida actual del jugador: " + vida);

        if (vida <= 0)
        {
            Debug.Log("¡El jugador ha muerto!");
            Destroy(gameObject);
        }
    }

}
