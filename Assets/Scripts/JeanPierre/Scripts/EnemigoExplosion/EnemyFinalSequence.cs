// EnemyFinalSequence.cs
using UnityEngine;
using System.Collections;

public class EnemyFinalSequence : MonoBehaviour
{
    [Header("Configuración de activación")]
    public Transform playerTransform;
    public float triggerDistance = 5f;
    [Tooltip("Distancia a la que el enemigo reanuda movimiento para evitar teleports inmediatos")]
    public float reopenDistance = 7f;

    [Header("Advertencia visual")]
    public Renderer enemyRenderer;
    public Material warningMaterial;
    public float blinkInterval = 0.2f;
    public float warningDuration = 2f;

    [Header("Bala final")]
    public GameObject bulletPrefab;
    public Vector3 finalBulletScale = new Vector3(2f, 2f, 2f);
    public float scaleDuration = 1f;
    public float finalDelay = 1f;

    [Header("Temblor de cámara (explosión)")]
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeMagnitude = 0.5f;

    private bool isWarning = false;
    private bool finalSequenceTriggered = false;
    private Coroutine warningCoroutine;
    private Coroutine blinkCoroutine;
    private Material originalMaterial;
    private EnemigoExplosion explosionScript;

    void Awake()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (enemyRenderer == null)
            enemyRenderer = GetComponent<Renderer>();
    }

    void Start()
    {
        // Guardamos el material asignado por VidaEnemigoGeneral
        originalMaterial = enemyRenderer.material;
        explosionScript = GetComponent<EnemigoExplosion>();
        explosionScript.enabled = true;  // movimiento activo al inicio
    }

    void Update()
    {
        if (finalSequenceTriggered) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= triggerDistance)
        {
            // Jugador cerca: detenido y comienza advertencia
            if (explosionScript.enabled)
                explosionScript.enabled = false;

            if (!isWarning)
                warningCoroutine = StartCoroutine(WarningAndCheck());
        }
        else if (dist >= reopenDistance)
        {
            // Jugador suficientemente lejos: cancelamos advertencia y reanudamos movimiento
            if (isWarning)
            {
                StopCoroutine(warningCoroutine);
                StopBlinking();
                ResetMaterial();
                isWarning = false;
            }
            if (!explosionScript.enabled)
                explosionScript.enabled = true;
        }
        // en la zona intermedia, no hacemos nada
    }

    IEnumerator WarningAndCheck()
    {
        isWarning = true;
        blinkCoroutine = StartCoroutine(BlinkMaterial());

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > triggerDistance)
            {
                // si se aleja antes de tiempo, cancelar advertencia
                StopBlinking();
                ResetMaterial();
                isWarning = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Advertencia completada estando cerca
        StopBlinking();
        ResetMaterial();
        isWarning = false;
        TriggerFinalSequence();
    }

    IEnumerator BlinkMaterial()
    {
        bool toggle = false;
        while (true)
        {
            enemyRenderer.material = toggle ? warningMaterial : originalMaterial;
            toggle = !toggle;
            yield return new WaitForSeconds(blinkInterval);
        }
    }

    void StopBlinking()
    {
        if (blinkCoroutine != null)
        {
            StopCoroutine(blinkCoroutine);
            blinkCoroutine = null;
        }
    }

    void ResetMaterial()
    {
        enemyRenderer.material = originalMaterial;
    }

    void TriggerFinalSequence()
    {
        finalSequenceTriggered = true;
        StopAllCoroutines();
        ResetMaterial();

        explosionScript.enabled = false;
        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        // Instanciar bala
        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Destroy(bullet, scaleDuration + finalDelay + 0.1f);

        // Al crear la bala, eliminamos este GameObject
        Destroy(gameObject);

        // Temblor de cámara
        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));

        // Escalado de la bala
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

        yield return new WaitForSeconds(finalDelay);
    }

    IEnumerator CameraShake(float duration, float magnitude)
    {
        Transform cam = Camera.main.transform;
        Vector3 orig = cam.localPosition;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;
            cam.localPosition = orig + new Vector3(x, y, 0f);
            yield return null;
        }
        cam.localPosition = orig;
    }
}










//using UnityEngine;
//using System.Collections;

//public class EnemyFinalSequence : MonoBehaviour
//{
//    [Header("Configuración de activación")]
//    [Tooltip("Transform del jugador (asignado automáticamente si no se arrastra manualmente)")]
//    public Transform playerTransform;
//    [Tooltip("Distancia a la que se activa la secuencia final (el enemigo se queda quieto)")]
//    public float triggerDistance = 5f;

//    [Header("Bala final")]
//    [Tooltip("Prefab de la bala a crear en la posición del enemigo")]
//    public GameObject bulletPrefab;
//    [Tooltip("Escala final que alcanzará la bala")]
//    public Vector3 finalBulletScale = new Vector3(2f, 2f, 2f);
//    [Tooltip("Tiempo en el que la bala interpola su escala hasta la escala final")]
//    public float scaleDuration = 1f;
//    [Tooltip("Tiempo de espera una vez alcanzada la escala final, antes de destruir objetos")]
//    public float finalDelay = 1f;

//    [Header("Temblor de cámara (explosión)")]
//    [Tooltip("Duración del temblor de la cámara")]
//    public float cameraShakeDuration = 0.5f;
//    [Tooltip("Magnitud del temblor de la cámara")]
//    public float cameraShakeMagnitude = 0.5f;

//    // Bandera para activar la secuencia final solo una vez.
//    private bool finalSequenceTriggered = false;

//    void Awake()
//    {
//        if (playerTransform == null)
//        {
//            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
//            if (playerObj != null)
//                playerTransform = playerObj.transform;
//            else
//                Debug.LogWarning("[EnemyFinalSequence] No se encontró ningún GameObject con tag 'Player'.");
//        }
//    }

//    void Update()
//    {
//        if (!finalSequenceTriggered && playerTransform != null)
//        {
//            float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);
//            if (distanceToPlayer <= triggerDistance)
//                TriggerFinalSequence();
//        }
//    }

//    void OnTriggerEnter(Collider other)
//    {
//        if (!finalSequenceTriggered && other.CompareTag("GunBullet"))
//            TriggerFinalSequence();
//    }

//    void TriggerFinalSequence()
//    {
//        finalSequenceTriggered = true;
//        StopAllCoroutines();

//        EnemigoExplosion explosionScript = GetComponent<EnemigoExplosion>();
//        if (explosionScript != null)
//        {
//            explosionScript.StopAllCoroutines();
//            explosionScript.enabled = false;
//        }

//        StartCoroutine(FinalSequence());
//    }

//    IEnumerator FinalSequence()
//    {
//        // Instanciar bala
//        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
//        // Asegurar destrucción automática si la coroutine se interrumpe
//        Destroy(bullet, scaleDuration + finalDelay + 0.1f);

//        // Temblor de cámara
//        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));

//        // Lerp de escala
//        Vector3 initialScale = bullet.transform.localScale;
//        float elapsed = 0f;
//        while (elapsed < scaleDuration)
//        {
//            elapsed += Time.deltaTime;
//            float t = Mathf.Clamp01(elapsed / scaleDuration);
//            bullet.transform.localScale = Vector3.Lerp(initialScale, finalBulletScale, t);
//            yield return null;
//        }
//        bullet.transform.localScale = finalBulletScale;

//        // Espera final
//        yield return new WaitForSeconds(finalDelay);

//        // Destruir enemigo
//        Destroy(gameObject);
//    }

//    IEnumerator CameraShake(float duration, float magnitude)
//    {
//        Transform camTransform = Camera.main.transform;
//        Vector3 originalPos = camTransform.localPosition;
//        float elapsed = 0f;
//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float offsetX = Random.Range(-1f, 1f) * magnitude;
//            float offsetY = Random.Range(-1f, 1f) * magnitude;
//            camTransform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0f);
//            yield return null;
//        }
//        camTransform.localPosition = originalPos;
//    }
//}












