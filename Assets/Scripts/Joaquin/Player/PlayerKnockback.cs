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

    /// <summary>
    /// Aplica una fuerza de empuje al Rigidbody del jugador.
    /// </summary>
    private void ApplyKnockback(Vector3 damageDirection, int damageAmount)
    {
        if (rb == null) return;

        // Escala la fuerza basada en el daño (opcional)
        float damageMultiplier = Mathf.Clamp01(damageAmount / 25f); // 25 de daño = fuerza base

        // La fuerza es en la misma dirección del daño, empujando al jugador
        Vector3 forceDirection = (damageDirection + Vector3.up * knockbackUpwardForce).normalized;

        // Aplicamos la fuerza. Usamos Impulse para una explosión instantánea.
        rb.AddForce(forceDirection * knockbackForce * damageMultiplier, ForceMode.Impulse);
    }
}
