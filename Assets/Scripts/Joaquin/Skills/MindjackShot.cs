using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MindjackShot : MonoBehaviour
{
    private float radius;
    private float damagePerSecond;
    private float duration;

    public void Initialize(float radius,float dps, float duration)
    {
        this.radius = Mathf.Max(0.1f, radius);
        this.damagePerSecond = Mathf.Max(0f, dps);
        this.duration = Mathf.Max(1f, duration);
    }

    private void Start()
    {
        gameObject.transform.localScale = new Vector3(radius, radius, radius);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Enemy"))
        {
            EnemyAbilityReceiver enemy = collision.collider.GetComponent<EnemyAbilityReceiver>();
            if (enemy != null)
            {
                enemy.ApplyMindjack(damagePerSecond, duration);
                Destroy(gameObject);
            }
        }
    }
}
