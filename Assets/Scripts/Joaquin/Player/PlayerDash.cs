using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody), typeof(PlayerMovement))]
public class PlayerDash : MonoBehaviour
{
    [Header("Key input")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 15f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float minPostDashSpeed = 1f; // Velocidad mínima después del dash
    [SerializeField] private float dashCollisionCheckDistance = 2f;
    [SerializeField] private float bounceSpeed = 5f;
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private bool flattenDashDirection = true;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCam;
    [SerializeField] private ParticleSystem dashEffect;
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    [Header("CameraEffects")]
    [SerializeField] private float dashFov = 45f;
    [SerializeField] private float fovTransitionSpeed = 8f;

    private Rigidbody rb;
    private PlayerMovement playerMovement;

    private float defaultFov;
    private float targetFov;

    private bool isDashing = false;
    private bool canDash = true;
    private float dashTimer = 0f;

    private Vector3 currentDashDirection;
    private Vector3 dashStartPos;
    private Vector3 dashEndPos;

    private void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (virtualCam != null)
        {
            defaultFov = virtualCam.m_Lens.FieldOfView;
            targetFov = defaultFov;
        }

        rb.useGravity = true;
        isDashing = false;
        canDash = true;
    }

    private void Update()
    {
        if (virtualCam != null)
        {
            float currentFov = virtualCam.m_Lens.FieldOfView;
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * fovTransitionSpeed);
        }

        // Verifica que haya input vertical
        float vertical = Input.GetAxisRaw("Vertical");

        bool isMovingForward = vertical > 0.1f;

        if (Input.GetKeyDown(dashKey) && canDash && !isDashing && isMovingForward)
        {
            Vector3 dashDirection = GetDashDirection();
            if (IsPathClear(dashDirection, dashCollisionCheckDistance))
            {
                StartDashState(dashDirection);
                StartCoroutine(DashFovEffect());
            }
        }
    }

    private void FixedUpdate()
    {
        if (!isDashing) return;

        dashTimer += Time.fixedDeltaTime;
        float t = dashTimer / dashDuration;
        t = Mathf.Clamp01(t);

        Vector3 targetPos = Vector3.Lerp(dashStartPos, dashEndPos, t);
        Vector3 delta = targetPos - rb.position;
        float step = delta.magnitude;

        // 1) Chequeo adelantado:
        if (Physics.Raycast(rb.position, delta.normalized, out var hit, step))
        {
            // chocaste: ajusta targetPos al punto de impacto minus un pequeño margen
            targetPos = hit.point - delta.normalized * 0.1f;
            isDashing = false;
        }

        // 2) Mueve al targetPos realista:
        rb.MovePosition(targetPos);

        if (!isDashing || t >= 1f)
        {
            ResetDashState();
        }
    }

    private IEnumerator DashFovEffect()
    {
        targetFov = dashFov;

        yield return new WaitForSeconds(dashDuration);

        targetFov = defaultFov;
    }

    private void StartDashState(Vector3 dashDirection)
    {
        PlayDashEffect();

        isDashing = true;
        canDash = false;
        dashTimer = 0f;

        currentDashDirection = dashDirection;
        dashStartPos = transform.position;
        dashEndPos = dashStartPos + dashDirection * dashDistance;

        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        if (playerMovement.IsGrounded)
        {
            playerMovement.enabled = false;
        }
    }
    
    private void ResetDashState()
    {
        StopDashEffect();

        isDashing = false;
        rb.useGravity = true;

        AdjustPostDashVelocity();

        if (!playerMovement.enabled)
        {
            playerMovement.enabled = true;
        }

        StartCoroutine(ResetDashCooldown());
    }

    private IEnumerator ResetDashCooldown()
    {
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private Vector3 GetDashDirection()
    {
        Transform forwardSource = useCameraForward ? playerCam : orientation; // usar la cámara o la orientación del jugador

        if (forwardSource == null)
        {
            return transform.forward;
        }

        Vector3 direction = forwardSource.forward;

        if (flattenDashDirection)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
        }

        return direction.normalized;
    }

    private void AdjustPostDashVelocity()
    {
        Vector3 horizontalVel = new Vector3(currentDashDirection.x, 0f, currentDashDirection.z).normalized * minPostDashSpeed;

        if (playerMovement.IsGrounded)
        {
            // Mantener la velocidad horizontal y aplicar gravedad
            rb.velocity = horizontalVel;
        }
        else
        {
            // Aplicar inercia horizontal y mantener la velocidad vertical
            rb.velocity = new Vector3(horizontalVel.x, rb.velocity.y, horizontalVel.z);
        }
    }

    private void PlayDashEffect()
    {
        // Reproducir el efecto de partículas
        if (dashEffect != null) dashEffect?.Play();
    }

    private void StopDashEffect()
    {
        // Detener el efecto de partículas
        if (dashEffect != null) dashEffect?.Stop();
    }

    private bool IsPathClear(Vector3 direction, float distance)
    {
        RaycastHit hit;
        return !Physics.Raycast(transform.position, direction, out hit, distance);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isDashing)
        {
            CancelDash();
            ApplyBounce();
        }
    }

    private void CancelDash()
    {
        rb.useGravity = true;
        isDashing = false;

        if (virtualCam != null)
        {
            targetFov = defaultFov;
        }
        StopDashEffect();
    }

    private void ApplyBounce()
    {
        rb.velocity = -currentDashDirection * bounceSpeed;
    }
}