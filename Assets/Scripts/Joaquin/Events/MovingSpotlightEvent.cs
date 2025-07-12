using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MovingSpotlightEvent : MonoBehaviour
{
    private Dictionary<PlayerHealth, Coroutine> damageCoroutines = new Dictionary<PlayerHealth, Coroutine>();

    [Header("Movimiento")]
    [SerializeField] private Vector3[] waypoints;
    [SerializeField] private float spotlightSpeed = 2f;
    [SerializeField] private float changePatternInterval = 5f;

    [Header("Daño")]
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private LayerMask playerLayer;

    private int currentWaypoint = 0;
    private float patternTimer;

    private void Update()
    {
        MoveToNextWaypoint();
        patternTimer += Time.deltaTime;

        if (patternTimer >= changePatternInterval)
        {
            ChangePattern();
            patternTimer = 0f;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.down);
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        Vector3 target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target, spotlightSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target) < 0.2f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    private void ChangePattern()
    {
        // Mezclar los waypoints para variar el patrón
        waypoints = waypoints.OrderBy(x => UnityEngine.Random.value).ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null && !damageCoroutines.ContainsKey(player))
            {
                Coroutine c = StartCoroutine(DamageOverTime(player));
                damageCoroutines[player] = c;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null && damageCoroutines.ContainsKey(player))
        {
            StopCoroutine(damageCoroutines[player]);
            damageCoroutines.Remove(player);
        }
    }

    private IEnumerator DamageOverTime(PlayerHealth player)
    {
        while (true)
        {
            player.TakeDamage((int)damagePerSecond, transform.position);
            yield return new WaitForSeconds(1f);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (waypoints != null && waypoints.Length > 0)
        {
            for (int i = 0; i < waypoints.Length; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.3f);
                if (i < waypoints.Length - 1)
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
            }

            if (waypoints.Length > 1)
                Gizmos.DrawLine(waypoints[waypoints.Length - 1], waypoints[0]);
        }
    }
}
