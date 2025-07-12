using System.Collections;
using UnityEngine;

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

    [Header("Material HDR")]
    [Tooltip("Renderer del GameObject que tiene el material HDR original")]
    public Renderer hdrRenderer;
    [Tooltip("Material HDR blanco para advertencia")]
    public Material warningHDRMaterial;

    [Header("Bala final")]
    public GameObject bulletPrefab;
    public Vector3 finalBulletScale = new Vector3(2f, 2f, 2f);
    public float scaleDuration = 1f;
    public float finalDelay = 1f;

    [Header("Nuevo Prefab")]
    [Tooltip("Prefab adicional a instanciar al destruirse este objeto")]
    public GameObject newPrefab;

    [Header("Temblor de cámara (explosión)")]
    public float cameraShakeDuration = 0.5f;
    public float cameraShakeMagnitude = 0.5f;

    private bool isWarning = false;
    private bool finalSequenceTriggered = false;
    private bool isQuitting = false;
    private Coroutine warningCoroutine;
    private Coroutine blinkCoroutine;
    private Material originalMaterial;
    private Material originalHDRMaterial;
    private EnemigoExplosion explosionScript;

    void Awake()
    {
        if (playerTransform == null)
            playerTransform = GameObject.FindWithTag("Player")?.transform;
        if (enemyRenderer == null)
            enemyRenderer = GetComponent<Renderer>();
        // hdrRenderer y warningHDRMaterial deben asignarse en el Inspector
    }

    void Start()
    {
        originalMaterial = enemyRenderer.material;

        if (hdrRenderer != null)
            originalHDRMaterial = hdrRenderer.material;

        explosionScript = GetComponent<EnemigoExplosion>();
        if (explosionScript != null)
            explosionScript.enabled = true;
    }

    void Update()
    {
        if (finalSequenceTriggered) return;

        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= triggerDistance)
        {
            if (explosionScript != null && explosionScript.enabled)
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
                ResetMaterials();
                isWarning = false;
            }
            if (explosionScript != null && !explosionScript.enabled)
                explosionScript.enabled = true;
        }
    }

    IEnumerator WarningAndCheck()
    {
        isWarning = true;
        blinkCoroutine = StartCoroutine(BlinkMaterials());

        float elapsed = 0f;
        while (elapsed < warningDuration)
        {
            if (Vector3.Distance(transform.position, playerTransform.position) > triggerDistance)
            {
                StopBlinking();
                ResetMaterials();
                isWarning = false;
                yield break;
            }
            elapsed += Time.deltaTime;
            yield return null;
        }

        StopBlinking();
        ResetMaterials();
        isWarning = false;
        TriggerFinalSequence();
    }

    IEnumerator BlinkMaterials()
    {
        bool toggle = false;
        while (true)
        {
            if (toggle)
            {
                enemyRenderer.material = warningMaterial;
                if (hdrRenderer != null && warningHDRMaterial != null)
                    hdrRenderer.material = warningHDRMaterial;
            }
            else
            {
                enemyRenderer.material = originalMaterial;
                if (hdrRenderer != null && originalHDRMaterial != null)
                    hdrRenderer.material = originalHDRMaterial;
            }

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

    void ResetMaterials()
    {
        enemyRenderer.material = originalMaterial;
        if (hdrRenderer != null && originalHDRMaterial != null)
            hdrRenderer.material = originalHDRMaterial;
    }

    void TriggerFinalSequence()
    {
        finalSequenceTriggered = true;
        StopAllCoroutines();
        ResetMaterials();
        if (explosionScript != null)
            explosionScript.enabled = false;
        StartCoroutine(FinalSequence());
    }

    IEnumerator FinalSequence()
    {
        SpawnBulletInstantly();
        StartCoroutine(CameraShake(cameraShakeDuration, cameraShakeMagnitude));
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

    void OnDestroy()
    {
        if (!isQuitting)
        {
            // Instancia el prefab adicional al destruirse este objeto
            Instantiate(newPrefab, transform.position, Quaternion.identity);
            MissionManager.Instance?.RegisterKill(gameObject.tag, name, null);
        }
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























