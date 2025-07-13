using UnityEngine;

public class MindjackShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;

    [SerializeField] private ParticleSystem impactEffect;

    public void Initialize(float radius, float dps, float duration)
    {
        this.radius = radius;
        this.damagePerSecond = dps;
        this.duration = duration;
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
        
        if (other.CompareTag("Ground"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Columns"))
        {
            PlayEffect();
            Destroy(gameObject);
        }

        if (other.CompareTag("Roof"))
        {
            PlayEffect();
            Destroy(gameObject);
        }
    }

    private void PlayEffect()
    {
        if (impactEffect != null)
        {
            GameObject impactInstance = Instantiate(impactEffect, transform.position, Quaternion.identity).gameObject;
            Destroy(impactInstance, 3f);
        }
        else
        {
            Debug.LogWarning("Particle System no está asignado en MindjackShot.");
        }
    }
}