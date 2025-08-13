using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovimientoMelee : MonoBehaviour
{
    private enum EnemyState
    {
        Chasing,
        Idle,
        Jumping,
        Landing
    }

    [Header("Referencia al Jugador")]
    [SerializeField] private Transform playerTransform;

    [Header("Detección del Jugador")]
    [SerializeField] private float detectionRange = 15f;

    [Header("Parámetros de Movimiento")]
    [SerializeField] private float stopDistance = 1.5f;

    [Header("Parámetros de Salto")]
    [SerializeField] private float minJumpDistance = 5f;
    [SerializeField] private float maxJumpHeight = 5f;
    [SerializeField] private float lerpDuration = 1.5f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private LayerMask groundLayer = 1;
    [SerializeField] private LayerMask obstacleLayer = 1;
    [SerializeField] private float obstacleCheckRadius = 0.8f;
    [SerializeField] private float groundCheckDistance = 2f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Visualización")]
    [SerializeField] private Color colorDetectionRange = Color.red;
    [SerializeField] private Color colorJumpTrajectory = Color.cyan;

    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;

    private bool hasBeenDamaged = false;
    private float damageDetectionDuration = 10f;
    private float damageTimer = 0f;
    private bool playerInRange = false;
    private bool isGrounded = true;
    private bool canJump = false;

    private EnemyState currentState = EnemyState.Idle;
    private Vector3 jumpStartPosition;
    private Vector3 jumpTargetPosition;
    private float jumpProgress = 0f;
    private bool isLerpMoving = false;

    void Start()
    {
        abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        agent = GetComponent<NavMeshAgent>();

        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.stoppingDistance = stopDistance;
        agent.autoTraverseOffMeshLink = true;

        if (!agent.isOnNavMesh)
        {
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 5f, NavMesh.AllAreas))
                agent.Warp(hit.position);
        }

        if (playerTransform == null)
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    void Update()
    {
        if (abilityReceiver == null || playerTransform == null)
        {
            currentState = EnemyState.Idle;
            return;
        }

        CheckGrounded();
        CheckPlayerInRange();
        CheckJumpViability();

        switch (currentState)
        {
            case EnemyState.Idle:
                if (playerInRange)
                {
                    currentState = EnemyState.Chasing;
                    StartChasing();
                }
                break;

            case EnemyState.Chasing:
                if (!playerInRange)
                {
                    currentState = EnemyState.Idle;
                    StopChasing();
                }
                else
                {
                    HandleChasingMovement();
                }
                break;

            case EnemyState.Jumping:
                HandleJumpMovement();
                break;

            case EnemyState.Landing:
                currentState = EnemyState.Chasing;
                StartChasing();
                break;
        }
    }

    void CheckJumpViability()
    {
        canJump = false;

        if (!isGrounded || playerTransform == null || !playerInRange)
        {
            return;
        }

        float horizontalDistance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                     new Vector3(playerTransform.position.x, 0, playerTransform.position.z));
        float verticalDistance = Mathf.Abs(playerTransform.position.y - transform.position.y);

        bool distanceCondition = horizontalDistance >= minJumpDistance;
        bool heightCondition = verticalDistance <= maxJumpHeight;
        bool obstacleCondition = CheckJumpRaycast();

        canJump = distanceCondition && heightCondition && obstacleCondition;
    }

    bool CheckJumpRaycast()
    {
        Vector3 directionToPlayer = (playerTransform.position - transform.position);
        directionToPlayer.y = 0;
        directionToPlayer = directionToPlayer.normalized;

        float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                             new Vector3(playerTransform.position.x, 0, playerTransform.position.z));

        Vector3 rayStart = transform.position + Vector3.up * 1f;

        bool hasObstacle = Physics.SphereCast(rayStart, obstacleCheckRadius, directionToPlayer, out RaycastHit hit, distance - 1f, obstacleLayer);

        return !hasObstacle;
    }

    public void OnDamageReceived()
    {
        hasBeenDamaged = true;
        damageTimer = damageDetectionDuration;
    }

    void CheckGrounded()
    {
        isGrounded = Physics.Raycast(transform.position, Vector3.down, groundCheckDistance, groundLayer);
    }

    void CheckPlayerInRange()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        if (hasBeenDamaged)
        {
            damageTimer -= Time.deltaTime;
            if (damageTimer <= 0f)
            {
                hasBeenDamaged = false;
            }
        }

        playerInRange = (distanceToPlayer <= detectionRange) || hasBeenDamaged;

        if (!playerInRange && agent.hasPath && !isLerpMoving)
        {
            agent.ResetPath();
        }
    }

    void StartChasing()
    {
        if (agent.enabled && agent.isOnNavMesh)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    void StopChasing()
    {
        if (agent.enabled && agent.hasPath)
        {
            agent.ResetPath();
        }
    }

    void HandleChasingMovement()
    {
        if (isLerpMoving) return;

        if (!agent.enabled || !agent.isOnNavMesh) return;

        agent.speed = abilityReceiver.CurrentSpeed;

        if (canJump && currentState != EnemyState.Jumping)
        {
            PerformJump();
            return;
        }

        agent.SetDestination(playerTransform.position);
    }

    void PerformJump()
    {
        if (agent.enabled)
        {
            agent.ResetPath();
        }

        jumpStartPosition = transform.position;
        jumpTargetPosition = FindValidLandingPosition(playerTransform.position);
        jumpProgress = 0f;

        Vector3 directionToTarget = (jumpTargetPosition - transform.position);
        directionToTarget.y = 0;

        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }

        agent.enabled = false;
        currentState = EnemyState.Jumping;
        isLerpMoving = true;
    }

    Vector3 FindValidLandingPosition(Vector3 desiredPosition)
    {
        if (NavMesh.SamplePosition(desiredPosition, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            return hit.position;

        for (float radius = 0.5f; radius <= 5f; radius += 0.5f)
        {
            for (int angle = 0; angle < 360; angle += 45)
            {
                Vector3 offset = new Vector3(
                    Mathf.Sin(angle * Mathf.Deg2Rad) * radius,
                    0,
                    Mathf.Cos(angle * Mathf.Deg2Rad) * radius
                );

                if (NavMesh.SamplePosition(desiredPosition + offset, out hit, 1f, NavMesh.AllAreas))
                    return hit.position;
            }
        }

        return desiredPosition;
    }

    void HandleJumpMovement()
    {
        jumpProgress += Time.deltaTime / lerpDuration;

        if (jumpProgress >= 1f)
        {
            jumpProgress = 1f;
            transform.position = jumpTargetPosition;

            currentState = EnemyState.Landing;
            isLerpMoving = false;
            OnLanding();
        }
        else
        {
            Vector3 currentPos = Vector3.Lerp(jumpStartPosition, jumpTargetPosition, jumpProgress);

            float height = jumpHeight * jumpCurve.Evaluate(jumpProgress) * 4f * jumpProgress * (1f - jumpProgress);
            currentPos.y = Mathf.Lerp(jumpStartPosition.y, jumpTargetPosition.y, jumpProgress) + height;

            transform.position = currentPos;
        }
    }

    Vector3 CalculateTrajectoryPoint(Vector3 start, Vector3 end, float maxHeight, float t)
    {
        Vector3 horizontalPos = Vector3.Lerp(start, end, t);
        float height = Mathf.Lerp(start.y, end.y, t) + (maxHeight * jumpCurve.Evaluate(t) * 4f * t * (1f - t));
        return new Vector3(horizontalPos.x, height, horizontalPos.z);
    }

    void OnLanding()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, 5f, groundLayer))
        {
            transform.position = hit.point;
        }

        if (NavMesh.SamplePosition(transform.position, out NavMeshHit navHit, 10f, NavMesh.AllAreas))
        {
            agent.Warp(navHit.position);
        }

        agent.enabled = true;

        currentState = EnemyState.Chasing;
        if (playerTransform != null)
        {
            agent.SetDestination(playerTransform.position);
        }
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Gizmos.color = colorDetectionRange;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, Vector3.down * groundCheckDistance);

        Vector3 directionToPlayer = (playerTransform.position - transform.position);
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            float distance = Vector3.Distance(new Vector3(transform.position.x, 0, transform.position.z),
                                                 new Vector3(playerTransform.position.x, 0, playerTransform.position.z));

            Gizmos.color = canJump ? Color.green : Color.red;
            Gizmos.DrawRay(transform.position + Vector3.up * 1f, directionToPlayer.normalized * (distance - 1f));

            Vector3 rayStart = transform.position + Vector3.up * 1f;
            Gizmos.DrawWireSphere(rayStart, obstacleCheckRadius);
        }

        if (isLerpMoving && currentState == EnemyState.Jumping)
        {
            Gizmos.color = colorJumpTrajectory;

            for (int i = 0; i <= 20; i++)
            {
                float t = i / 20f;
                Vector3 pos = CalculateTrajectoryPoint(jumpStartPosition, jumpTargetPosition, jumpHeight, t);
                Gizmos.DrawWireSphere(pos, 0.1f);
            }

            Gizmos.DrawWireSphere(jumpTargetPosition, 0.5f);
        }
    }
}