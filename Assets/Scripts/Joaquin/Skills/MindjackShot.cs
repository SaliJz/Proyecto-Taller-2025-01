using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindjackShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;

    [SerializeField] private ParticleSystem impactEffect;

    public void Initialize(float radius,float dps, float duration)
    {
        this.radius = radius;
        this.damagePerSecond = dps;
        this.duration = duration;

        transform.localScale = new Vector3(this.radius * 2, this.radius * 2, this.radius * 2);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                PlayEffect();
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Ground"))
        {
            Destroy(gameObject);
        }
    }

    private void PlayEffect()
    {
        if (impactEffect != null)
        {
            Instantiate(impactEffect, transform.position, Quaternion.identity);
            Destroy(impactEffect.gameObject, 2f);
        }
        else
        {
            Debug.LogWarning("Particle System no está asignado en MindjackShot.");
        }
    }
}
