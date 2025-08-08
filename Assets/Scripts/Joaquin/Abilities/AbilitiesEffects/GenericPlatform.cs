using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericPlatform : MonoBehaviour, ISlowable, IHackable
{
    private enum PlatformState
    {
        Idle,
        Moving,
        Paused,
        Disabled
    }

    [Header("Movement")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float moveSpeed = 3f;
    [Tooltip("Mueve la plataforma solo en ejes cardinales (X, Y, Z).")]
    [SerializeField] private bool useManhattanMovement = false;

    [Header("Behavior")]
    [Tooltip("La plataforma se mueve continuamente entre los puntos.")]
    [SerializeField] private bool isAutomatic = true;
    [Tooltip("Tiempo de espera en cada punto antes de regresar.")]
    [SerializeField] private float waitTimeAtPoints = 2f;

    [Header("Safety & Collision")]
    [Tooltip("Distancia adelante de la plataforma para detectar obstáculos.")]
    [SerializeField] private float safetyStopDistance = 0.5f;
    [SerializeField] private LayerMask obstacleLayers;
    private BoxCollider platformCollider;

    [Header("Hacking & Emissive")]
    [SerializeField] private bool isHackable = true;
    [SerializeField] private Renderer[] emissiveRenderers;
    [ColorUsage(true, true)][SerializeField] private Color colorActive = Color.green;
    [ColorUsage(true, true)][SerializeField] private Color colorHackable = Color.red;
    [ColorUsage(true, true)][SerializeField] private Color colorLocked = Color.gray;

    private PlatformState currentState = PlatformState.Idle;
    private Transform currentTarget;
    private float originalSpeed;
    private Coroutine movementCoroutine;
    private List<Material> dynamicEmissiveMaterials = new List<Material>();

    private void Awake()
    {
        platformCollider = GetComponent<BoxCollider>();
        if (platformCollider == null) Debug.LogError("GenericPlatform requiere un BoxCollider.");

        originalSpeed = moveSpeed;

        foreach (var rend in emissiveRenderers)
        {
            if (rend) dynamicEmissiveMaterials.Add(rend.material);
        }
    }

    private void Start()
    {
        transform.position = startPoint.position;
        currentTarget = endPoint;

        if (isAutomatic)
        {
            UpdateState(PlatformState.Moving);
        }
        else
        {
            UpdateState(PlatformState.Idle);
        }
    }

    private void StartMovement()
    {
        if (movementCoroutine != null) StopCoroutine(movementCoroutine);

        if (useManhattanMovement)
        {
            movementCoroutine = StartCoroutine(ManhattanMoveRoutine());
        }
        else
        {
            movementCoroutine = StartCoroutine(DirectMoveRoutine());
        }
    }

    private IEnumerator DirectMoveRoutine()
    {
        while (Vector3.Distance(transform.position, currentTarget.position) > 0.01f)
        {
            if (CheckForObstacles())
            {
                UpdateState(PlatformState.Paused);
                yield return new WaitUntil(() => !CheckForObstacles()); // Espera a que el camino esté libre.
                UpdateState(PlatformState.Moving);
            }

            transform.position = Vector3.MoveTowards(transform.position, currentTarget.position, moveSpeed * Time.deltaTime);
            yield return null;
        }

        OnTargetReached();
    }

    // Movimiento en estilo Manhattan: 6 direcciones posibles: ±X, ±Y, ±Z
    private IEnumerator ManhattanMoveRoutine()
    {
        Vector3 targetPos = currentTarget.position;

        // Mover en X
        while (Mathf.Abs(transform.position.x - targetPos.x) > 0.01f)
        {
            if (CheckForObstacles()) { UpdateState(PlatformState.Paused); yield return new WaitUntil(() => !CheckForObstacles()); UpdateState(PlatformState.Moving); }
            Vector3 stepTarget = new Vector3(targetPos.x, transform.position.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, stepTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
        // Mover en Y
        while (Mathf.Abs(transform.position.y - targetPos.y) > 0.01f)
        {
            if (CheckForObstacles()) { UpdateState(PlatformState.Paused); yield return new WaitUntil(() => !CheckForObstacles()); UpdateState(PlatformState.Moving); }
            Vector3 stepTarget = new Vector3(transform.position.x, targetPos.y, transform.position.z);
            transform.position = Vector3.MoveTowards(transform.position, stepTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }
        // Mover en Z
        while (Mathf.Abs(transform.position.z - targetPos.z) > 0.01f)
        {
            if (CheckForObstacles()) { UpdateState(PlatformState.Paused); yield return new WaitUntil(() => !CheckForObstacles()); UpdateState(PlatformState.Moving); }
            Vector3 stepTarget = new Vector3(transform.position.x, transform.position.y, targetPos.z);
            transform.position = Vector3.MoveTowards(transform.position, stepTarget, moveSpeed * Time.deltaTime);
            yield return null;
        }

        OnTargetReached();
    }

    private void OnTargetReached()
    {
        transform.position = currentTarget.position;
        currentTarget = (currentTarget == startPoint) ? endPoint : startPoint;

        if (isAutomatic)
        {
            UpdateState(PlatformState.Idle);
            Invoke(nameof(StartMovement), waitTimeAtPoints);
        }
        else
        {
            UpdateState(PlatformState.Idle);
        }
    }

    private bool CheckForObstacles()
    {
        Vector3 direction = (currentTarget.position - transform.position).normalized;
        if (direction == Vector3.zero) return false;

        // Lanza una caja (BoxCast) del tamaño de la plataforma para detectar colisiones.
        return Physics.BoxCast(
            transform.position,
            platformCollider.size / 2,
            direction,
            Quaternion.identity,
            safetyStopDistance,
            obstacleLayers
        );
    }

    private void UpdateState(PlatformState newState)
    {
        if (currentState == newState) return;
        currentState = newState;
        UpdateEmissiveColor();

        if (currentState == PlatformState.Moving)
        {
            StartMovement();
        }
        else if (movementCoroutine != null)
        {
            StopCoroutine(movementCoroutine);
            movementCoroutine = null;
        }
    }

    private void UpdateEmissiveColor()
    {
        Color targetColor = colorLocked;

        if (isAutomatic)
        {
            targetColor = colorActive;
        }
        else
        {
            if (currentState == PlatformState.Idle || currentState == PlatformState.Disabled)
            {
                targetColor = isHackable ? colorHackable : colorLocked;
            }
            else
            {
                targetColor = colorActive;
            }
        }

        foreach (var mat in dynamicEmissiveMaterials)
        {
            mat.SetColor("_EmissionColor", targetColor);
        }
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        StartCoroutine(SlowRoutine(slowMultiplier, duration));
    }

    private IEnumerator SlowRoutine(float multiplier, float duration)
    {
        moveSpeed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        moveSpeed = originalSpeed;
    }

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (isHackable && !isAutomatic)
        {
            if (currentState == PlatformState.Idle)
            {
                Debug.Log("Plataforma hackeada, iniciando movimiento.");
                UpdateState(PlatformState.Moving);
            }
            else
            {
                Debug.Log("Plataforma hackeada, deteniendo movimiento.");
                UpdateState(PlatformState.Idle);
            }
        }
    }

    // Para visualizar el BoxCast de seguridad en el editor.
    private void OnDrawGizmos()
    {
        if (platformCollider != null)
        {
            Gizmos.color = Color.red;
            Vector3 direction = (currentTarget != null) ? (currentTarget.position - transform.position).normalized : transform.forward;
            Gizmos.DrawWireCube(transform.position + direction * safetyStopDistance, platformCollider.size);
        }
    }
}