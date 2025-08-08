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
        ISlowable slowable = other.GetComponent<ISlowable>();
        if (slowable != null)
        {
            slowable.ApplySlow(slowMultiplier, duration);
            Destroy(gameObject);
            return;
        }

        if (other.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemyReceiver = other.GetComponent<EnemyAbilityReceiver>();
            if (enemyReceiver != null)
            {
                enemyReceiver.ApplyGlitchTime(slowMultiplier, duration, glitchParticlePrefab);
                Destroy(gameObject);
            }
        }

        if (other.CompareTag("Ground") || other.CompareTag("Wall") || other.CompareTag("Columns") || other.CompareTag("Roof"))
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