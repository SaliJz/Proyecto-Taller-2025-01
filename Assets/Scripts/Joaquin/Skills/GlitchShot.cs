using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchShot : MonoBehaviour
{
    [SerializeField] private float slowDuration = 5f;
    [SerializeField] private float slowMultiplier = 0.5f;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy")) // Asegúrate de que el enemigo tenga este tag
        {
            EnemyAITest enemy = other.GetComponent<EnemyAITest>();
            if (enemy != null)
            {
                enemy.ApplySlow(slowMultiplier, slowDuration);
            }
            Destroy(gameObject); // Se destruye al impactar
        }
    }
}
