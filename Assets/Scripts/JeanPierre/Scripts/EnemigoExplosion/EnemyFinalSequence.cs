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
    private bool isQuitting = false;
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
        originalMaterial = enemyRenderer.material;
        explosionScript = GetComponent<EnemigoExplosion>();
        explosionScript.enabled = true;
    }

    void Update()
    {
        if (finalSequenceTriggered) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= triggerDistance)
        {
            if (explosionScript.enabled)
                explosionScript.enabled = false;
            if (!isWarning)
                warningCoroutine = StartCoroutine(WarningAndCheck());
        }
        else if (dist >= reopenDistance)
        {
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
                StopBlinking();
                ResetMaterial();
                isWarning = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

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
        // Instanciamos y escalamos la bala
        SpawnBulletInstantly();

        // Temblor de cámara
        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));

        // Destruimos este enemigo
        Destroy(gameObject);

        yield return null;
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

    void OnApplicationQuit()
    {
        isQuitting = true;
    }

    void OnDisable()
    {
        if (!isQuitting && !finalSequenceTriggered)
            SpawnBulletInstantly();
    }

    private void SpawnBulletInstantly()
    {
        var bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        StartCoroutine(ScaleBullet(bullet));
        Destroy(bullet, scaleDuration + finalDelay + 0.1f);
    }

    private IEnumerator ScaleBullet(GameObject bullet)
    {
        Vector3 startScale = bullet.transform.localScale;
        float elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / scaleDuration);
            bullet.transform.localScale = Vector3.Lerp(startScale, finalBulletScale, t);
            yield return null;
        }
        bullet.transform.localScale = finalBulletScale;
        yield return new WaitForSeconds(finalDelay);
    }
}


//// EnemyFinalSequence.cs
//using UnityEngine;
//using System.Collections;

//public class EnemyFinalSequence : MonoBehaviour
//{
//    [Header("Configuración de activación")]
//    public Transform playerTransform;
//    public float triggerDistance = 5f;
//    [Tooltip("Distancia a la que el enemigo reanuda movimiento para evitar teleports inmediatos")]
//    public float reopenDistance = 7f;

//    [Header("Advertencia visual")]
//    public Renderer enemyRenderer;
//    public Material warningMaterial;
//    public float blinkInterval = 0.2f;
//    public float warningDuration = 2f;

//    [Header("Bala final")]
//    public GameObject bulletPrefab;
//    public Vector3 finalBulletScale = new Vector3(2f, 2f, 2f);
//    public float scaleDuration = 1f;
//    public float finalDelay = 1f;

//    [Header("Temblor de cámara (explosión)")]
//    public float cameraShakeDuration = 0.5f;
//    public float cameraShakeMagnitude = 0.5f;

//    private bool isWarning = false;
//    private bool finalSequenceTriggered = false;
//    private Coroutine warningCoroutine;
//    private Coroutine blinkCoroutine;
//    private Material originalMaterial;
//    private EnemigoExplosion explosionScript;

//    void Awake()
//    {
//        if (playerTransform == null)
//            playerTransform = GameObject.FindWithTag("Player")?.transform;
//        if (enemyRenderer == null)
//            enemyRenderer = GetComponent<Renderer>();
//    }

//    void Start()
//    {
//        // Guardamos el material asignado por VidaEnemigoGeneral
//        originalMaterial = enemyRenderer.material;
//        explosionScript = GetComponent<EnemigoExplosion>();
//        explosionScript.enabled = true;  // movimiento activo al inicio
//    }

//    void Update()
//    {
//        if (finalSequenceTriggered) return;

//        float dist = Vector3.Distance(transform.position, playerTransform.position);

//        if (dist <= triggerDistance)
//        {
//            // Jugador cerca: detenido y comienza advertencia
//            if (explosionScript.enabled)
//                explosionScript.enabled = false;

//            if (!isWarning)
//                warningCoroutine = StartCoroutine(WarningAndCheck());
//        }
//        else if (dist >= reopenDistance)
//        {
//            // Jugador suficientemente lejos: cancelamos advertencia y reanudamos movimiento
//            if (isWarning)
//            {
//                StopCoroutine(warningCoroutine);
//                StopBlinking();
//                ResetMaterial();
//                isWarning = false;
//            }
//            if (!explosionScript.enabled)
//                explosionScript.enabled = true;
//        }
//        // en la zona intermedia, no hacemos nada
//    }

//    IEnumerator WarningAndCheck()
//    {
//        isWarning = true;
//        blinkCoroutine = StartCoroutine(BlinkMaterial());

//        float elapsed = 0f;
//        while (elapsed < warningDuration)
//        {
//            if (Vector3.Distance(transform.position, playerTransform.position) > triggerDistance)
//            {
//                // si se aleja antes de tiempo, cancelar advertencia
//                StopBlinking();
//                ResetMaterial();
//                isWarning = false;
//                yield break;
//            }
//            elapsed += Time.deltaTime;
//            yield return null;
//        }

//        // Advertencia completada estando cerca
//        StopBlinking();
//        ResetMaterial();
//        isWarning = false;
//        TriggerFinalSequence();
//    }

//    IEnumerator BlinkMaterial()
//    {
//        bool toggle = false;
//        while (true)
//        {
//            enemyRenderer.material = toggle ? warningMaterial : originalMaterial;
//            toggle = !toggle;
//            yield return new WaitForSeconds(blinkInterval);
//        }
//    }

//    void StopBlinking()
//    {
//        if (blinkCoroutine != null)
//        {
//            StopCoroutine(blinkCoroutine);
//            blinkCoroutine = null;
//        }
//    }

//    void ResetMaterial()
//    {
//        enemyRenderer.material = originalMaterial;
//    }

//    void TriggerFinalSequence()
//    {
//        finalSequenceTriggered = true;
//        StopAllCoroutines();
//        ResetMaterial();

//        explosionScript.enabled = false;
//        StartCoroutine(FinalSequence());
//    }

//    IEnumerator FinalSequence()
//    {
//        // Instanciar bala
//        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
//        Destroy(bullet, scaleDuration + finalDelay + 0.1f);

//        // Al crear la bala, eliminamos este GameObject
//        Destroy(gameObject);

//        // Temblor de cámara
//        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));

//        // Escalado de la bala
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

//        yield return new WaitForSeconds(finalDelay);
//    }

//    IEnumerator CameraShake(float duration, float magnitude)
//    {
//        Transform cam = Camera.main.transform;
//        Vector3 orig = cam.localPosition;
//        float elapsed = 0f;
//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float x = Random.Range(-1f, 1f) * magnitude;
//            float y = Random.Range(-1f, 1f) * magnitude;
//            cam.localPosition = orig + new Vector3(x, y, 0f);
//            yield return null;
//        }
//        cam.localPosition = orig;
//    }


//}















