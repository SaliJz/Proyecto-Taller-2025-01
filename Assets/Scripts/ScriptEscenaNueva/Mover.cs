using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Mover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float groundCheckDistance = 0.1f;

    [Header("Collision Settings")]
    public LayerMask groundLayer;
    public float skinWidth = 0.1f;

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
    }

    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");

        Vector3 moveVelocity = new Vector3(horizontalInput * moveSpeed, rb.velocity.y, verticalInput * moveSpeed);

        if (horizontalInput != 0 && CheckSideCollision(horizontalInput, true))
        {
            moveVelocity.x = 0;
        }

        if (verticalInput != 0 && CheckSideCollision(verticalInput, false))
        {
            moveVelocity.z = 0;
        }

        rb.velocity = moveVelocity;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics.Raycast(
            transform.position,
            Vector3.down,
            col.height / 2 + groundCheckDistance,
            groundLayer);
    }

    bool CheckSideCollision(float direction, bool isHorizontal)
    {
        Vector3 rayOrigin = transform.position;
        float rayDistance = col.radius + skinWidth;

        Vector3 rayDirection = isHorizontal ?
            Vector3.right * Mathf.Sign(direction) :
            Vector3.forward * Mathf.Sign(direction);


        RaycastHit hit;
        if (Physics.Raycast(
            rayOrigin,
            rayDirection,
            out hit,
            rayDistance,
            groundLayer))
        {
            return true; 
        }
        return false;
    }

    void OnDrawGizmos()
    {
        if (col != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, Vector3.down * (col.height / 2 + groundCheckDistance));

            
            Gizmos.DrawRay(transform.position, Vector3.right * (col.radius + skinWidth));
            Gizmos.DrawRay(transform.position, Vector3.left * (col.radius + skinWidth));
            Gizmos.DrawRay(transform.position, Vector3.forward * (col.radius + skinWidth));
            Gizmos.DrawRay(transform.position, Vector3.back * (col.radius + skinWidth));
        }
    }





}
