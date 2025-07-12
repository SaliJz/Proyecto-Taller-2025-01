using Cinemachine;
using System.Collections;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Key input")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Settings")]
    [SerializeField] private float dashDistance = 15f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private float dashCollisionCheckDistance = 2f;
    [SerializeField] private float postDashImpulse = 2.5f;
    [SerializeField] private float bounceImpulse = 5f;
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

    [Header("Audio")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip dashSound;

    private Rigidbody rb;
    private PlayerMovement playerMovement;

    private float defaultFov;
    private float targetFov;

    private bool isDashing = false;
    public bool IsDashing => isDashing;
    private bool canDash = true;
    private float dashTimer = 0f;

    private Vector3 currentDashDirection;
    private Vector3 dashStartPos;
    private Vector3 dashEndPos;

    private void OnEnable()
    {
        Initialize();
        isDashing = false;
        dashTimer = 0f;
    }
    private void OnDisable()
    {
        isDashing = false;
        canDash = true;
        rb.useGravity = true;
        if (dashEffect != null) dashEffect.Stop();
        if (!playerMovement.MovementEnabled)
            playerMovement.MovementEnabled = true;
    }


    private void Initialize()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody>();

        if (playerMovement == null)
            playerMovement = GetComponent<PlayerMovement>();

        if (virtualCam != null)
        {
            defaultFov = virtualCam.m_Lens.FieldOfView;
            targetFov = defaultFov;
        }

        if (sfxSource == null)
        {
            sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();
            if (sfxSource == null)
                Debug.LogError("No se encontró el AudioSource para efectos de sonido.");
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

        if (Input.GetKeyDown(dashKey) && canDash && !isDashing)
        {
            Debug.Log("Intentando dashear");
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

        if (Physics.Raycast(rb.position, delta.normalized, out var hit, step))
        {
            if (!hit.collider.CompareTag("Fragment"))
            {
                targetPos = hit.point - delta.normalized * 0.1f;
                isDashing = false;
            }
        }

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
        dashEffect?.Play();
        if (sfxSource != null && dashSound != null)
            sfxSource.PlayOneShot(dashSound);

        PlayerAnimatorController.Instance?.PlayDashAnim();

        isDashing = true;
        canDash = false;
        dashTimer = 0f;

        currentDashDirection = dashDirection;

        dashStartPos = rb.position;
        dashEndPos = dashStartPos + dashDirection * dashDistance;

        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        if (playerMovement.IsGrounded)
            playerMovement.MovementEnabled = false;

        Debug.Log("DASH START POS: " + dashStartPos);
        Debug.Log("DASH END POS: " + dashEndPos);
    }

    private void ResetDashState()
    {
        dashEffect?.Stop();

        isDashing = false;
        rb.useGravity = true;

        ApplyPostDashImpulse();

        if (!playerMovement.MovementEnabled)
            playerMovement.MovementEnabled = true;

        Invoke(nameof(ResetDashCooldown), dashCooldown);
    }

    private void ResetDashCooldown()
    {
        canDash = true;
    }

    private void ApplyPostDashImpulse()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(currentDashDirection * postDashImpulse, ForceMode.Impulse);
    }

    private void ApplyBounce()
    {
        rb.velocity = Vector3.zero;
        rb.AddForce(-currentDashDirection * bounceImpulse, ForceMode.Impulse);
    }

    private Vector3 GetDashDirection()
    {
        Transform forwardSource = useCameraForward ? playerCam : orientation;
        if (forwardSource == null)
            return transform.forward;

        Vector3 direction = forwardSource.forward;
        if (flattenDashDirection)
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

        return direction.normalized;
    }

    private bool IsPathClear(Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
            return hit.collider.CompareTag("Fragment");

        return true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDashing)
        {
            ResetDashState();
            ApplyBounce();
        }
    }
}
