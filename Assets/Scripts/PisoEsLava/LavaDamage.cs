using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaDamage : MonoBehaviour
{
    public int damagePerSecond = 5;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(Mathf.RoundToInt(damagePerSecond * Time.deltaTime));
            }
        }
    }
}
