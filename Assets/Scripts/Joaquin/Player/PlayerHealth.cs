using System;
using System.Collections;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private int baseMaxHealth = 100;
    private int currentHealth;

    [Header("Escudo")]
    [SerializeField] private int baseMaxShield = 20;
    private int currentShield;
    [SerializeField] private float shieldRegenRate = 4f;
    private bool isRegenShield;

    [Header("Muerte y transición")]
    //[SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private GameObject deathPrefab;

    [SerializeField] private bool isTutorial = false;

    [Header("Referencias del HUD")]
    [SerializeField] private Active_ShieldHUD directionalDamageHUD;

    private bool isIgnited = false;
    private Coroutine ignitionCoroutine;

    private void Awake()
    {
        if (isTutorial)
        {
            DataManager.IsTutorial = true;
            DataManager.ResetData();
        }
        else DataManager.IsTutorial = false;
    }

    private void Start()
    {
        if (directionalDamageHUD == null) directionalDamageHUD = FindObjectOfType<Active_ShieldHUD>();

        ResetToBaseStats();
        ApplyUpgrades();
        HUDManager.Instance?.UpdateHealth(currentHealth, GetMaxHealth());
        HUDManager.Instance?.UpdateShield(currentShield, GetMaxShield());
    }

    private void OnEnable()
    {
        DataManager.OnDataChanged += ApplyUpgrades;
    }

    private void OnDisable()
    {
        DataManager.OnDataChanged -= ApplyUpgrades;
    }

    private void ApplyUpgrades()
    {
        // Obtiene niveles de upgrade de DataManager
        int healthLevel = DataManager.HealthUpgradeLevel;
        int shieldLevel = DataManager.ShieldUpgradeLevel;

        // Constantes de mejoras
        const float HEALTH_INC = 0.1f; // +10% por nivel
        const float SHIELD_INC = 0.05f; // +5% por nivel

        // Calcula máximos con upgrades
        int upgradedMaxHealth = Mathf.RoundToInt(baseMaxHealth * (1 + healthLevel * HEALTH_INC));
        int upgradedMaxShield = Mathf.RoundToInt(baseMaxShield * (1 + shieldLevel * SHIELD_INC));

        // Mantiene porcentajes actuales
        float healthPct = (float)currentHealth / GetMaxHealth();
        float shieldPct = (float)currentShield / GetMaxShield();

        // Actualiza base para futuras referencias
        baseMaxHealth = upgradedMaxHealth;
        baseMaxShield = upgradedMaxShield;

        // Ajusta valores actuales según nuevo máximo
        currentHealth = Mathf.RoundToInt(upgradedMaxHealth * healthPct);
        currentShield = Mathf.RoundToInt(upgradedMaxShield * shieldPct);

        HUDManager.Instance?.UpdateHealth(currentHealth, upgradedMaxHealth);
        HUDManager.Instance?.UpdateShield(currentShield, upgradedMaxShield);
    }

    private int GetMaxHealth() => baseMaxHealth;
    private int GetMaxShield() => baseMaxShield;

    private void ResetToBaseStats()
    {
        currentHealth = baseMaxHealth;
        currentShield = baseMaxShield;
    }

    private void Update()
    {
        if (currentShield < baseMaxShield && !isRegenShield)
        {
            StartCoroutine(RegenShield());
        }
    }

    public void TakeDamage(int damage, Vector3 attackerPosition)
    {
        Vector3 damageDirection = (attackerPosition - transform.position).normalized;

        int damageLeft = damage;

        if (directionalDamageHUD != null)
        {
            Debug.LogWarning($"Dirección del daño: {damageDirection}");
            directionalDamageHUD.ActivateIndicator(damageDirection);
        }

        if (currentShield > 0)
        {
            if (damage <= currentShield)
            {
                currentShield -= damage;
                damageLeft = 0;
            }
            else
            {
                damageLeft = damage - currentShield;
                currentShield = 0;
            }

            HUDManager.Instance.UpdateShield(currentShield, GetMaxShield());
        }

        if (damageLeft > 0)
        {
            currentHealth -= damageLeft;
            HUDManager.Instance.UpdateHealth(currentHealth, GetMaxHealth());

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                HUDManager.Instance.UpdateHealth(currentHealth, GetMaxHealth());
                Die();
            }
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, GetMaxHealth());
        HUDManager.Instance.UpdateHealth(currentHealth, GetMaxHealth());
    }

    private IEnumerator RegenShield()
    {
        isRegenShield = true;

        while (currentShield < baseMaxShield)
        {
            yield return new WaitForSeconds(1f);
            currentShield += Mathf.RoundToInt(shieldRegenRate);
            currentShield = Mathf.Clamp(currentShield, 0, GetMaxShield());
            HUDManager.Instance.UpdateShield(currentShield, GetMaxShield());
        }

        isRegenShield = false;
    }

    public void ApplyIgnition(float damagePerSecond, float duration)
    {
        if (isIgnited) return;
        isIgnited = true;

        // Detener la coroutine anterior si está activa
        if (ignitionCoroutine != null)
        {
            StopCoroutine(ignitionCoroutine);
        }

        StartCoroutine(IgnitionRoutine(damagePerSecond, duration));
    }

    private IEnumerator IgnitionRoutine(float damagePerSecond, float duration)
    {
        int ticks = Mathf.FloorToInt(duration);
        for (int i = 0; i < ticks; i++)
        {
            TakeDamage((int)damagePerSecond, transform.position);
            yield return new WaitForSeconds(1f);
        }
    }

    public void Die()
    {
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner();
        }

        if (deathPrefab != null)
        {
            DeathManager.Instance.RegisterDeath(deathPrefab, transform.position);
        }

        GameManager.Instance?.PlayerDied();
    }
}