using UnityEngine;

public class LookAtPlayerY : MonoBehaviour
{
    [Tooltip("Tag que identifica al jugador")]
    public string playerTag = "Player";

    private Transform player;

    void Awake()
    {
        GameObject go = GameObject.FindGameObjectWithTag(playerTag);
        if (go != null)
        {
            player = go.transform;
        }
        else
        {
            Debug.LogWarning($"No se encontró ningún GameObject con tag '{playerTag}'");
        }
    }

    void Update()
    {
        if (player == null)
            return;

        // Calculamos el vector dirección desde este objeto al jugador
        Vector3 direction = player.position - transform.position;

        // Anulamos la componente Y para rotar solo en el plano horizontal
        direction.y = 0f;

        // Evitamos giros si la distancia es prácticamente cero
        if (direction.sqrMagnitude < 0.0001f)
            return;

        // Ángulo deseado en Y
        float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        // Aplicamos la rotación en Y, sin tocar X ni Z
        transform.rotation = Quaternion.Euler(0f, targetAngle, 0f);
    }
}
