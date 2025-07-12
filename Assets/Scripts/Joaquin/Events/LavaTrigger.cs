using System.Collections.Generic;
using UnityEngine;

public class LavaTrigger : MonoBehaviour
{
    [SerializeField] private int damagePerSecond = 5;
    private Dictionary<PlayerHealth, float> timers = new Dictionary<PlayerHealth, float>();

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                if (!timers.ContainsKey(playerHealth))
                    timers[playerHealth] = 0f;

                timers[playerHealth] += Time.deltaTime;

                if (timers[playerHealth] >= 1f)
                {
                    playerHealth.TakeDamage(damagePerSecond, transform.position);
                    timers[playerHealth] = 0f;
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null && timers.ContainsKey(playerHealth))
            {
                timers.Remove(playerHealth);
            }
        }
    }
}
