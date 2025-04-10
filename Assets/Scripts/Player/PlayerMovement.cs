using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header ("Movement")]
    
    private Rigidbody rb;
    private Vector3 velocity;

    [SerializeField] private float speedWalk = 10f;
    [SerializeField] private Transform orientation;

    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f; // Tiempo de espera entre saltos
    private bool readyToJump;

    [Header("GroundCheck")]
    private bool onGround;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        Movement();
        Jump();
    }

    private void Movement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        //Rota en sincronía con la cámara
        Vector3 direction = (orientation.forward * vertical + orientation.right * horizontal).normalized;
        velocity = direction * speedWalk;
        rb.velocity = new Vector3(velocity.x, rb.velocity.y, velocity.z);
    }

    private void Jump()
    {
        if (Input.GetButtonDown("Jump") && readyToJump && onGround)
        {
            readyToJump = false;

            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z); // Resetea la velocidad vertical
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown); // Espera un tiempo antes de permitir otro salto
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            readyToJump = true; // Permite saltar si está en el suelo
            onGround = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            onGround = false;
        }
    }
}
