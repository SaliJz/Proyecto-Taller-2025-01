using UnityEngine;

public class LookBackAtPlayer : MonoBehaviour
{
    [Header("Configuraci�n")]
    [Tooltip("Velocidad de rotaci�n (0 para rotaci�n instant�nea)")]
    public float rotationSpeed = 5f;
    [Tooltip("Inclinar hacia arriba/abajo seg�n la altura del jugador")]
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

        // Calcular direcci�n hacia el jugador
        Vector3 directionToPlayer = playerTarget.position - transform.position;

        // Ignorar altura si no se requiere rotaci�n vertical
        if (!verticalRotation) directionToPlayer.y = 0;

        // Evitar rotaci�n cero
        if (directionToPlayer == Vector3.zero) return;

        // Calcular rotaci�n para que el BACK mire al jugador
        Quaternion targetRotation = Quaternion.LookRotation(-directionToPlayer);

        // Aplicar rotaci�n
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
            Debug.LogError("No se encontr� objeto con tag 'Player'. Aseg�rate de tener un jugador en escena con ese tag.", this);
            enabled = false; // Desactiva el script
        }
    }
}