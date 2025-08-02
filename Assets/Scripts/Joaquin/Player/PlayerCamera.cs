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

    private void TriggerDamageEffect(Vector3 damageDirection, int damageAmount)
    {
        if (damageEffectCoroutine != null)
        {
            StopCoroutine(damageEffectCoroutine);
        }
        damageEffectCoroutine = StartCoroutine(DamageEffectCoroutine(damageDirection, damageAmount));
    }

    private IEnumerator DamageEffectCoroutine(Vector3 damageDirection, int damageAmount)
    {
        float timer = 0f;

        Vector3 localDirection = transform.InverseTransformDirection(damageDirection);
        Vector3 impactOffset = new Vector3(-localDirection.y, -localDirection.x, 0) * impactIntensity;

        float damageMultiplier = Mathf.Clamp01(damageAmount / 50f);

        while (timer < impactDuration)
        {
            float progress = 1 - (timer / impactDuration);
            Vector3 currentImpact = Vector3.Lerp(Vector3.zero, impactOffset, progress) * damageMultiplier;

            float currentShake = shakeIntensity * progress * damageMultiplier;
            Vector3 shakeOffset = new Vector3(
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake),
                Random.Range(-currentShake, currentShake)
            );

            currentDamageOffset = currentImpact + shakeOffset;

            timer += Time.deltaTime;
            yield return null;
        }

        currentDamageOffset = Vector3.zero;
    }
}
