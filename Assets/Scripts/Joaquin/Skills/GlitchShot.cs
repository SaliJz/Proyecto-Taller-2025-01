using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlitchShot : MonoBehaviour
{
    private float slowDuration;
    private float slowMultiplier;
    private float radius;

    public void Initialize(float radius, float slowDuration, float slowMultiplier)
    {
        this.radius = Mathf.Max(0.1f, radius);
        this.slowDuration = Mathf.Max(1f, slowDuration);
        this.slowMultiplier = Mathf.Clamp(slowMultiplier, 0f, 1f);
    }

    private void Start()
    {
        gameObject.transform.localScale = new Vector3(radius, radius, radius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemy = collision.gameObject.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplySlow(slowMultiplier, slowDuration);
                Destroy(gameObject);
            }
        }
    }
}
