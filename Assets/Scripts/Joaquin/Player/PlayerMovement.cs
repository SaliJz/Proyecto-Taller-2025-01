using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform orientation;

    [Header("Key input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header ("Movement")]
    [SerializeField] private float speedWalk = 10f;
    [SerializeField] private float groundDrag = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float jumpCooldown = 0.25f;
    [SerializeField] private float jumpAirControl = 0.5f;
    private bool readyToJump = true;
    private float jumpBuffer = 0.15f;
    private float lastJumpPressedTime;

    /*
    [Header("GroundCheck")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckDistance = 0.5f;
    [SerializeField] private LayerMask groundMask;
    */

    // Ángulo máximo permitido desde la vertical para considerar el suelo
    [Header("Ground Normal Check")]
    [SerializeField] private float maxGroundAngle = 45f;
    private bool isGrounded = true;
    public bool IsGrounded => isGrounded;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    void Update()
    {
        if (Input.GetKeyDown(jumpKey))
        {
            lastJumpPressedTime = Time.time;
        }

        //CheckGround();
        HandleJump();

        // Aplicar fricción al suelo
        if (isGrounded)
        {
            rb.drag = groundDrag; // Fricción al suelo
        }
        else
        {
            rb.drag = 0; // Sin fricción en el aire
        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if (IsFacingSteepWall())
        {
            // Cancelar movimiento hacia adelante si está contra una pared empinada
            vertical = Mathf.Min(0f, vertical);
        }

        Vector3 moveDirection = (orientation.forward * vertical + orientation.right * horizontal).normalized;

        // Aplicar movimiento
        if (isGrounded)
        {
            Vector3 velocity = new Vector3(moveDirection.x * speedWalk, rb.velocity.y, moveDirection.z * speedWalk);
            rb.velocity = velocity;
        }
        // Aplicar control de aire
        else if (!isGrounded)
        {
            rb.AddForce(moveDirection * speedWalk * jumpAirControl, ForceMode.Acceleration); // Control de aire
        }

        // Aplicar rotación
        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection); // Rotación hacia la dirección de movimiento
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f); // Rotación suave
        }
    }

    private bool IsFacingSteepWall()
    {
        Debug.DrawRay(transform.position, orientation.forward * 0.6f, Color.red);

        if (Physics.Raycast(transform.position, orientation.forward, out RaycastHit hit, 0.6f))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            return angle > maxGroundAngle;
        }
        return false;
    }

    private void HandleJump()
    {
        // Si se presionó el salto recientemente y está en suelo y listo
        if (Time.time - lastJumpPressedTime < jumpBuffer && isGrounded && readyToJump)
        {
            readyToJump = false;
            lastJumpPressedTime = 0;

            rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        isGrounded = false;

        // Comprobar si el contacto está en el suelo
        foreach (ContactPoint contact in collision.contacts)
        {
            float angle = Vector3.Angle(contact.normal, Vector3.up);
            if (angle < maxGroundAngle)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        isGrounded = false;
    }

    /*
    private void CheckGround()
    {
        Ray ray = new Ray(groundCheckPoint.position, Vector3.down);
        isGrounded = Physics.Raycast(ray, groundCheckDistance, groundMask);

        Debug.DrawRay(ray.origin, ray.direction * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    private void OnDrawGizmos()
    {
        if (groundCheckPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(groundCheckPoint.position, groundCheckPoint.position + Vector3.down * groundCheckDistance);
    }
    */
}