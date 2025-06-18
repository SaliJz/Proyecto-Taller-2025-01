using UnityEngine;

public class LookBackAtPlayer : MonoBehaviour
{
    [Header("Configuración")]
    [Tooltip("Velocidad de rotación (0 para rotación instantánea)")]
    public float rotationSpeed = 5f;
    [Tooltip("Inclinar hacia arriba/abajo según la altura del jugador")]
    public bool verticalRotation = false;

    private Transform playerTarget;

    void Start()
    {
        FindPlayer();
    }

    void Update()
    {
        if (playerTarget == null)
        {
            FindPlayer();
            return;
        }

        // Calcular dirección hacia el jugador
        Vector3 directionToPlayer = playerTarget.position - transform.position;

        // Ignorar altura si no se requiere rotación vertical
        if (!verticalRotation) directionToPlayer.y = 0;

        // Evitar rotación cero
        if (directionToPlayer == Vector3.zero) return;

        // Calcular rotación para que el BACK mire al jugador
        Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer);

        // Aplicar rotación
        if (rotationSpeed <= 0)
        {
            transform.rotation = targetRotation;
        }
        else
        {
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    void FindPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
        {
            playerTarget = playerObj.transform;
        }
        else
        {
            Debug.LogError("No se encontró objeto con tag 'Player'. Asegúrate de tener un jugador en escena con ese tag.", this);
            enabled = false; // Desactiva el script
        }
    }
}