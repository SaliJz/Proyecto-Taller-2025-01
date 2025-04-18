




using UnityEngine;

public class PositionAboveAndFacePlayer : MonoBehaviour
{
    [Header("Configuración de Posición")]
    [Tooltip("Objeto al que seguirá este GameObject")]
    public GameObject targetObject;

    [Tooltip("Distancia vertical sobre el objeto objetivo")]
    public float verticalOffset = 2f;

    [Header("Configuración de Rotación")]
    [Tooltip("Transform del jugador para orientación")]
    public Transform playerTransform;

    [Tooltip("Suavizado de rotación (0 = instantáneo)")]
    [Range(0, 1)]
    public float rotationSmoothness = 0.1f;

    [Header("Opciones")]
    [Tooltip("Mantener altura constante respecto al objetivo")]
    public bool maintainHeight = true;

    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private Vector3 originalOffset;

    void Start()
    {
        if (targetObject != null)
        {
            originalOffset = transform.position - targetObject.transform.position;
        }
    }

    void LateUpdate()
    {
        if (targetObject != null && playerTransform != null)
        {
            UpdatePosition();
            UpdateRotation();
        }
    }

    void UpdatePosition()
    {
        // Calcular posición con offset vertical
        Vector3 basePosition = targetObject.transform.position;
        targetPosition = maintainHeight ?
            basePosition + originalOffset :
            basePosition + Vector3.up * verticalOffset;

        transform.position = targetPosition;
    }

    void UpdateRotation()
    {
        // Calcular dirección hacia el jugador
        Vector3 lookDirection = playerTransform.position - transform.position;
        lookDirection.y = 0; // Mantener rotación horizontal

        if (lookDirection != Vector3.zero)
        {
            // Calcular rotación objetivo
            targetRotation = Quaternion.LookRotation(lookDirection);

            // Aplicar suavizado de rotación
            if (rotationSmoothness > 0)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime / rotationSmoothness
                );
            }
            else
            {
                transform.rotation = targetRotation;
            }
        }
    }

    // Dibuja gizmos en el editor para mejor visualización
    void OnDrawGizmosSelected()
    {
        if (targetObject != null && playerTransform != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, playerTransform.position);
        }
    }
}