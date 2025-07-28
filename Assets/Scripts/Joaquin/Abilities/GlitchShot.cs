using UnityEngine;

public class GlitchShot : MonoBehaviour
{
    private float duration;
    private float slowMultiplier;
    private float radius;
    private LayerMask enemyLayer;

    [SerializeField] private GameObject glitchParticlePrefab;
    [SerializeField] private GameObject glitchAreaPrefab;

    public void Initialize(float radius, float duration, float slowMultiplier, LayerMask enemyLayer)
    {
        this.radius = radius;
        this.duration = duration;
        this.slowMultiplier = slowMultiplier;
        this.enemyLayer = enemyLayer;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemy = other.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyGlitchTime(slowMultiplier, duration, glitchParticlePrefab);
                Destroy(gameObject);
            }
        }
        
        if (other.CompareTag("Ground"))
        {
            ApplyGlitchArea(transform.position);
            Destroy(gameObject);
        }

        if (other.CompareTag("Wall"))
        {
            ApplyGlitchArea(transform.position);
            Destroy(gameObject);
        }

        if (other.CompareTag("Columns"))
        {
            ApplyGlitchArea(transform.position);
            Destroy(gameObject);
        }

        if (other.CompareTag("Roof"))
        {
            ApplyGlitchArea(transform.position);
            Destroy(gameObject);
        }
    }

    private void ApplyGlitchArea(Vector3 center)
    {
        if (glitchAreaPrefab != null)
        {
            GameObject glitchArea = Instantiate(glitchAreaPrefab, center, Quaternion.identity);
            GlitchTimeArea areaEffect = glitchArea.GetComponent<GlitchTimeArea>();
            if (areaEffect != null)
            {
                areaEffect.Initialize(radius, duration, slowMultiplier, enemyLayer);
            }
        }
        else
        {
            Debug.LogError("Glitch Time Area Effect Prefab no está asignado en GlitchShot.");
        }
    }
}