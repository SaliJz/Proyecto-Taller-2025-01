using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 30;
    private int currentHealth;


    void Start()
    {
        currentHealth = maxHealth;
       
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibió " + amount + " de daño. Vida restante: " + currentHealth);

        if (currentHealth <= 0) 
        {
            Debug.Log(gameObject.name + " fue destruido.");
            Destroy(gameObject);
        }
    }
}
