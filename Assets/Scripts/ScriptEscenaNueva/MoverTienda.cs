using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
public class MoverTienda : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public LayerMask groundLayer;
    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;

    private Rigidbody rb;
    private CapsuleCollider col;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        if (groundCheckPoint == null)
        {
            GameObject gcp = new GameObject("GroundCheckPoint");
            gcp.transform.parent = transform;
            gcp.transform.localPosition = new Vector3(0, -col.height / 2 + 0.05f, 0);
            groundCheckPoint = gcp.transform;
        }
    }

    
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDirection = new Vector3(h, 0f, v).normalized;
        Vector3 moveVelocity = moveDirection * moveSpeed;
        moveVelocity.y = rb.velocity.y;

        rb.velocity = moveVelocity;


        
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }


    }

    void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(groundCheckPoint.position, groundCheckRadius, groundLayer);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}
