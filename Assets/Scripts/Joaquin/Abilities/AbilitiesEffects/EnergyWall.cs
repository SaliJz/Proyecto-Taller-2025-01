using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyWall : MonoBehaviour
{
    [Header("Wall Settings")]
    [SerializeField]
    private float damagePerSecond = 20f;

    private HashSet<EnemyAbilityReceiver> touchingEnemies = new HashSet<EnemyAbilityReceiver>();

    private void Awake()
    {
        var obstacle = GetComponent<UnityEngine.AI.NavMeshObstacle>();
        if (obstacle == null)
        {
            obstacle = gameObject.AddComponent<UnityEngine.AI.NavMeshObstacle>();
            obstacle.carving = true;
            obstacle.shape = UnityEngine.AI.NavMeshObstacleShape.Box;
            var col = GetComponent<Collider>();
            if (col is BoxCollider box)
            {
                obstacle.size = box.size;
            }
        }
    }

    public void Initialize(float duration)
    {
        Destroy(gameObject, duration);
        StartCoroutine(DamageRoutine());
    }

    private IEnumerator DamageRoutine()
    {
        while (true)
        {
            foreach (var enemy in touchingEnemies)
            {
                if (enemy != null)
                {
                    enemy.TakeDamage(damagePerSecond);
                }
            }
            yield return new WaitForSeconds(1.0f);
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<EnemyAbilityReceiver>();
        if (enemy != null)
        {
            if (!touchingEnemies.Contains(enemy))
            {
                touchingEnemies.Add(enemy);
                Debug.Log($"Enemy {enemy.name} entered the wall and will take {damagePerSecond} damage per second.");
            }
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        var enemy = collision.gameObject.GetComponent<EnemyAbilityReceiver>();
        if (enemy != null)
        {
            if (touchingEnemies.Contains(enemy))
            {
                touchingEnemies.Remove(enemy);
            }
        }
    }
}