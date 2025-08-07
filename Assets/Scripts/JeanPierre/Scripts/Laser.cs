using UnityEngine;

public class Laser : MonoBehaviour
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

    [Header("Debug Visual")]
    [SerializeField] private bool showRaycast = true;
    [SerializeField] private Color noCollisionColor = Color.green;
    [SerializeField] private Color collisionColor = Color.red;

    private Vector3 targetScale;
    private RaycastHit hitInfo;
    private float currentDistance;

    void Start()
    {
        if (scalableObject == null)
        {
            scalableObject = this.gameObject;
        }

        if (originPoint == null)
        {
            originPoint = this.transform;
        }

        targetScale = baseScale;
        scalableObject.transform.localScale = baseScale;
    }

    void Update()
    {
        ShootRaycast();
        ScaleObject();
        ShowDebugVisual();
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

    public void SetScaleMultiplier(float multiplier)
    {
        scaleMultiplier = multiplier;
    }

    public void SetLocalDirection(Vector3 newDirection)
    {
        localDirection = newDirection.normalized;
    }

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
}