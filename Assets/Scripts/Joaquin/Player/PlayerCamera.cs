using UnityEngine;
using System.Collections;

public class PlayerCamera : MonoBehaviour
{
    [Header("Configuración General")]
    [SerializeField] private float baseSensitivity = 2f;
    [SerializeField] private float verticalClamp = 90f;
    [SerializeField] private Transform orientation;
    [SerializeField] private PlayerHealth playerHealth;

    [Header("Efectos de Daño")]
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private float tiltIntensity = 10f;
    [SerializeField] private float shakeIntensity = 1.5f;
    [SerializeField] private AnimationCurve recoveryCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

    private float rotationX;
    private float rotationY;

    private Quaternion damageRotationOffset = Quaternion.identity;
    private Coroutine damageEffectCoroutine;
    private float currentSensitivity;

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

        currentSensitivity = baseSensitivity;
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

        float newSens = SettingsService.Sensitivity * 100;
        if (!Mathf.Approximately(newSens, currentSensitivity)) currentSensitivity = newSens;

        float mouseX = Input.GetAxis("Mouse X") * Time.deltaTime * currentSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * Time.deltaTime * currentSensitivity;

        rotationX -= mouseY;
        rotationY += mouseX;
        rotationX = Mathf.Clamp(rotationX, -verticalClamp, verticalClamp);

        Quaternion targetRotation = Quaternion.Euler(rotationX, rotationY, 0f);
        transform.rotation = targetRotation * damageRotationOffset;
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
        float elapsed = 0f;

        Vector3 localDirection = orientation.InverseTransformDirection(damageDirection);

        float damageMultiplier = Mathf.Clamp01(damageAmount / 50f) + 0.5f;

        while (elapsed < effectDuration)
        {
            float t = elapsed / effectDuration;
            float intensity = recoveryCurve.Evaluate(t / effectDuration) * damageMultiplier;

            float pitch = -localDirection.z * tiltIntensity * intensity;
            float roll = localDirection.x * tiltIntensity * intensity;

            Quaternion tiltRotation = Quaternion.Euler(pitch, 0, roll);

            float currentShake = shakeIntensity * intensity;
            float shakeX = Random.Range(-currentShake, currentShake);
            float shakeY = Random.Range(-currentShake, currentShake);
            float shakeZ = Random.Range(-currentShake, currentShake);

            Quaternion shakeRotation = Quaternion.Euler(shakeX, shakeY, shakeZ);

            damageRotationOffset = tiltRotation * shakeRotation;

            elapsed += Time.deltaTime;
            yield return null;
        }

        damageRotationOffset = Quaternion.identity;
    }
}
