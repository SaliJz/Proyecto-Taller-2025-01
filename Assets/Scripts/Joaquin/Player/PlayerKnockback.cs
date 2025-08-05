using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Configuración de Empuje")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackUpwardForce = 2f; // Para levantar un poco al jugador

    private Rigidbody rb;
    private PlayerHealth playerHealth;

    private void Awake()
    {
        TryGetComponent(out rb);
        TryGetComponent(out playerHealth);
    }

    private void OnEnable()
    {
        playerHealth.OnPlayerDamaged += ApplyKnockback;
    }

    private void OnDisable()
    {
        playerHealth.OnPlayerDamaged -= ApplyKnockback;
    }

    private void ApplyKnockback(Vector3 damageDirection, int damageAmount)
    {
        if (rb == null) return;
        if (damageDirection == Vector3.zero) return;

        float damageMultiplier = Mathf.Clamp01(damageAmount / 25f);

        Vector3 horizontalDirection = -damageDirection.normalized;
        Vector3 force = horizontalDirection * knockbackForce * damageMultiplier;
        force += Vector3.up * knockbackUpwardForce;
        rb.AddForce(force, ForceMode.Impulse);
    }
}