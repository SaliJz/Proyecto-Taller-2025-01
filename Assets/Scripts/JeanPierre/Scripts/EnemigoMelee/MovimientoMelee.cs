using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovimientoMelee : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    [SerializeField] private Transform playerTransform;

    [Header("Parámetros de Movimiento")]
    [SerializeField] private float startVelocity = 5f;
    [SerializeField] private float maxVelocity = 10f;
    [SerializeField] private float acceleration = 0.1f;
    [SerializeField] private float minDistance = 1f;

    [Header("Parámetros de Salto")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private float jumpDistance = 8f;
    [SerializeField] private float longJumpDistance = 15f;
    [SerializeField] private float jumpHeightThreshold = 2f;
    [SerializeField] private float waitTimeBeforeJump = 2f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private float raycastDistance = 20f;
    [SerializeField] private CapsuleCollider targetCollider;

    private float currentVelocity;
    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;
    private Rigidbody rb;

    private bool isJumping = false;
    private bool canJump = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private Vector3 lastPlayerPosition;
    private float stuckTimer = 0f;
    private float maxStuckTime = 3f;

    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        rb.isKinematic = true;
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        agent.updatePosition = true;
        agent.updateRotation = true;
        currentVelocity = startVelocity;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 1f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning("No se encontró posición de NavMesh cercana.");
        }

        if (playerTransform == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else Debug.LogError("No se encontró objeto con tag 'Player'.");
        }

        lastPlayerPosition = playerTransform.position;
    }

    void Update()
    {
        if (abilityReceiver == null || abilityReceiver.CurrentTarget == null) return;

        currentVelocity = abilityReceiver.CurrentSpeed;

        if (isJumping)
        {
            HandleJumpMovement();
            return;
        }

        if (isWaiting)
        {
            HandleWaitState();
            return;
        }

        HandleNormalMovement();
        CheckForJumpOpportunity();
    }

    void HandleNormalMovement()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        float distancia = Vector3.Distance(transform.position, abilityReceiver.CurrentTarget.position);

        if (distancia > minDistance)
        {
            currentVelocity = Mathf.Min(currentVelocity + acceleration * Time.deltaTime, maxVelocity);
            agent.speed = currentVelocity;
            agent.SetDestination(abilityReceiver.CurrentTarget.position);

            if (agent.pathStatus == NavMeshPathStatus.PathPartial ||
                Vector3.Distance(lastPlayerPosition, playerTransform.position) > 0.5f)
            {
                stuckTimer += Time.deltaTime;
            }
            else
            {
                stuckTimer = 0f;
            }

            lastPlayerPosition = playerTransform.position;
        }
        else
        {
            currentVelocity = startVelocity;
            agent.speed = currentVelocity;
            agent.ResetPath();
            stuckTimer = 0f;
        }
    }

    void CheckForJumpOpportunity()
    {
        if (!canJump || isJumping) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float heightDifference = playerTransform.position.y - transform.position.y;

        bool isStuck = stuckTimer > maxStuckTime;
        bool isDifferentLevel = distanceToPlayer > jumpDistance && heightDifference > jumpHeightThreshold;
        bool isFarOnSameLevel = distanceToPlayer > longJumpDistance && Mathf.Abs(heightDifference) < 1f;

        bool shouldConsiderJump = isStuck || isDifferentLevel || isFarOnSameLevel;

        if (shouldConsiderJump && HasClearLineOfSight())
        {
            StartWaitingForJump();
        }
    }

    bool HasClearLineOfSight()
    {
        Vector3 rayStart = transform.position + Vector3.up * 1f;
        Vector3 rayEnd = playerTransform.position + Vector3.up * 1f;
        Vector3 rayDirection = (rayEnd - rayStart).normalized;
        float rayDistance = Vector3.Distance(rayStart, rayEnd);

        RaycastHit hit;
        if (Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, obstacleLayer))
        {
            if (targetCollider != null)
            {
                return hit.collider == targetCollider;
            }
            else
            {
                return hit.collider.transform == playerTransform;
            }
        }

        return true;
    }

    void StartWaitingForJump()
    {
        isWaiting = true;
        waitTimer = 0f;
        agent.ResetPath();
        agent.enabled = false;
        rb.isKinematic = false;
    }

    void HandleWaitState()
    {
        waitTimer += Time.deltaTime;

        if (waitTimer >= waitTimeBeforeJump)
        {
            PerformJump();
        }
    }

    void PerformJump()
    {
        isWaiting = false;
        isJumping = true;
        canJump = false;

        Vector3 jumpDirection = (playerTransform.position - transform.position).normalized;
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer > longJumpDistance)
        {
            jumpDirection.y = 0.3f;
        }
        else
        {
            jumpDirection.y = 0.5f;
        }

        rb.AddForce(jumpDirection * jumpForce, ForceMode.Impulse);

        Invoke(nameof(ResetJumpCooldown), 5f);
    }

    void HandleJumpMovement()
    {
        if (rb.velocity.y <= 0 && IsGrounded())
        {
            isJumping = false;
            rb.isKinematic = true;
            agent.enabled = true;

            if (agent.isOnNavMesh)
            {
                agent.SetDestination(playerTransform.position);
            }
            else
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                    agent.SetDestination(playerTransform.position);
                }
            }
        }
    }

    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.2f, groundLayer);
    }

    void ResetJumpCooldown()
    {
        canJump = true;
        stuckTimer = 0f;

        if (!isJumping && !isWaiting)
        {
            rb.isKinematic = true;
        }
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, jumpDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, longJumpDistance);

        Vector3 rayStart = transform.position + Vector3.up * 1f;
        Vector3 rayEnd = playerTransform.position + Vector3.up * 1f;
        Vector3 rayDirection = (rayEnd - rayStart).normalized;
        float rayDistance = Vector3.Distance(rayStart, rayEnd);

        Gizmos.color = HasClearLineOfSight() ? Color.green : Color.red;
        Gizmos.DrawRay(rayStart, rayDirection * rayDistance);

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.2f);

        if (isWaiting)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        if (isJumping)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.8f);
        }

        Gizmos.color = Color.cyan;
        Vector3 jumpTarget = playerTransform.position;
        jumpTarget.y = transform.position.y;
        Gizmos.DrawLine(transform.position, jumpTarget);

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
        float heightDifference = playerTransform.position.y - transform.position.y;

        if (distanceToPlayer > longJumpDistance && Mathf.Abs(heightDifference) < 1f)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 3f, Vector3.one * 0.3f);
        }
    }
}