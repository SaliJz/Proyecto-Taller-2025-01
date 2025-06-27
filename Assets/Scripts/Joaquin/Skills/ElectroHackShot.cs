using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ElectroHackShot : MonoBehaviour
{
    private float radius;
    private float maxTargets;
    private float damagePerSecond;
    private float duration;
    private float slowMultiplier;
    private bool hasExploded = false;
    private LayerMask targetLayer;

    [SerializeField] private GameObject electroAreaEffectPrefab;

    public void Initialize(float radius, float maxTargets, float damagePerSecond, float duration, float slowMultiplier, LayerMask targetLayer)
    {
        this.radius = radius; 
        this.maxTargets = maxTargets;
        this.damagePerSecond = damagePerSecond;
        this.duration = duration;
        this.slowMultiplier = slowMultiplier;
        this.targetLayer = targetLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (hasExploded) return;

        if (other.CompareTag("Enemy"))
        {
            hasExploded = true;
            ApplyElectroHack(transform.position);
            Destroy(gameObject);
        }
        else if (other.CompareTag("Ground"))
        {
            hasExploded = true;
            ApplyElectroArea(transform.position);
            Destroy(gameObject);
        }
    }

    private void ApplyElectroHack(Vector3 center)
    {
        Collider[] hits = Physics.OverlapSphere(center, radius, targetLayer);
        int affectedCount = 0;

        foreach (Collider col in hits)
        {
            if (affectedCount >= maxTargets) break;

            if (col.CompareTag("Enemy"))
            {
                EnemyAbilityReceiver enemy = col.GetComponent<EnemyAbilityReceiver>();
                if (enemy != null)
                {
                    enemy.ApplyElectroHack(damagePerSecond, duration, slowMultiplier);
                    affectedCount++;
                }
            }
        }
    }

    private void ApplyElectroArea(Vector3 center)
    {
        if (electroAreaEffectPrefab != null)
        {
            GameObject electroArea = Instantiate(electroAreaEffectPrefab, center, Quaternion.identity);
            ElectroAreaEffect areaEffect = electroArea.GetComponent<ElectroAreaEffect>();
            if (areaEffect != null)
            {
                areaEffect.Initialize(radius, damagePerSecond, duration, targetLayer);
            }
        }
        else
        {
            Debug.LogError("Electro Area Effect Prefab no está asignado en ElectroHackShot.");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.4f);
        Gizmos.DrawSphere(transform.position, radius);
    }
}
