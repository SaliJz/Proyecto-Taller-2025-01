using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LavaDamage : MonoBehaviour
{
    public int damagePerSecond = 5;
    private Dictionary<GameObject, float> nextDamageTime = new Dictionary<GameObject, float>();


    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("PLAYER"))
        {
            if (!nextDamageTime.ContainsKey(other.gameObject))
                nextDamageTime[other.gameObject] = 0f;

            if (Time.time >= nextDamageTime[other.gameObject])
            {
                JugadorLava playerHealth = other.GetComponent<JugadorLava>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damagePerSecond);
                    nextDamageTime[other.gameObject] = Time.time + 1f; 
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (nextDamageTime.ContainsKey(other.gameObject))
        {
            nextDamageTime.Remove(other.gameObject);
        }
    }
}
