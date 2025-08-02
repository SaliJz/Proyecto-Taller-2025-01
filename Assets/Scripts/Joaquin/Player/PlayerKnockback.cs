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
        rb = GetComponent<Rigidbody>();
        playerHealth = GetComponent<PlayerHealth>();
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

        float damageMultiplier = Mathf.Clamp01(damageAmount / 25f);

        Vector3 forceDirection = (damageDirection + Vector3.up * knockbackUpwardForce).normalized;

        rb.AddForce(forceDirection * knockbackForce * damageMultiplier, ForceMode.Impulse);
    }
}
