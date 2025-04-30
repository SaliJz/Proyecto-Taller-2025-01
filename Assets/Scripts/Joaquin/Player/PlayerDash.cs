using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDash : MonoBehaviour
{
    [Header("Dash")]
    [SerializeField] private float dashForce = 20f;
    [SerializeField] private float dashTime = 0.2f;
    [SerializeField] private Transform cameraForwardSource;
    private Rigidbody rb;
    private bool isDashing = false;
    private bool canDash = true;
    private bool isGrounded;
    private bool hasAirDashed = false;

    [SerializeField] private float recoveryTime = 1;
    private bool isRecovering = false;

    [Header("Camera")]
    [SerializeField] private float dashFOVBoost = 15f;
    [SerializeField] private float fovLerpSpeed = 10f;
    private bool fovInitialized = false;

    [Header("Objects")]
    [SerializeField] private GameObject weaponHolderObject;
    [SerializeField] private GameObject abilityHolderObject;

    private float originalFOV;
    private Cinemachine.CinemachineVirtualCamera vcam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
    }

    private void Update()
    {
        if (!fovInitialized && vcam != null)
        {
            originalFOV = vcam.m_Lens.FieldOfView;
            fovInitialized = true;
        }

        // Verifica que haya input vertical
        float vertical = Input.GetAxisRaw("Vertical");

        bool isMovingForward = vertical > 0.1f;

        if (canDash && Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && isMovingForward)
        {
            if (!isGrounded && hasAirDashed) return;

            StartCoroutine(PerformDash());
            StartCoroutine(RestoreStateAfterDash());

            if (!isGrounded)
            {
                hasAirDashed = true;
            }
        }

        if (isRecovering)
        {
            // Lerp del FOV
            if (vcam != null)
            {
                vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, originalFOV, Time.deltaTime * fovLerpSpeed);
                isRecovering = vcam.m_Lens.FieldOfView != originalFOV; // Verifica si el FOV ha vuelto al original
            }
        }

    }

    private IEnumerator PerformDash()
    {
        // Desactivar holders durante dash
        if (weaponHolderObject != null) weaponHolderObject.SetActive(false);
        if (abilityHolderObject != null) abilityHolderObject.SetActive(false);

        // FOV de la cámara
        if (vcam != null)
        {
            vcam.m_Lens.FieldOfView = originalFOV + dashFOVBoost;
        }

        isDashing = true;

        // Dirección en base a cámara
        Vector3 dashDirection = cameraForwardSource.forward.normalized;

        // Limita el impulso vertical para que no "vuele"
        if (isGrounded)
        {
            dashDirection.y = 0f;
        }
        else
        {
            dashDirection.y = 0.05f; // Fuerza a que el dash no sume el salto
        }

        rb.velocity = Vector3.zero;
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        yield return new WaitForSeconds(dashTime);

        isDashing = false;
    }

    private IEnumerator RestoreStateAfterDash()
    {
        yield return new WaitForSeconds(recoveryTime); // Tiempo específico para restaurar el estado

        // Reactivar objetos
        if (weaponHolderObject != null) weaponHolderObject.SetActive(true);
        if (abilityHolderObject != null) abilityHolderObject.SetActive(true);

        isRecovering = true;
    }

    private void OnCollisionStay(Collision collision)
    {
        // Desactiva el uso del dash al colisionar con paredes
        if (collision.gameObject.CompareTag("Wall"))
        {
            canDash = false;
        }
        // Permite el uso del dash al colisionar con el suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
            hasAirDashed = false;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        // Reactiva el uso del dash al salir de la colisión con paredes
        if (collision.gameObject.CompareTag("Wall"))
        {
            canDash = true;
        }
        // Desactiva el dash al salir del suelo
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}