using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindjackShot : MonoBehaviour
{
    private float damagePerSecond;
    private float duration;

    public void Initialize(float dps, float duration)
    {
        this.damagePerSecond = dps;
        this.duration = duration;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemy = collision.collider.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
            }
        }

        Destroy(gameObject);
    }
}
