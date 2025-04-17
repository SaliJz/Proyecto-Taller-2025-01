using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchShot : MonoBehaviour
{
    private float slowDuration;
    private float slowMultiplier;

    public void Initialize(float slowDuration, float slowMultiplier)
    {
        this.slowDuration = slowDuration;
        this.slowMultiplier = slowMultiplier;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAITest enemy = collision.gameObject.GetComponent<EnemyAITest>();
            if (enemy != null)
            {
                enemy.ApplySlow(slowMultiplier, slowDuration);
            }
        }

        Destroy(gameObject);
    }
}
