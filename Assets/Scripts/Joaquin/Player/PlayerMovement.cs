using System;
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
    [SerializeField] private float jumpForce = 8f;
    [SerializeField] private float gravityFallMultiplier = 3.5f;

    [Header("Ground Check")]
    [SerializeField] private float playerHeight = 2f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField] private float groundCheckDistance = 0.1f;
    private bool isGrounded;

    [Header("Debug Options")]
    [SerializeField] private bool showDetailsOptions = false;

    private Vector2 moveInput;
    private bool jumpInput;

    TutorialManager0 manager;
    private PlayerDash playerDash;

    public bool IsMoving => rb.velocity.magnitude > 0f;
    public bool IsGrounded => isGrounded;
    public bool MovementEnabled { get; set; } = true;

    void Awake()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    private void Start()
    {
        playerDash = GetComponent<PlayerDash>();
        manager = TutorialManager0.Instance;
    }

    private void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (Input.GetKeyDown(jumpKey))
        {
            jumpInput = true;
        }

        if (!MovementEnabled || (playerDash != null && playerDash.IsDashing)) return;

        CheckGround();
        HandleJumpInput();
    }

    private void FixedUpdate()
    {
        if (!MovementEnabled || (playerDash != null && playerDash.IsDashing)) return;

        HandleMovement();
        HandleDrag();
        HandleGravity();
    }

    private void HandleMovement()
    {
        Vector3 moveDirection = (orientation.forward * moveInput.y + orientation.right * moveInput.x).normalized;

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
        rb.drag = isGrounded ? groundDrag : 0f;
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
        if (jumpInput && isGrounded)
        {
            jumpInput = false;

            if (manager != null && manager.currentDialogueIndex < 2)
            {
                return;
            }
            else
            {
                rb.velocity = new Vector3(rb.velocity.x, 0, rb.velocity.z);
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else
        {
            jumpInput = false;
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

    private void OnGUI()
    {
        if (!showDetailsOptions) return;

        GUI.Label(new Rect(10, 10, 300, 20), "Vel: " + rb.velocity.ToString("F2"));
        GUI.Label(new Rect(10, 30, 300, 20), "Grounded: " + isGrounded);
        GUI.Label(new Rect(10, 50, 300, 20), "Moving: " + IsMoving);
        GUI.Label(new Rect(10, 70, 300, 20), "Movement Enabled: " + MovementEnabled);
        GUI.Label(new Rect(10, 90, 300, 20), "Gravity: " + Physics.gravity.ToString("F2"));
        GUI.Label(new Rect(10, 110, 300, 20), "Drag: " + rb.drag.ToString("F2"));
        GUI.Label(new Rect(10, 130, 300, 20), "Jump Force: " + jumpForce.ToString("F2"));
        GUI.Label(new Rect(10, 150, 300, 20), "Air Control: " + airControl.ToString("F2"));
        GUI.Label(new Rect(10, 170, 300, 20), "Ground Layer: " + groundLayer.value.ToString("F2"));
        GUI.Label(new Rect(10, 190, 300, 20), "Ground Check Radius: " + groundCheckRadius.ToString("F2"));
        GUI.Label(new Rect(10, 210, 300, 20), "Ground Check Distance: " + groundCheckDistance.ToString("F2"));
        GUI.Label(new Rect(10, 230, 300, 20), "Player Height: " + playerHeight.ToString("F2"));
        GUI.Label(new Rect(10, 250, 300, 20), "Orientation: " + orientation.name);
        GUI.Label(new Rect(10, 270, 300, 20), "Rigidbody: " + rb.name);
    }
}