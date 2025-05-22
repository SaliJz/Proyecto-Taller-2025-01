using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaludJugador : MonoBehaviour
{
    public float maxHealth = 100f;
    private float currentHealth;

    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;
        Debug.Log("Salud: " + currentHealth);
        if (currentHealth <= 0)
        {
            Debug.Log("¡Jugador muerto!");
           
        }
    }

}
