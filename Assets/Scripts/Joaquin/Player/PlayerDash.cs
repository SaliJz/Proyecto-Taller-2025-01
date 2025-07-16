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

    private void Start()
    {
        Initialize();
        isDashing = false;
        dashTimer = 0f;
    }

    private void Initialize()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();

        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();

        if (virtualCam != null)
        {
            defaultFov = virtualCam.m_Lens.FieldOfView;
            targetFov = defaultFov;
        }

        if (sfxSource == null)
        {
            sfxSource = GameObject.Find("SFXSource")?.GetComponent<AudioSource>();
            if (sfxSource == null) Debug.LogError("No se encontró el AudioSource para efectos de sonido.");
        }

        rb.useGravity = true;
        isDashing = false;
        canDash = true;
    }

    private void Update()
    {
        if (Time.timeScale == 0f) return; // Evita actualizaciones cuando el tiempo está pausado

        if (virtualCam != null)
        {
            float currentFov = virtualCam.m_Lens.FieldOfView;
            virtualCam.m_Lens.FieldOfView = Mathf.Lerp(currentFov, targetFov, Time.deltaTime * fovTransitionSpeed);
        }

        if (Input.GetKeyDown(dashKey) && canDash && !isDashing)
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

        if (DetectDashCollision())
        {
            StopDashAtCollision();
            return;
        }

        rb.velocity = currentDashDirection * (dashDistance / dashDuration);

        dashTimer += Time.fixedDeltaTime;
        if (dashTimer >= dashDuration)
        {
            ResetDashState();
        }
    }

    private bool DetectDashCollision()
    {
        if (!TryGetCapsuleCastPoints(out Vector3 p1, out Vector3 p2, out float radius)) return false;

        float castDistance = currentDashDirection.magnitude * Time.fixedDeltaTime;
        return Physics.CapsuleCast(p1, p2, radius, currentDashDirection, out RaycastHit hit, castDistance, ~0,
            QueryTriggerInteraction.Ignore) && IsBlockingTag(hit.collider.tag);
    }

    private void StopDashAtCollision()
    {
        rb.velocity = Vector3.zero;
        ResetDashState();
    }

    private bool TryGetCapsuleCastPoints(out Vector3 p1, out Vector3 p2, out float radius)
    {
        p1 = p2 = Vector3.zero;
        radius = 0f;

        CapsuleCollider capsule = GetComponent<CapsuleCollider>();
        if (capsule == null) return false;

        float height = Mathf.Max(capsule.height / 2f - capsule.radius, 0f);
        Vector3 center = transform.TransformPoint(capsule.center);

        Vector3 up = transform.up * height;
        p1 = center + up;
        p2 = center - up;
        radius = capsule.radius * Mathf.Max(transform.localScale.x, transform.localScale.z);
        return true;
    }

    private bool IsBlockingTag(string tag)
    {
        return tag == "Wall" || tag == "Ground" || tag == "Roof" || tag == "Columns" || tag == "Enemy";
    }

    private IEnumerator DashFovEffect()
    {
        targetFov = dashFov;
        yield return new WaitForSeconds(dashDuration);
        targetFov = defaultFov;
    }

    private void StartDashState(Vector3 dashDirection)
    {
        if (dashDirection == Vector3.zero) return;

        dashEffect?.Play();
        if (sfxSource != null && dashSound != null) sfxSource.PlayOneShot(dashSound);

        PlayerAnimatorController.Instance?.PlayDashAnim();

        isDashing = true;
        canDash = false;
        dashTimer = 0f;

        currentDashDirection = dashDirection;

        rb.velocity = currentDashDirection * (dashDistance / dashDuration);

        rb.velocity = Vector3.zero;
        rb.useGravity = false;

        if (playerMovement.IsGrounded) playerMovement.MovementEnabled = false;
    }

    private void ResetDashState()
    {
        dashEffect?.Stop();

        isDashing = false;
        rb.useGravity = true;

        ApplyPostDashImpulse();

        if (!playerMovement.MovementEnabled) playerMovement.MovementEnabled = true;

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

    private Vector3 GetDashDirection()
    {
        Transform forwardSource = useCameraForward ? playerCam : orientation;
        if (forwardSource == null) return transform.forward;

        Vector3 direction = forwardSource.forward;
        if (flattenDashDirection)
        {
            direction = Vector3.ProjectOnPlane(direction, Vector3.up).normalized;

            if (direction.sqrMagnitude < 0.01f)
            {
                return Vector3.ProjectOnPlane(orientation.forward, Vector3.up).normalized;
            }
        }

        return direction.normalized;
    }

    private bool IsPathClear(Vector3 direction, float distance)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, direction, out hit, distance))
        {
            if (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Ground") || hit.collider.CompareTag("Roof") || hit.collider.CompareTag("Columns")) return false;
        }
        return true;
    }
}