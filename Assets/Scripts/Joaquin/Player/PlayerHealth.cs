using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("Salud")]
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    [Header("Escudo")]
    [SerializeField] private int maxShield = 20;
    private int currentShield;
    [SerializeField] private float shieldRegenRate = 4f;
    private bool isRegenShield;

    [Header("Muerte y transición")]
    [SerializeField] private string gameOverSceneName = "GameOver";
    [SerializeField] private GameObject deathPrefab;
    [SerializeField] private SceneTransition sceneTransition;

    private bool isIgnited = false;
    private Coroutine ignitionCoroutine;

    private void Start()
    {
        maxHealth = UpgradeDataStore.Instance.playerMaxHealth;
        currentHealth = maxHealth;
        currentShield = maxShield;

        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
        HUDManager.Instance.UpdateShield(currentShield, maxShield);
    }

    private void Update()
    {
        // Regeneración del escudo
        if (currentShield < maxShield && !isRegenShield)
        {
            StartCoroutine(RegenShield());
        }
    }

    public void TakeDamage(int damage)
    {
        int damageLeft = damage;

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

            HUDManager.Instance.UpdateShield(currentShield, maxShield);
        }

        if (damageLeft > 0)
        {
            currentHealth -= damageLeft;
            HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    private IEnumerator RegenShield()
    {
        isRegenShield = true;

        while (currentShield < maxShield)
        {
            yield return new WaitForSeconds(1f);
            currentShield += Mathf.RoundToInt(shieldRegenRate);
            currentShield = Mathf.Clamp(currentShield, 0, maxShield);
            HUDManager.Instance.UpdateShield(currentShield, maxShield);
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
            TakeDamage((int)damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
    }

    private void Die()
    {
        EnemySpawner enemySpawner = FindObjectOfType<EnemySpawner>();
        if (enemySpawner != null)
        {
            enemySpawner.ResetSpawner();
        }
        UpgradeDataStore.Instance.ResetTemporaryUpgrades();
        DeathManager.Instance.RegisterDeath(deathPrefab, transform.position);
        if (sceneTransition != null)
        {
            sceneTransition.LoadSceneWithFade(gameOverSceneName);
        }
        else
        {
            SceneManager.LoadScene(gameOverSceneName);
        }
    }
}
