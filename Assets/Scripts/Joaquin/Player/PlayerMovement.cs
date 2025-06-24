using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [Header("References")]
    private Rigidbody rb;
    [SerializeField] private Transform orientation;
    [SerializeField] private LayerMask groundLayer;

    [Header("Key input")]
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;

    [Header ("Movement")]
    [SerializeField] private float speedWalk = 10f;
    [SerializeField] private float groundDrag = 5f;
    [SerializeField] private float airControl = 0.2f;

    [Header("Jump")]
    [SerializeField] private float minJumpForce = 4f;
    [SerializeField] private float maxJumpForce = 8f;
    [SerializeField] private float maxJumpTime = 0.3f;
    [SerializeField] private float gravityFallMultiplier = 3.5f;
    private bool isJumping = false;
    private float jumpTimeCounter;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private float groundCheckDistance = 0.2f;
    private bool isGrounded;

    public bool IsGrounded => isGrounded;
    public float SpeedWalk => speedWalk;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    private void Update()
    {
        CheckGround();
        HandleJumpInput();
        HandleDrag();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleGravity();
    }

    private void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = (orientation.forward * vertical + orientation.right * horizontal).normalized;

        if (isGrounded)
        {
            rb.velocity = new Vector3(moveDirection.x * speedWalk, rb.velocity.y, moveDirection.z * speedWalk);
        }
        else
        {
            rb.AddForce(moveDirection * speedWalk * airControl, ForceMode.Acceleration);
        }

        if (moveDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

    private void HandleDrag()
    {
        rb.drag = isGrounded ? groundDrag : 0;
    }

    private void CheckGround()
    {
        Vector3 startPoint = transform.position + Vector3.up * groundCheckRadius;
        float castDistance = playerHeight / 2 - groundCheckRadius + groundCheckDistance;

        Debug.DrawRay(startPoint, Vector3.down * castDistance, Color.green);

        isGrounded = Physics.SphereCast(startPoint, groundCheckRadius, Vector3.down, out _, castDistance, groundLayer);
    }

    private void HandleJumpInput()
    {
        if (Input.GetKeyDown(jumpKey) && isGrounded)
        {
            isJumping = true;
            jumpTimeCounter = maxJumpTime;
            rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
            rb.AddForce(Vector3.up * minJumpForce, ForceMode.Impulse);
        }

        if (Input.GetKey(jumpKey) && isJumping)
        {
            if (jumpTimeCounter > 0)
            {
                float jumpForce = Mathf.Lerp(maxJumpForce, minJumpForce, 1 - (jumpTimeCounter / maxJumpTime));
                rb.AddForce(Vector3.up * jumpForce * Time.deltaTime, ForceMode.Impulse);
                jumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                isJumping = false;
            }
        }

        if (Input.GetKeyUp(jumpKey))
        {
            isJumping = false;
        }
    }

    private void HandleGravity()
    {
        if (!isGrounded && rb.velocity.y < 0)
        {
            rb.AddForce(Physics.gravity * (gravityFallMultiplier - 1), ForceMode.Acceleration);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position + Vector3.up * groundCheckRadius, groundCheckRadius);
        
        Gizmos.color = Color.green;
        Vector3 startPoint = transform.position + Vector3.up * groundCheckRadius;
        Gizmos.DrawLine(startPoint, startPoint + Vector3.down * (playerHeight / 2 - groundCheckRadius + groundCheckDistance));
    }
}