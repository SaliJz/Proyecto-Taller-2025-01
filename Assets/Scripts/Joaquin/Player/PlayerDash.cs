using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class PlayerDash : MonoBehaviour
{
    [Header("Key input")]
    [SerializeField] private KeyCode dashKey = KeyCode.LeftShift;

    [Header("Dash Settings")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashUpwardForce = 2f;
    [SerializeField] private float dashDuration = 0.25f;
    [SerializeField] private float dashCooldown = 1f;
    [SerializeField] private bool useCameraForward = true;
    [SerializeField] private float dashCollisionCheckDistance = 2f;

    [Header("References")]
    [SerializeField] private Transform orientation;
    [SerializeField] private Transform playerCam;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private ParticleSystem dashEffectForward;
    [SerializeField] private CinemachineVirtualCamera virtualCam;

    [Header("CameraEffects")]
    [SerializeField] private float dashFov = 45f;
    [SerializeField] private float fovTransitionSpeed = 8f;

    private float defaultFov;
    private float targetFov;
    private bool isDashing = false;
    private bool canDash = true;
    private Coroutine dashCoroutine;
    private PlayerMovement playerMovement;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        playerMovement = GetComponent<PlayerMovement>();

        if (virtualCam != null)
        {
            defaultFov = virtualCam.m_Lens.FieldOfView;
            targetFov = defaultFov;
        }
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
            rb.drag = 0; // Desactiva la fricción al iniciar el dash
            Vector3 dashDirection = GetDashDirection();
            if (IsPathClear(dashDirection, dashCollisionCheckDistance))
            {
                StartDash(dashDirection);
            }
            else
            {
                Log("Dash bloqueado por obstáculo.");
            }
        }
    }

    private void StartDash(Vector3 dashDirection)
    {
        if (dashCoroutine != null) StopCoroutine(dashCoroutine);

        dashCoroutine = StartCoroutine(DashRoutine(dashDirection));
    }

    private IEnumerator DashRoutine(Vector3 dashDirection)
    {
        isDashing = true;
        canDash = false;

        PlayDashEffect();
        targetFov = dashFov;

        if (playerMovement != null && playerMovement.IsGrounded == true)
        {
            rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);
        }
        else
        {
            // Cambia a VelocityChange para un dash más suave en el aire
            rb.AddForce(dashDirection * (dashForce/2), ForceMode.VelocityChange);
            // Agrega fuerza hacia arriba solo si no estamos subiendo rápidamente ya
            if (rb.velocity.y < dashUpwardForce)
            {
                rb.AddForce(Vector3.up * dashUpwardForce, ForceMode.VelocityChange);
            }
        }

        float timer = 0f;
        while (timer < dashDuration && isDashing)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        targetFov = defaultFov;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlayDashEffect()
    {
        if (dashEffectForward != null)
        {
            // Reproducir el efecto de partículas
            dashEffectForward.Play();
        }
    }

    private Vector3 GetDashDirection()
    {
        Transform forwardSource = useCameraForward ? playerCam : orientation; // usar la cámara o la orientación del jugador
        Vector3 direction = forwardSource.forward;
        return direction.normalized;
    }

    private bool IsPathClear(Vector3 direction, float distance)
    {
        RaycastHit hit;
        return !Physics.Raycast(transform.position, direction, out hit, distance);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isDashing)
        {
            rb.velocity = Vector3.zero;
            isDashing = false; // cancela el dash al chocar
            targetFov = defaultFov;
        }
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}