using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectroHackShot : MonoBehaviour
{
    private float radius;
    private int maxTargets;
    private float tickDamage;
    private float tickInterval;
    private int ticks;
    private float slowMultiplier;
    private bool hasExploded = false;

    public void Initialize(float radius, int maxTargets, float tickDamage, float tickInterval, int ticks, float slowMultiplier)
    {
        this.radius = Mathf.Max(0.1f, radius); 
        this.maxTargets = Mathf.Max(1, maxTargets);
        this.tickDamage = Mathf.Max(0f, tickDamage);
        this.tickInterval = Mathf.Max(0.01f, tickInterval);
        this.ticks = Mathf.Max(1, ticks);
        this.slowMultiplier = Mathf.Clamp(slowMultiplier, 0f, 1f);
    }

    private void Start()
    {
        gameObject.transform.localScale = new Vector3(radius, radius, radius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (hasExploded) return;
        hasExploded = true;

        Collider[] hits = Physics.OverlapSphere(transform.position, radius);
        HashSet<EnemyAbilityReceiver> affectedEnemies = new HashSet<EnemyAbilityReceiver>();

        int affected = 0;

        foreach (Collider col in hits)
        {
            if (affected >= maxTargets) break;

            if (col.CompareTag("Enemy"))
            {
                EnemyAbilityReceiver enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null && !affectedEnemies.Contains(enemy))
                {
                    enemy.ApplyElectroHack(tickDamage, tickInterval, ticks, slowMultiplier);
                    enemy.ApplySlow(slowMultiplier, ticks * tickInterval);
                    affectedEnemies.Add(enemy);
                    affected++;

                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f); // Azul cian
        Gizmos.DrawSphere(transform.position, radius);
    }
}
