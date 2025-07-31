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
    [SerializeField] private float stopDistance = 1.5f; 

    [Header("Parámetros de Salto")]
    [SerializeField] private float jumpDistance = 8f;
    [SerializeField] private float longJumpDistance = 15f;
    [SerializeField] private float jumpHeightThreshold = 3f;
    [SerializeField] private float waitTimeBeforeJump = 1.5f;
    [SerializeField] private LayerMask groundLayer = -1;
    [SerializeField] private LayerMask obstacleLayer = -1;
    [SerializeField] private CapsuleCollider targetCollider;

    [Header("Movimiento con Lerp")]
    [SerializeField] private float lerpDuration = 1f;
    [SerializeField] private float lerpHeight = 5f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Detección de Condiciones de Salto")]
    [SerializeField] private float gapDetectionDistance = 3f;
    [SerializeField] private float pathLengthThreshold = 25f; 
    [SerializeField] private float stuckTimeThreshold = 4f; 
    [SerializeField] private float playerMovementThreshold = 3f;

    [Header("Color")]
    [SerializeField] private Color colorJumpDistance;
    [SerializeField] private Color colorLongJumpDistance;
    [SerializeField] private Color colorIsWaiting;
    [SerializeField] private Color colorIsJumping;
    [SerializeField] private Color colorPathLine;

    private float currentVelocity;
    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;

    private bool isJumping = false;
    private bool canJump = true;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private Vector3 lastPosition;
    private float stuckTimer = 0f;
    private Vector3 lastPlayerPosition;
    private float timeSincePlayerMoved = 0f;

    private bool isLerpMoving = false;
    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;
    private float lerpTimer = 0f;

    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.stoppingDistance = stopDistance;
        currentVelocity = startVelocity;

        if (!agent.isOnNavMesh)
        {
            NavMeshHit hit;
            if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
            else
                Debug.LogWarning("No se encontró posición de NavMesh cercana.");
        }

        if (playerTransform == null)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else Debug.LogError("No se encontró objeto con tag 'Player'.");
        }

        lastPosition = transform.position;
        lastPlayerPosition = playerTransform.position;
    }

    void Update()
    {
        if (abilityReceiver == null || abilityReceiver.CurrentTarget == null) return;

        currentVelocity = abilityReceiver.CurrentSpeed;

        if (isLerpMoving)
        {
            HandleLerpMovement();
            return;
        }

        if (isWaiting)
        {
            HandleWaitState();
            return;
        }

        HandleMainMovement();
    }

    void HandleMainMovement()
    {
        if (!agent.enabled || !agent.isOnNavMesh) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (distanceToPlayer <= stopDistance)
        {
            agent.ResetPath();
            return;
        }

        agent.speed = currentVelocity;
        agent.SetDestination(abilityReceiver.CurrentTarget.position);

        CheckIfJumpNeeded(distanceToPlayer);
    }

    void CheckIfJumpNeeded(float distanceToPlayer)
    {
        if (!canJump) return;

        if (distanceToPlayer < jumpDistance) return;

        if (HasGapBetweenEnemyAndPlayer())
        {
            StartWaitingForJump("Gap detected");
            return;
        }

        float heightDifference = Mathf.Abs(playerTransform.position.y - transform.position.y);
        if (heightDifference > jumpHeightThreshold && distanceToPlayer > jumpDistance)
        {
            if (IsPathTooLongOrInvalid(distanceToPlayer))
            {
                StartWaitingForJump("Height difference and long path");
                return;
            }
        }

        if (distanceToPlayer > longJumpDistance)
        {
            if (IsStuckOrPathTooLong(distanceToPlayer))
            {
                StartWaitingForJump("Player too far and stuck/long path");
                return;
            }
        }
    }

    bool HasGapBetweenEnemyAndPlayer()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        int checkPoints = Mathf.RoundToInt(distance / gapDetectionDistance);
        checkPoints = Mathf.Max(3, checkPoints);

        for (int i = 1; i < checkPoints; i++)
        {
            float t = (float)i / checkPoints;
            Vector3 checkPos = Vector3.Lerp(transform.position, playerTransform.position, t);

            if (!Physics.Raycast(checkPos + Vector3.up * 0.5f, Vector3.down, 3f, groundLayer))
            {
                return true; 
            }
        }

        return false;
    }

    bool IsPathTooLongOrInvalid(float directDistance)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(playerTransform.position, path))
        {
            if (path.status == NavMeshPathStatus.PathComplete)
            {
                float pathLength = GetPathLength(path);
                return pathLength > directDistance * 2f || pathLength > pathLengthThreshold;
            }
            else
            {
                return true; 
            }
        }
        return true; 
    }

    bool IsStuckOrPathTooLong(float directDistance)
    {
        if (Vector3.Distance(transform.position, lastPosition) < 0.1f)
        {
            stuckTimer += Time.deltaTime;
        }
        else
        {
            stuckTimer = 0f;
            lastPosition = transform.position;
        }

        if (stuckTimer > stuckTimeThreshold)
        {
            return true;
        }

        return IsPathTooLongOrInvalid(directDistance);
    }

    float GetPathLength(NavMeshPath path)
    {
        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }

    void StartWaitingForJump(string reason = "")
    {
        Debug.Log($"Starting jump sequence: {reason}");
        isWaiting = true;
        waitTimer = 0f;
        agent.ResetPath();
    }

    void HandleWaitState()
    {
        waitTimer += Time.deltaTime;

        if (Vector3.Distance(lastPlayerPosition, playerTransform.position) > playerMovementThreshold)
        {
            CancelJump();
            return;
        }

        float currentDistance = Vector3.Distance(transform.position, playerTransform.position);
        if (currentDistance <= jumpDistance)
        {
            CancelJump();
            return;
        }

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

        Vector3 targetPosition = GetBestLandingPosition();

        lerpStartPosition = transform.position;
        lerpTargetPosition = targetPosition;
        lerpTimer = 0f;
        isLerpMoving = true;

        agent.enabled = false;

        Invoke(nameof(ResetJumpCooldown), 3f);

        Debug.Log($"Jumping from {transform.position} to {targetPosition}");
    }

    void HandleLerpMovement()
    {
        lerpTimer += Time.deltaTime;
        float progress = lerpTimer / lerpDuration;

        if (progress >= 1f)
        {
            transform.position = lerpTargetPosition;
            isLerpMoving = false;
            isJumping = false;

            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    agent.Warp(hit.position);
                }
            }

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(abilityReceiver.CurrentTarget.position);
            }

            return;
        }

        float curveValue = jumpCurve.Evaluate(progress);
        Vector3 basePosition = Vector3.Lerp(lerpStartPosition, lerpTargetPosition, progress);
        float heightOffset = Mathf.Sin(progress * Mathf.PI) * lerpHeight;

        transform.position = basePosition + Vector3.up * heightOffset;
    }

    Vector3 GetBestLandingPosition()
    {
        Vector3 playerPos = playerTransform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(playerPos, out hit, 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }

        Vector3[] offsets = {
            Vector3.forward * 2f,
            Vector3.back * 2f,
            Vector3.left * 2f,
            Vector3.right * 2f,
            Vector3.forward * 1f + Vector3.right * 1f,
            Vector3.forward * 1f + Vector3.left * 1f,
            Vector3.back * 1f + Vector3.right * 1f,
            Vector3.back * 1f + Vector3.left * 1f
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 testPos = playerPos + offset;
            if (NavMesh.SamplePosition(testPos, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return playerPos;
    }

    void CancelJump()
    {
        isWaiting = false;
        isJumping = false;
        stuckTimer = 0f;
        lastPlayerPosition = playerTransform.position;

        if (!agent.enabled)
        {
            agent.enabled = true;
        }

        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(abilityReceiver.CurrentTarget.position);
        }
    }

    void ResetJumpCooldown()
    {
        canJump = true;
        stuckTimer = 0f;

        if (!isLerpMoving && !isWaiting)
        {
            if (!agent.enabled)
            {
                agent.enabled = true;
            }

            if (agent.enabled && agent.isOnNavMesh)
            {
                agent.SetDestination(abilityReceiver.CurrentTarget.position);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Gizmos.color = colorJumpDistance;
        Gizmos.DrawWireSphere(transform.position, jumpDistance);

        Gizmos.color = colorLongJumpDistance;
        Gizmos.DrawWireSphere(transform.position, longJumpDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, stopDistance);

        if (isWaiting)
        {
            Gizmos.color = colorIsWaiting;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.5f);
        }

        if (isJumping || isLerpMoving)
        {
            Gizmos.color = colorIsJumping;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.8f);
        }

        Gizmos.color = colorPathLine;
        Gizmos.DrawLine(transform.position, playerTransform.position);

        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, Vector3.down * 1.5f);

        if (isLerpMoving)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(lerpStartPosition, lerpTargetPosition);
            Gizmos.DrawWireSphere(lerpTargetPosition, 0.5f);
        }

        if (Application.isPlaying && playerTransform != null)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            int checkPoints = Mathf.RoundToInt(distance / gapDetectionDistance);
            checkPoints = Mathf.Max(3, checkPoints);

            for (int i = 1; i < checkPoints; i++)
            {
                float t = (float)i / checkPoints;
                Vector3 checkPos = Vector3.Lerp(transform.position, playerTransform.position, t);

                Gizmos.color = Color.yellow;
                Gizmos.DrawRay(checkPos + Vector3.up * 0.5f, Vector3.down * 3f);
            }
        }
    }
}