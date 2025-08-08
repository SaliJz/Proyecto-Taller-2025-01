using UnityEngine;
using System.Collections;

public class Laser : MonoBehaviour, IHackable, ISlowable
{
    [Header("References")]
    [SerializeField] private GameObject scalableObject;
    [SerializeField] private LayerMask collisionLayers = -1;

    [Header("Scaling Settings")]
    [SerializeField] private Vector3 baseScale = Vector3.one;
    [SerializeField] private float scaleMultiplier = 0.01f;
    [SerializeField] private float transitionSpeed = 5f;

    [Header("Raycast Settings")]
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private Transform originPoint;
    [SerializeField] private Vector3 localDirection = Vector3.forward;

    [Header("Movement Settings")]
    [SerializeField] private Transform objectToMove;
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float arrivalThreshold = 0.1f;

    [Header("Damage Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float damageInterval = 0.5f;

    [Header("Debug Visual")]
    [SerializeField] private bool showRaycast = true;
    [SerializeField] private Color noCollisionColor = Color.green;
    [SerializeField] private Color collisionColor = Color.red;

    private Vector3 targetScale;
    private RaycastHit hitInfo;
    private float currentDistance;

    private int currentWaypointIndex = 0;
    private bool isMoving = false;

    private float lastDamageTime = 0f;

    private float originalSpeed;
    private bool isHacked = false;

    void Start()
    {
        originalSpeed = moveSpeed;

        if (scalableObject == null)
            scalableObject = this.gameObject;

        if (originPoint == null)
            originPoint = this.transform;

        if (objectToMove == null)
            objectToMove = this.transform;

        targetScale = baseScale;
        scalableObject.transform.localScale = baseScale;

        InitializeMovement();
    }

    void Update()
    {
        HandleMovement();
        ShootRaycast();
        ScaleObject();
        ShowDebugVisual();
    }

    void InitializeMovement()
    {
        if (waypoints == null || waypoints.Length == 0 || objectToMove == null)
        {
            return;
        }

        if (waypoints.Length == 1)
        {
            isMoving = true;
        }
        else if (waypoints.Length >= 2)
        {
            isMoving = true;
            currentWaypointIndex = 0;
        }
    }

    void HandleMovement()
    {
        if (waypoints == null || waypoints.Length == 0 || !isMoving || objectToMove == null)
            return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
            return;

        Vector3 targetPosition = targetWaypoint.position;
        objectToMove.position = Vector3.MoveTowards(objectToMove.position, targetPosition, moveSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(objectToMove.position, targetPosition);

        if (distanceToTarget <= arrivalThreshold)
        {
            if (waypoints.Length == 1)
            {
                isMoving = false;
            }
            else if (waypoints.Length >= 2)
            {
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }

    void ShootRaycast()
    {
        Vector3 worldDirection = originPoint.TransformDirection(localDirection.normalized);
        Vector3 startPosition = originPoint.position;

        if (Physics.Raycast(startPosition, worldDirection, out hitInfo, maxDistance, collisionLayers))
        {
            currentDistance = hitInfo.distance;
            OnLaserHit(hitInfo);
        }
        else
        {
            currentDistance = maxDistance;
            OnLaserMiss();
        }

        CalculateScaleFromDistance();
    }

    void CalculateScaleFromDistance()
    {
        float scaleX = currentDistance * scaleMultiplier;
        targetScale = new Vector3(scaleX, baseScale.y, baseScale.z);
    }

    void ScaleObject()
    {
        if (Vector3.Distance(scalableObject.transform.localScale, targetScale) > 0.01f)
        {
            scalableObject.transform.localScale = Vector3.Lerp(
                scalableObject.transform.localScale,
                targetScale,
                transitionSpeed * Time.deltaTime
            );
        }
    }

    void ShowDebugVisual()
    {
        if (!showRaycast) return;

        Vector3 worldDirection = originPoint.TransformDirection(localDirection.normalized);
        bool hasHit = Physics.Raycast(originPoint.position, worldDirection, maxDistance, collisionLayers);
        Color rayColor = hasHit ? collisionColor : noCollisionColor;

        Debug.DrawRay(originPoint.position, worldDirection * currentDistance, rayColor);
    }

    void OnLaserHit(RaycastHit hit)
    {
        //Debug.Log($"Láser impactó: {hit.collider.name} a distancia: {hit.distance:F2} - Escala X: {targetScale.x:F2}");
    }

    void OnLaserMiss()
    {
        //Debug.Log($"Láser libre - Distancia máxima: {currentDistance:F2} - Escala X: {targetScale.x:F2}");
    }

    public void SetScaleMultiplier(float multiplier) => scaleMultiplier = multiplier;
    public void SetLocalDirection(Vector3 newDirection) => localDirection = newDirection.normalized;

    public float GetCurrentDistance()
    {
        return currentDistance;
    }

    public GameObject GetHitObject()
    {
        Vector3 worldDirection = originPoint.TransformDirection(localDirection.normalized);
        return Physics.Raycast(originPoint.position, worldDirection, out RaycastHit hit, maxDistance, collisionLayers)
               ? hit.collider.gameObject : null;
    }

    public Vector3 GetHitPoint()
    {
        Vector3 worldDirection = originPoint.TransformDirection(localDirection.normalized);
        return Physics.Raycast(originPoint.position, worldDirection, out RaycastHit hit, maxDistance, collisionLayers)
               ? hit.point : originPoint.position + worldDirection * maxDistance;
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
        InitializeMovement();
    }

    public void StartMovement()
    {
        if (waypoints != null && waypoints.Length > 0 && objectToMove != null)
        {
            isMoving = true;
        }
    }

    public void StopMovement() => isMoving = false;

    public bool IsMoving()
    {
        return isMoving;
    }

    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }

    public void SetObjectToMove(Transform newObjectToMove) => objectToMove = newObjectToMove;

    public Transform GetObjectToMove()
    {
        return objectToMove;
    }

    private void OnTriggerStay(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        if (Time.time >= lastDamageTime + damageInterval)
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage, transform.position);
                lastDamageTime = Time.time;
            }
        }
    }

    public void SetDamage(int newDamage) => damage = newDamage;
    public void SetDamageInterval(float newInterval) => damageInterval = newInterval;
    public float GetDamage() => damage;
    public float GetDamageInterval() => damageInterval;

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        isHacked = !isHacked;
        moveSpeed = isHacked ? 0 : originalSpeed;
        Debug.Log("El estado de la plataforma ha cambiado. Hackeada: " + isHacked);
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (isHacked) return;
        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        moveSpeed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }
}