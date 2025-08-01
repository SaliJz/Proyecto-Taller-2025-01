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

    [Header("Trayectoria de Salto Curvo")]
    [SerializeField] private float lerpDuration = 1.5f;
    [SerializeField] private float minJumpHeight = 3f;
    [SerializeField] private float maxJumpHeight = 8f;
    [SerializeField] private float gravityMultiplier = 1f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Detección de Rutas")]
    [SerializeField] private float pathEfficiencyThreshold = 1.8f; 
    [SerializeField] private float gapDetectionDistance = 2f;
    [SerializeField] private float maxPathCalculationDistance = 30f;
    [SerializeField] private float playerMovementThreshold = 3f;

    [Header("Detección de Obstáculos")]
    [SerializeField] private float obstacleCheckDistance = 15f;
    [SerializeField] private int trajectoryPoints = 20;

    [Header("Color")]
    [SerializeField] private Color colorJumpDistance;
    [SerializeField] private Color colorLongJumpDistance;
    [SerializeField] private Color colorIsWaiting;
    [SerializeField] private Color colorIsJumping;
    [SerializeField] private Color colorPathLine;
    [SerializeField] private Color colorJumpTrajectory;

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

    private bool isLerpMoving = false;
    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;
    private float lerpTimer = 0f;
    private float calculatedJumpHeight;

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
            HandleCurvedJumpMovement();
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

        CheckIfJumpIsOptimal(distanceToPlayer);
    }

    void CheckIfJumpIsOptimal(float distanceToPlayer)
    {
        if (!canJump || distanceToPlayer < jumpDistance) return;

        JumpAnalysis analysis = AnalyzeJumpFeasibility();

        if (analysis.shouldJump)
        {
            StartWaitingForJump(analysis.reason);
        }
    }

    struct JumpAnalysis
    {
        public bool shouldJump;
        public string reason;
        public float pathLength;
        public float directDistance;
        public bool hasObstacles;
        public bool isPathBlocked;
    }

    JumpAnalysis AnalyzeJumpFeasibility()
    {
        JumpAnalysis analysis = new JumpAnalysis();
        analysis.shouldJump = false;
        analysis.directDistance = Vector3.Distance(transform.position, playerTransform.position);

        NavMeshPath path = new NavMeshPath();
        bool hasValidPath = agent.CalculatePath(playerTransform.position, path);

        if (!hasValidPath || path.status != NavMeshPathStatus.PathComplete)
        {
            analysis.shouldJump = true;
            analysis.isPathBlocked = true;
            return analysis;
        }

        analysis.pathLength = GetPathLength(path);
        float pathEfficiency = analysis.pathLength / analysis.directDistance;

        if (pathEfficiency > pathEfficiencyThreshold && analysis.directDistance <= obstacleCheckDistance)
        {
            if (IsJumpTrajectoryViable())
            {
                analysis.shouldJump = true;
                return analysis;
            }
        }

        if (HasGapInDirectPath())
        {
            analysis.shouldJump = true;
            return analysis;
        }

        float heightDifference = playerTransform.position.y - transform.position.y;
        if (Mathf.Abs(heightDifference) > jumpHeightThreshold && pathEfficiency > 1.5f)
        {
            if (IsJumpTrajectoryViable())
            {
                analysis.shouldJump = true;
                return analysis;
            }
        }

        return analysis;
    }

    bool IsJumpTrajectoryViable()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = GetBestLandingPosition();

        float distance = Vector3.Distance(startPos, targetPos);
        float heightDiff = targetPos.y - startPos.y;

        float requiredHeight = Mathf.Max(minJumpHeight, distance * 0.3f);
        if (heightDiff > 0) requiredHeight += heightDiff * 0.5f; 

        calculatedJumpHeight = Mathf.Min(requiredHeight, maxJumpHeight);

        return !HasObstaclesInJumpTrajectory(startPos, targetPos, calculatedJumpHeight);
    }

    bool HasObstaclesInJumpTrajectory(Vector3 start, Vector3 end, float maxHeight)
    {
        for (int i = 0; i <= trajectoryPoints; i++)
        {
            float t = (float)i / trajectoryPoints;
            Vector3 point = CalculateTrajectoryPoint(start, end, maxHeight, t);

            if (Physics.CheckSphere(point, 0.5f, obstacleLayer))
            {
                return true;
            }
        }
        return false;
    }

    Vector3 CalculateTrajectoryPoint(Vector3 start, Vector3 end, float maxHeight, float t)
    {
        Vector3 horizontalPos = Vector3.Lerp(start, end, t);

        float heightMultiplier = jumpCurve.Evaluate(t);
        float parabolicHeight = 4f * t * (1f - t); 

        float currentHeight = Mathf.Lerp(start.y, end.y, t) + (maxHeight * parabolicHeight * heightMultiplier);

        return new Vector3(horizontalPos.x, currentHeight, horizontalPos.z);
    }

    bool HasGapInDirectPath()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        int checkPoints = Mathf.RoundToInt(distance / gapDetectionDistance);
        checkPoints = Mathf.Max(3, Mathf.Min(checkPoints, 10));

        for (int i = 1; i < checkPoints; i++)
        {
            float t = (float)i / checkPoints;
            Vector3 checkPos = Vector3.Lerp(transform.position, playerTransform.position, t);

            if (!Physics.Raycast(checkPos + Vector3.up * 0.5f, Vector3.down, 4f, groundLayer))
            {
                return true;
            }
        }
        return false;
    }

    float GetPathLength(NavMeshPath path)
    {
        if (path.corners.Length < 2) return 0f;

        float length = 0f;
        for (int i = 1; i < path.corners.Length; i++)
        {
            length += Vector3.Distance(path.corners[i - 1], path.corners[i]);
        }
        return length;
    }

    void StartWaitingForJump(string reason = "")
    {
        isWaiting = true;
        waitTimer = 0f;
        agent.ResetPath();
        lastPlayerPosition = playerTransform.position;
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
        if (currentDistance <= stopDistance)
        {
            CancelJump();
            return;
        }

        if (waitTimer >= waitTimeBeforeJump)
        {
            PerformCurvedJump();
        }
    }

    void PerformCurvedJump()
    {
        isWaiting = false;
        isJumping = true;
        canJump = false;

        lerpStartPosition = transform.position;
        lerpTargetPosition = GetBestLandingPosition();
        lerpTimer = 0f;
        isLerpMoving = true;

        float distance = Vector3.Distance(lerpStartPosition, lerpTargetPosition);
        float heightDiff = lerpTargetPosition.y - lerpStartPosition.y;

        calculatedJumpHeight = Mathf.Max(minJumpHeight, distance * 0.25f);
        if (heightDiff > 0) calculatedJumpHeight += heightDiff * 0.7f;
        if (heightDiff < -2f) calculatedJumpHeight += Mathf.Abs(heightDiff) * 0.3f; 

        calculatedJumpHeight = Mathf.Min(calculatedJumpHeight, maxJumpHeight);

        agent.enabled = false;

        lerpDuration = Mathf.Clamp(distance / 10f, 0.8f, 2.5f);

        Invoke(nameof(ResetJumpCooldown), 4f);
    }

    void HandleCurvedJumpMovement()
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

        Vector3 newPosition = CalculateTrajectoryPoint(lerpStartPosition, lerpTargetPosition, calculatedJumpHeight, progress);
        transform.position = newPosition;

        Vector3 lookDirection = (lerpTargetPosition - transform.position).normalized;
        if (lookDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }

    Vector3 GetBestLandingPosition()
    {
        Vector3 playerPos = playerTransform.position;
        NavMeshHit hit;

        Vector3[] offsets = {
            Vector3.zero,
            Vector3.forward * 1.5f,
            Vector3.back * 1.5f,
            Vector3.left * 1.5f,
            Vector3.right * 1.5f,
            Vector3.forward * 1f + Vector3.right * 1f,
            Vector3.forward * 1f + Vector3.left * 1f,
            Vector3.back * 1f + Vector3.right * 1f,
            Vector3.back * 1f + Vector3.left * 1f
        };

        foreach (Vector3 offset in offsets)
        {
            Vector3 testPos = playerPos + offset;
            if (NavMesh.SamplePosition(testPos, out hit, 3f, NavMesh.AllAreas))
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

        if (isLerpMoving || (isWaiting && Application.isPlaying))
        {
            Vector3 start = isLerpMoving ? lerpStartPosition : transform.position;
            Vector3 end = isLerpMoving ? lerpTargetPosition : GetBestLandingPosition();
            float height = calculatedJumpHeight > 0 ? calculatedJumpHeight : minJumpHeight;

            Gizmos.color = colorJumpTrajectory;
            Vector3 lastPoint = start;

            for (int i = 1; i <= trajectoryPoints; i++)
            {
                float t = (float)i / trajectoryPoints;
                Vector3 point = CalculateTrajectoryPoint(start, end, height, t);
                Gizmos.DrawLine(lastPoint, point);
                lastPoint = point;
            }

            Gizmos.DrawWireSphere(end, 0.5f);
        }

        if (Application.isPlaying)
        {
            Vector3 direction = (playerTransform.position - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, playerTransform.position);
            int checkPoints = Mathf.RoundToInt(distance / gapDetectionDistance);
            checkPoints = Mathf.Max(3, Mathf.Min(checkPoints, 10));

            for (int i = 1; i < checkPoints; i++)
            {
                float t = (float)i / checkPoints;
                Vector3 checkPos = Vector3.Lerp(transform.position, playerTransform.position, t);

                bool hasGround = Physics.Raycast(checkPos + Vector3.up * 0.5f, Vector3.down, 4f, groundLayer);
                Gizmos.color = hasGround ? Color.green : Color.red;
                Gizmos.DrawRay(checkPos + Vector3.up * 0.5f, Vector3.down * 4f);
            }
        }

        if (Application.isPlaying && agent != null)
        {
            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(playerTransform.position, path) && path.corners.Length > 1)
            {
                Gizmos.color = Color.blue;
                for (int i = 1; i < path.corners.Length; i++)
                {
                    Gizmos.DrawLine(path.corners[i - 1], path.corners[i]);
                }
            }
        }
    }
}