using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericDoor : MonoBehaviour, ISlowable, IHackable
{
    private enum DoorState
    {
        Closed, 
        Opening, 
        Open, 
        Closing,
        Locked
    }

    [Header("Door Configuration")]
    [SerializeField] private bool isDoubleDoor = true;
    [Tooltip("Si es puerta doble, ¿cuál se abre?")]
    [SerializeField] private DoorSelection doorToOpen = DoorSelection.Both;
    [SerializeField] private Transform leftDoorPanel;
    [SerializeField] private Transform rightDoorPanel;

    [Header("Movement Settings")]
    [SerializeField] private Transform leftDoorOpenTarget;
    [SerializeField] private Transform rightDoorOpenTarget;
    [SerializeField] private float openCloseSpeed = 5f;

    [Header("Behavior")]
    [Tooltip("Si está activo, la puerta comenzará en el estado 'Abierto'.")]
    [SerializeField] private bool startOpen = false;
    [Tooltip("Se abre y cierra automáticamente al detectar al jugador.")]
    [SerializeField] private bool isAutomatic = true;
    [Tooltip("Distancia a la que la puerta automática reacciona.")]
    [SerializeField] private float detectionRadius = 8f;
    [Tooltip("Collider que, al ser cruzado, bloquea la puerta si no es automática.")]
    [SerializeField] private Collider oneWayPassTrigger;

    [Header("Hacking & Emissive")]
    [SerializeField] private bool isHackable = true;
    [SerializeField] private Renderer[] emissiveRenderers;
    [ColorUsage(true, true)][SerializeField] private Color colorActive = Color.green;
    [ColorUsage(true, true)][SerializeField] private Color colorHackable = Color.red;
    [ColorUsage(true, true)][SerializeField] private Color colorLocked = Color.gray;

    [Tooltip("Si la puerta está permanentemente bloqueada y no puede ser abierta.")]
    [SerializeField] private bool isPermanentlyLocked = false;

    private DoorState currentState = DoorState.Closed;
    private Vector3 leftDoorInitialPos;
    private Vector3 rightDoorInitialPos;
    private Transform playerTransform;
    private float originalSpeed;
    private List<Material> dynamicEmissiveMaterials = new List<Material>();

    public enum DoorSelection { Left, Right, Both }

    private void Awake()
    {
        if (leftDoorPanel) leftDoorInitialPos = leftDoorPanel.position;
        if (rightDoorPanel) rightDoorInitialPos = rightDoorPanel.position;
        originalSpeed = openCloseSpeed;

        playerTransform = GameObject.FindGameObjectWithTag("Player").transform;

        foreach (var rend in emissiveRenderers)
        {
            if (rend) dynamicEmissiveMaterials.Add(rend.material);
        }
    }

    private void Start()
    {
        if (oneWayPassTrigger != null)
        {
            oneWayPassTrigger.isTrigger = true;
        }

        if (startOpen)
        {
            if (leftDoorPanel) leftDoorPanel.position = GetTargetPosition(leftDoorPanel, leftDoorOpenTarget);
            if (rightDoorPanel) rightDoorPanel.position = GetTargetPosition(rightDoorPanel, rightDoorOpenTarget);

            currentState = DoorState.Open;
        }
        else
        {
            currentState = DoorState.Closed;
        }

        UpdateState(DoorState.Closed);
    }

    private void Update()
    {
        HandleDoorState();
    }

    private void HandleDoorState()
    {
        if (currentState == DoorState.Locked) return;

        if (isAutomatic)
        {
            HandleAutomaticBehavior();
        }

        switch (currentState)
        {
            case DoorState.Opening:
                MoveDoors(GetTargetPosition(leftDoorPanel, leftDoorOpenTarget), GetTargetPosition(rightDoorPanel, rightDoorOpenTarget));
                if (IsAtDestination(GetTargetPosition(leftDoorPanel, leftDoorOpenTarget)))
                {
                    UpdateState(DoorState.Open);
                }
                break;
            case DoorState.Closing:
                MoveDoors(leftDoorInitialPos, rightDoorInitialPos);
                if (IsAtDestination(leftDoorInitialPos))
                {
                    if (isPermanentlyLocked)
                    {
                        isPermanentlyLocked = false;
                        UpdateState(DoorState.Locked);
                    }
                    else
                    {
                        UpdateState(DoorState.Closed);
                    }
                }
                break;
        }
    }

    private void HandleAutomaticBehavior()
    {
        if (Vector3.Distance(playerTransform.position, transform.position) < detectionRadius)
        {
            if (currentState == DoorState.Closed || currentState == DoorState.Closing)
            {
                UpdateState(DoorState.Opening);
            }
        }
        else
        {
            if (currentState == DoorState.Open || currentState == DoorState.Opening)
            {
                UpdateState(DoorState.Closing);
            }
        }
    }

    private bool IsAtDestination(Vector3 targetPosition)
    {
        if (!leftDoorPanel) return true;
        return Vector3.Distance(leftDoorPanel.position, targetPosition) < 0.01f;
    }

    private void MoveDoors(Vector3 leftTarget, Vector3 rightTarget)
    {
        float step = openCloseSpeed * Time.deltaTime;

        if (isDoubleDoor)
        {
            if (doorToOpen == DoorSelection.Both || doorToOpen == DoorSelection.Left)
                leftDoorPanel.position = Vector3.Lerp(leftDoorPanel.position, leftTarget, step);

            if (doorToOpen == DoorSelection.Both || doorToOpen == DoorSelection.Right)
                rightDoorPanel.position = Vector3.Lerp(rightDoorPanel.position, rightTarget, step);
        }
        else
        {
            if (doorToOpen == DoorSelection.Left)
                leftDoorPanel.position = Vector3.Lerp(leftDoorPanel.position, leftTarget, step);
            
            if (doorToOpen == DoorSelection.Right)
                rightDoorPanel.position = Vector3.Lerp(rightDoorPanel.position, rightTarget, step);

            if (doorToOpen == DoorSelection.Both)
            {
                return; // No acciona si es una sola puerta
            }
        }
    }

    private Vector3 GetTargetPosition(Transform door, Transform target)
    {
        return new Vector3(target.position.x, door.position.y, target.position.z);
    }

    public void PlayerPassedManualDoor()
    {
        if (isAutomatic) return;

        if (!isHackable)
        {
            isPermanentlyLocked = true;
            Debug.Log("Puerta bloqueada permanentemente.");
        }

        UpdateState(DoorState.Closing);
    }

    private void UpdateState(DoorState newState)
    {
        if (currentState == DoorState.Locked) return;
        if (currentState == newState) return;

        currentState = newState;

        UpdateEmissiveColor();
    }

    private void UpdateEmissiveColor()
    {
        Color targetColor = colorLocked;

        if (currentState == DoorState.Locked)
        {
            targetColor = colorLocked;
        }
        else if (isAutomatic)
        {
            targetColor = colorActive;
        }
        else
        {
            if (currentState == DoorState.Closed)
            {
                targetColor = isHackable ? colorHackable : colorActive;
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
        openCloseSpeed = originalSpeed * multiplier;
        yield return new WaitForSeconds(duration);
        openCloseSpeed = originalSpeed;
    }

    public void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal)
    {
        if (!isHackable || currentState == DoorState.Locked) return;

        Debug.Log("Puerta hackeada. Cambiando estado.");

        if (currentState == DoorState.Closed || currentState == DoorState.Closing)
        {
            UpdateState(DoorState.Opening);
        }
        else if (currentState == DoorState.Open || currentState == DoorState.Opening)
        {
            UpdateState(DoorState.Closing);
        }
    }
}