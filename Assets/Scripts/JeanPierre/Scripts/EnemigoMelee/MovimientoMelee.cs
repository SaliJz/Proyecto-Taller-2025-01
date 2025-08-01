using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MovimientoMelee : MonoBehaviour
{
    [Header("Referencia al Jugador")]
    [SerializeField] private Transform playerTransform;

    [Header("Parámetros de Movimiento")]
    [SerializeField] private float startVelocity = 5f;
    [SerializeField] private float stopDistance = 1.5f;
    [SerializeField] private float farDistanceThreshold = 20f;

    [Header("Parámetros de Salto")]
    [SerializeField] private float jumpDistance = 8f;
    [SerializeField] private float longJumpDistance = 15f;
    [SerializeField] private float requiredHeightDifference = 5f;
    [SerializeField] private LayerMask groundLayer = -1; 
    [SerializeField] private CapsuleCollider targetCollider;
    [SerializeField] private bool rotateDuringJump = true;

    [Header("Trayectoria de Salto")]
    [SerializeField] private float lerpDuration = 1.5f;
    [SerializeField] private float minJumpHeight = 3f;
    [SerializeField] private float maxJumpHeight = 8f;
    [SerializeField] private AnimationCurve jumpCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float jumpPreparationTime = 1f;

    [Header("Detección de Rutas")]
    [SerializeField] private float playerMovementThreshold = 3f;
    [SerializeField] private float maxPathRatioForJump = 3f;
    [SerializeField] private float minDirectDistanceForJump = 5f;

    [Header("Detección de Obstáculos")]
    [SerializeField] private int trajectoryPoints = 20;

    [Header("Configuración de Raycast")]
    [SerializeField] private int raycastsCount = 5;
    [SerializeField] private float raycastSpacing = 0.5f;
    [SerializeField] private float raycastLength = 10f;
    [SerializeField] private bool showDebugRaycasts = true;

    [Header("Detección durante Salto")]
    [SerializeField] private float detectionRadiusDuringJump = 5f;
    [SerializeField] private LayerMask playerDetectionMask;

    [Header("Comportamiento de Salto")]
    [SerializeField] private float minJumpCooldown = 1f;
    [SerializeField] private float maxJumpCooldown = 3f;

    [Header("Visualización")]
    [SerializeField] private Color colorJumpDistance;
    [SerializeField] private Color colorLongJumpDistance;
    [SerializeField] private Color colorIsJumping;
    [SerializeField] private Color colorPathLine;
    [SerializeField] private Color colorJumpTrajectory;

    private NavMeshAgent agent;
    private EnemyAbilityReceiver abilityReceiver;
    private float currentVelocity;
    private bool isJumping = false;
    private bool canJump = true;
    private bool isLerpMoving = false;
    private Vector3 lerpStartPosition;
    private Vector3 lerpTargetPosition;
    private float lerpTimer = 0f;
    private float calculatedJumpHeight;
    private float jumpCooldownTimer = 0f;
    private float jumpPreparationTimer = 0f;
    private bool preparingToJump = false;
    private Vector3 lastKnownPlayerPosition;
    private Vector3 jumpTargetPosition;
    private Vector3[] debugRaycastPoints;

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
        }

        if (playerTransform == null)
        {
            playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        }

        lastKnownPlayerPosition = playerTransform.position;
        debugRaycastPoints = new Vector3[raycastsCount];
    }

    void Update()
    {
        if (abilityReceiver == null || abilityReceiver.CurrentTarget == null) return;

        UpdatePlayerPositionTracking();
        currentVelocity = abilityReceiver.CurrentSpeed;

        if (isLerpMoving)
        {
            HandleCurvedJumpMovement();
            return;
        }

        if (preparingToJump)
        {
            jumpPreparationTimer -= Time.deltaTime;
            if (jumpPreparationTimer <= 0f || !ShouldJumpToTarget(jumpTargetPosition))
            {
                preparingToJump = false;
                if (jumpPreparationTimer <= 0f && ShouldJumpToTarget(jumpTargetPosition))
                {
                    PrepareJump(jumpTargetPosition);
                }
                else
                {
                    ResumeNavigation();
                }
            }
            return;
        }

        if (!canJump)
        {
            jumpCooldownTimer -= Time.deltaTime;
            if (jumpCooldownTimer <= 0f) canJump = true;
        }

        HandleMainMovement();
    }

    bool ShouldJumpToTarget(Vector3 target)
    {
        float distance = Vector3.Distance(transform.position, target);
        if (distance > farDistanceThreshold) return true;

        float heightDiff = Mathf.Abs(target.y - transform.position.y);
        bool heightCondition = heightDiff >= requiredHeightDifference;
        bool pathCondition = HasSignificantGapInDirectPath() || !HasDirectPathToTarget(target);
        return heightCondition || pathCondition;
    }

    bool HasDirectPathToTarget(Vector3 target)
    {
        NavMeshPath path = new NavMeshPath();
        if (agent.CalculatePath(target, path))
        {
            return path.status == NavMeshPathStatus.PathComplete;
        }
        return false;
    }

    void ResumeNavigation()
    {
        canJump = true;
        agent.enabled = true;
        agent.SetDestination(abilityReceiver.CurrentTarget.position);
    }

    void UpdatePlayerPositionTracking()
    {
        if (isLerpMoving)
        {
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRadiusDuringJump, playerDetectionMask);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.CompareTag("Player"))
                {
                    lastKnownPlayerPosition = playerTransform.position;
                    return;
                }
            }
        }
        else
        {
            float distanceMoved = Vector3.Distance(lastKnownPlayerPosition, playerTransform.position);
            if (distanceMoved > playerMovementThreshold)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(playerTransform.position, out hit, 5f, NavMesh.AllAreas))
                {
                    lastKnownPlayerPosition = hit.position;
                }
            }
        }
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

        if (canJump && distanceToPlayer > jumpDistance)
        {
            EvaluateJumpConditions();
        }
    }

    void EvaluateJumpConditions()
    {
        JumpAnalysis analysis = AnalyzeJumpFeasibility();

        if (analysis.shouldJump)
        {
            jumpTargetPosition = analysis.isPathBlocked ? GetOptimalJumpPosition() : playerTransform.position;
            preparingToJump = true;
            jumpPreparationTimer = jumpPreparationTime;
            canJump = false;
        }
    }

    struct JumpAnalysis
    {
        public bool shouldJump;
        public bool isPathBlocked;
        public float pathLength;
        public float directDistance;
    }

    JumpAnalysis AnalyzeJumpFeasibility()
    {
        JumpAnalysis analysis = new JumpAnalysis();
        analysis.shouldJump = false;
        analysis.directDistance = Vector3.Distance(transform.position, playerTransform.position);

        if (!agent.enabled || !agent.isOnNavMesh || analysis.directDistance < minDirectDistanceForJump)
            return analysis;

        if (analysis.directDistance > farDistanceThreshold)
        {
            analysis.shouldJump = true;
            return analysis;
        }

        NavMeshPath path = new NavMeshPath();
        bool hasValidPath = agent.CalculatePath(playerTransform.position, path);
        analysis.pathLength = GetPathLength(path);

        if (!hasValidPath || path.status != NavMeshPathStatus.PathComplete)
        {
            analysis.shouldJump = true;
            analysis.isPathBlocked = true;
            return analysis;
        }

        float pathRatio = analysis.pathLength / analysis.directDistance;
        if (pathRatio > maxPathRatioForJump || HasSignificantGapInDirectPath())
        {
            analysis.shouldJump = true;
            return analysis;
        }

        float heightDifference = Mathf.Abs(playerTransform.position.y - transform.position.y);
        if (heightDifference >= requiredHeightDifference)
        {
            analysis.shouldJump = true;
            return analysis;
        }

        return analysis;
    }

    Vector3 GetOptimalJumpPosition()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        float distance = Vector3.Distance(transform.position, playerTransform.position);

        for (float dist = Mathf.Min(distance, longJumpDistance); dist > 3f; dist -= 2f)
        {
            Vector3 testPos = transform.position + direction * dist;
            NavMeshHit hit;
            if (NavMesh.SamplePosition(testPos, out hit, 2f, NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        return lastKnownPlayerPosition;
    }

    bool HasSignificantGapInDirectPath()
    {
        Vector3 direction = (playerTransform.position - transform.position).normalized;
        Vector3 right = Vector3.Cross(direction, Vector3.up).normalized * raycastSpacing;

        int gapsDetected = 0;
        for (int i = 0; i < raycastsCount; i++)
        {
            Vector3 rayOrigin = transform.position + right * (i - (raycastsCount / 2));
            rayOrigin.y += 0.5f;
            debugRaycastPoints[i] = rayOrigin;

            if (!Physics.Raycast(rayOrigin, Vector3.down, raycastLength, groundLayer))
            {
                gapsDetected++;
            }
        }

        return gapsDetected >= (raycastsCount / 2) + 1;
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

    void PrepareJump(Vector3 targetPosition)
    {
        isJumping = true;
        canJump = false;
        jumpCooldownTimer = Random.Range(minJumpCooldown, maxJumpCooldown);

        lerpStartPosition = transform.position;
        lerpTargetPosition = FindValidLandingPosition(targetPosition);
        lerpTimer = 0f;
        isLerpMoving = true;

        float distance = Vector3.Distance(lerpStartPosition, lerpTargetPosition);
        calculatedJumpHeight = Mathf.Max(minJumpHeight, distance * 0.25f);
        calculatedJumpHeight = Mathf.Min(calculatedJumpHeight, maxJumpHeight);

        agent.enabled = false;
        lerpDuration = Mathf.Clamp(distance / 12f, 0.8f, 2f);
    }

    Vector3 FindValidLandingPosition(Vector3 desiredPosition)
    {
        NavMeshHit hit;
        if (NavMesh.SamplePosition(desiredPosition, out hit, 3f, NavMesh.AllAreas))
        {
            return hit.position;
        }

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
                {
                    return hit.position;
                }
            }
        }

        return transform.position;
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

            if (!agent.enabled) agent.enabled = true;
            if (!agent.isOnNavMesh)
            {
                NavMeshHit hit;
                if (NavMesh.SamplePosition(transform.position, out hit, 5f, NavMesh.AllAreas))
                    agent.Warp(hit.position);
            }

            if (agent.enabled && agent.isOnNavMesh)
                agent.SetDestination(abilityReceiver.CurrentTarget.position);

            return;
        }

        transform.position = CalculateTrajectoryPoint(lerpStartPosition, lerpTargetPosition, calculatedJumpHeight, progress);

        if (rotateDuringJump)
        {
            Vector3 lookDirection = (lerpTargetPosition - transform.position).normalized;
            if (lookDirection != Vector3.zero)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection), Time.deltaTime * 2f);
        }
    }

    Vector3 CalculateTrajectoryPoint(Vector3 start, Vector3 end, float maxHeight, float t)
    {
        Vector3 horizontalPos = Vector3.Lerp(start, end, t);
        float height = Mathf.Lerp(start.y, end.y, t) + (maxHeight * jumpCurve.Evaluate(t) * 4f * t * (1f - t));
        return new Vector3(horizontalPos.x, height, horizontalPos.z);
    }

    void OnDrawGizmos()
    {
        if (playerTransform == null) return;

        Gizmos.color = colorJumpDistance;
        Gizmos.DrawWireSphere(transform.position, jumpDistance);

        Gizmos.color = colorLongJumpDistance;
        Gizmos.DrawWireSphere(transform.position, longJumpDistance);

        if (isJumping || isLerpMoving)
        {
            Gizmos.color = colorIsJumping;
            Gizmos.DrawWireCube(transform.position + Vector3.up * 2f, Vector3.one * 0.8f);
        }

        Gizmos.color = colorPathLine;
        Gizmos.DrawLine(transform.position, playerTransform.position);

        if (isLerpMoving)
        {
            Gizmos.color = colorJumpTrajectory;
            Vector3 lastPoint = lerpStartPosition;
            for (int i = 1; i <= trajectoryPoints; i++)
            {
                float t = (float)i / trajectoryPoints;
                Vector3 point = CalculateTrajectoryPoint(lerpStartPosition, lerpTargetPosition, calculatedJumpHeight, t);
                Gizmos.DrawLine(lastPoint, point);
                lastPoint = point;
            }
            Gizmos.DrawWireSphere(lerpTargetPosition, 0.5f);
        }

        if (showDebugRaycasts && debugRaycastPoints != null)
        {
            Gizmos.color = Color.cyan;
            foreach (Vector3 point in debugRaycastPoints)
            {
                if (point != Vector3.zero)
                {
                    Gizmos.DrawLine(point, point + Vector3.down * raycastLength);
                }
            }
        }
    }
}