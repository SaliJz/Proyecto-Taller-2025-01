using UnityEngine;

public class PlayerKnockback : MonoBehaviour
{
    [Header("Configuraci�n de Empuje")]
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

        // Escala la fuerza basada en el da�o (opcional)
        float damageMultiplier = Mathf.Clamp01(damageAmount / 25f); // 25 de da�o = fuerza base

        // La fuerza es en la misma direcci�n del da�o, empujando al jugador
        Vector3 forceDirection = (damageDirection + Vector3.up * knockbackUpwardForce).normalized;

        // Aplicamos la fuerza. Usamos Impulse para una explosi�n instant�nea.
        rb.AddForce(forceDirection * knockbackForce * damageMultiplier, ForceMode.Impulse);
    }
}
