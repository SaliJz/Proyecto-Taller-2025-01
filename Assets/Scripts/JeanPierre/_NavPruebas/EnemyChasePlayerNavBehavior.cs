using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyChasePlayerNavBehavior : MonoBehaviour
{
    [Tooltip("Transform of the player to chase. If null, will look for GameObject tagged 'Player' on Start.")]
    public Transform player;

    [Tooltip("Maximum distance at which the enemy will begin chasing.")]
    public float chaseDistance = 20f;

    [Tooltip("Distance at which the enemy will stop moving toward the player.")]
    public float stoppingDistance = 2f;

    private NavMeshAgent agent;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (player == null)
        {
            var playerGO = GameObject.FindGameObjectWithTag("Player");
            if (playerGO != null)
                player = playerGO.transform;
            else
                Debug.LogError("EnemyChasePlayerNavBehavior: No player assigned and no GameObject with tag 'Player' found.");
        }
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= chaseDistance)
        {
            agent.isStopped = false;
            agent.SetDestination(player.position);

            // Stop and face the player when within stopping distance
            if (distance <= stoppingDistance)
            {
                agent.isStopped = true;
                Vector3 lookDir = (player.position - transform.position).normalized;
                lookDir.y = 0f;
                if (lookDir.sqrMagnitude > 0f)
                    transform.rotation = Quaternion.LookRotation(lookDir);
            }
        }
        else
        {
            agent.isStopped = true;
        }
    }
}
