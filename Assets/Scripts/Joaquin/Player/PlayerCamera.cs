using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private float sensitivity = 2f;
    [SerializeField] private float verticalClamp = 90f;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Efectos de Daño")]
    [SerializeField] private float impactDuration = 0.25f;
    [SerializeField] private float impactIntensity = 5f;
    [SerializeField] private float shakeIntensity = 0.5f;

    private float rotationX;
    private float rotationY;

    private Vector3 currentDamageOffset;
    private Coroutine damageEffectCoroutine;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        playerHealth= FindAnyObjectByType<PlayerHealth>();
    }

    private void Start()
    {
        // Captura rotación inicial desde la escena
        Vector3 eulerAngles = transform.rotation.eulerAngles;
        rotationX = eulerAngles.x;
        rotationY = eulerAngles.y;
    }

    private void OnEnable()
    {
        if (playerHealth != null) playerHealth.OnPlayerDamaged += TriggerDamageEffect;
    }

    private void OnDisable()
    {
        if (playerHealth != null) playerHealth.OnPlayerDamaged -= TriggerDamageEffect;
    }

    private void LateUpdate()
    {
        if (Time.timeScale == 0f) return; // Evita actualizaciones cuando el tiempo está pausado

        float currentSensitivity = SettingsService.Sensitivity * 100f;
        sensitivity = currentSensitivity;

        float mouseX = Input.GetAxis("Mouse X") * Time.fixedDeltaTime * sensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.fixedDeltaTime * sensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;
        rotationX = Mathf.Clamp(rotationX, -verticalClamp, verticalClamp);

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        transform.rotation = targetRotation * Quaternion.Euler(currentDamageOffset);
        orientation.rotation = Quaternion.Euler(0f, rotationY, 0f);
    }

    /// <summary>
    /// Activa la corutina que maneja los efectos de cámara al recibir daño.
    /// </summary>
    private void TriggerDamageEffect(Vector3 damageDirection, int damageAmount)
    {
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine(damageDirection, damageAmount));
    }

    /// <summary>
    /// Corutina que genera el tambaleo y el empuje visual de la cámara.
    /// </summary>
    private IEnumerator DamageEffectCoroutine(Vector3 damageDirection, int damageAmount)
    {
        float timer = 0f;

        // 1. Calcular el empuje direccional inicial
        // Convierte la dirección del daño a un espacio local relativo a la cámara
        Vector3 localDirection = transform.InverseTransformDirection(damageDirection);
        // El empuje visual será hacia arriba y en la dirección opuesta al golpe
        Vector3 impactOffset = new Vector3(-localDirection.y, -localDirection.x, 0) * impactIntensity;

        // Escala el efecto basado en el daño (opcional pero recomendado)
        float damageMultiplier = Mathf.Clamp01(damageAmount / 50f); // Se normaliza (ej: 50 de daño = efecto máximo)

        while (timer < impactDuration)
        {
            // Interpola el empuje de vuelta a cero
            float progress = 1 - (timer / impactDuration); // De 1 a 0
            Vector3 currentImpact = Vector3.Lerp(Vector3.zero, impactOffset, progress) * damageMultiplier;

            // 2. Calcular el tambaleo (shake) aleatorio
            float currentShake = shakeIntensity * progress * damageMultiplier;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake)
            );

            // 3. Combinar ambos efectos
            currentDamageOffset = currentImpact + shakeOffset;

            timer += Time.deltaTime;
            yield return null;
        }

        currentDamageOffset = Vector3.zero; // Asegura que la cámara vuelva a la normalidad
    }
}
