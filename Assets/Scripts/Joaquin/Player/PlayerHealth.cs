using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;
    [SerializeField] private GameObject deathPrefab;

    private void Start()
    {
        currentHealth = maxHealth;
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        HUDManager.Instance.UpdateHealth(currentHealth, maxHealth);
    }

    private void Die()
    {
        Instantiate(deathPrefab, transform.position, Quaternion.identity);
        gameObject.SetActive(false);
    }
}
