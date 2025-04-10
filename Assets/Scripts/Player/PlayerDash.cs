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

    [Header("VFX")]
    [SerializeField] private GameObject dashVFX;

    [Header("Camera")]
    [SerializeField] private Transform cameraHolder;
    [SerializeField] private float dashFOVBoost = 15f;
    [SerializeField] private float fovLerpSpeed = 10f;

    private float originalFOV;
    private Cinemachine.CinemachineVirtualCamera vcam;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        vcam = FindObjectOfType<Cinemachine.CinemachineVirtualCamera>();
        if (vcam != null)
        {
            originalFOV = vcam.m_Lens.FieldOfView;
        }
    }

    private void Update()
    {
        // Verifica que haya input (horizontal o vertical)
        bool isMoving = Input.GetAxisRaw("Horizontal") != 0 || Input.GetAxisRaw("Vertical") != 0;

        if (canDash)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !isDashing && isMoving)
            {
                // Si estamos en el aire y ya hicimos dash, no hacemos nada
                if (!isGrounded && hasAirDashed) return;

                StartCoroutine(PerformDash());

                if (!isGrounded)
                {
                    hasAirDashed = true;
                }
            }
        }

        // Suavizar FOV
        if (vcam != null && !isDashing)
        {
            vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, originalFOV, Time.deltaTime * fovLerpSpeed);
        }
    }

    private IEnumerator PerformDash()
    {
        isDashing = true;

        // Dirección en base a cámara
        Vector3 dashDirection = cameraForwardSource.forward.normalized;

        // Suavizar el componente vertical
        dashDirection = new Vector3(dashDirection.x, dashDirection.y * 0.1f, dashDirection.z);

        // Si está en el suelo, ignorar componente Y (dash solo horizontal)
        if (isGrounded)
        {
            dashDirection.y = 0f;
            dashDirection = dashDirection.normalized;
        }

        // Aplicar fuerza
        rb.velocity = Vector3.zero; // reset para no acumular velocidad anterior
        rb.AddForce(dashDirection * dashForce, ForceMode.Impulse);

        // Efecto visual
        if (dashVFX != null)
        {
            dashVFX.SetActive(true);
            ParticleSystem ps = dashVFX.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
        }

        // Efecto de cámara
        if (vcam != null)
        {
            vcam.m_Lens.FieldOfView = originalFOV + dashFOVBoost;
        }

        yield return new WaitForSeconds(dashTime);
        isDashing = false;
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