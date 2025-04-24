using UnityEngine;
using System.Collections;

public class EnemyFinalSequence : MonoBehaviour
{
    [Header("Configuraci�n de activaci�n")]
    [Tooltip("Transform del jugador (asignado autom�ticamente si no se arrastra manualmente)")]
    public Transform playerTransform;
    [Tooltip("Distancia a la que se activa la secuencia final (el enemigo se queda quieto)")]
    public float triggerDistance = 5f;

    [Header("Bala final")]
    [Tooltip("Prefab de la bala a crear en la posici�n del enemigo")]
    public GameObject bulletPrefab;
    [Tooltip("Escala final que alcanzar� la bala")]
    public Vector3 finalBulletScale = new Vector3(2f, 2f, 2f);
    [Tooltip("Tiempo en el que la bala interpola su escala hasta la escala final")]
    public float scaleDuration = 1f;
    [Tooltip("Tiempo de espera una vez alcanzada la escala final, antes de destruir objetos")]
    public float finalDelay = 1f;

    [Header("Temblor de c�mara (explosi�n)")]
    [Tooltip("Duraci�n del temblor de la c�mara")]
    public float cameraShakeDuration = 0.5f;
    [Tooltip("Magnitud del temblor de la c�mara")]
    public float cameraShakeMagnitude = 0.5f;

    // Bandera para activar la secuencia final solo una vez.
    private bool finalSequenceTriggered = false;

    void Awake()
    {
        // Si no se arrastr� manualmente, buscar por tag
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerTransform = playerObj.transform;
            else
                Debug.LogWarning("[EnemyFinalSequence] No se encontr� ning�n GameObject con tag 'Player'.");
        }
    }

    void Update()
    {
        // Se chequea la distancia siempre que la secuencia final no se haya activado
        if (!finalSequenceTriggered && playerTransform != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
            if (distanceToPlayer <= triggerDistance)
            {
                TriggerFinalSequence();
            }
        }
    }

    // Activaci�n mediante colisi�n con objeto con tag "BalaPlayer"
    void OnTriggerEnter(Collider other)
    {
        if (!finalSequenceTriggered && other.CompareTag("BalaPlayer"))
        {
            TriggerFinalSequence();
        }
    }

    // M�todo para activar la secuencia final
    void TriggerFinalSequence()
    {
        finalSequenceTriggered = true;
        // Detenemos todas las corrutinas de este script
        StopAllCoroutines();

        // Si existe el script de teletransportaci�n/movimiento, lo detenemos y deshabilitamos.
        EnemigoExplosion explosionScript = GetComponent<EnemigoExplosion>();
        if (explosionScript != null)
        {
            explosionScript.StopAllCoroutines();
            explosionScript.enabled = false;
        }
        // Iniciamos la secuencia final
        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        // Se crea la bala en la posici�n actual del enemigo.
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);

        // Se activa el temblor de c�mara con efecto de explosi�n.
        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));

        // Interpolaci�n suave de la escala de la bala hasta alcanzar la escala final.
        Vector3 initialScale = bullet.transform.localScale;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            bullet.transform.localScale = Vector3.Lerp(initialScale, finalBulletScale, t);
            yield return null;
        }
        bullet.transform.localScale = finalBulletScale;

        // Espera adicional antes de destruir la bala y el enemigo.
        yield return new WaitForSeconds(finalDelay);
        Destroy(bullet);
        Destroy(gameObject);
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Transform camTransform = Camera.main.transform;
        Vector3 originalPos = camTransform.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;
            camTransform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
            yield return null;
        }
        camTransform.localPosition = originalPos;
    }
}

