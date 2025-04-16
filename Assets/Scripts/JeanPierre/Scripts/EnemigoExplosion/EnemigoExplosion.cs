using UnityEngine;
using System.Collections;

public class EnemigoExplosion : MonoBehaviour
{
    [Header("Teletransportación")]
    public float minTeleportDistance = 1f;
    public float maxTeleportDistance = 3f;
    public int minTeleports = 3;
    public int maxTeleports = 4;
    public float zigzagTeleportAmplitude = 0.5f;
    public float lateralTeleportChance = 0.3f;

    [Header("Movimiento Continuo")]
    public float minAdvanceTime = 2f;
    public float maxAdvanceTime = 4f;
    public float moveSpeed = 3f;
    public float rotationSpeed = 180f;

    [Header("Movimiento Continuo - Zigzag")]
    public float continuousZigzagAmplitude = 1f;
    public float continuousZigzagFrequency = 2f;

    [Header("Transición de Opacidad (Efecto Espectral)")]
    public float disappearDuration = 0.1f;
    public float appearDuration = 0.1f;

    [Header("Referencias")]
    public Transform playerTransform;
    public GameObject toggleObject;

    private Renderer enemyRenderer;
    private Material enemyMaterial;
    private bool canMoveContinuously = false;
    private bool toggleZigzag = false;

    // Variable para almacenar el offset lateral previo y lograr un desplazamiento absoluto
    private Vector3 previousLateralOffset = Vector3.zero;

    void Start()
    {
        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            Debug.LogError("No se encontró un componente Renderer en " + gameObject.name);
            return;
        }
        enemyMaterial = enemyRenderer.material;

        if (playerTransform == null)
        {
            Debug.LogError("No se asignó la referencia al playerTransform en " + gameObject.name);
        }
        StartCoroutine(TeleportMovementRoutine());
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }

        if (canMoveContinuously)
        {
            float lateralOscillation = continuousZigzagAmplitude * Mathf.Sin(Time.time * continuousZigzagFrequency * 2 * Mathf.PI);
            Vector3 newLateralOffset = transform.right * lateralOscillation;
            Vector3 forwardMovement = transform.forward * moveSpeed * Time.deltaTime;
            transform.position += forwardMovement + (newLateralOffset - previousLateralOffset);
            previousLateralOffset = newLateralOffset;
        }
    }

    IEnumerator TeleportMovementRoutine()
    {
        while (true)
        {
            int teleportCount = Random.Range(minTeleports, maxTeleports + 1);
            for (int i = 0; i < teleportCount; i++)
            {
                yield return StartCoroutine(DoTeleport());
            }
            canMoveContinuously = true;
            float advanceTime = Random.Range(minAdvanceTime, maxAdvanceTime);
            yield return new WaitForSeconds(advanceTime);
            canMoveContinuously = false;
        }
    }

    IEnumerator DoTeleport()
    {
        yield return StartCoroutine(FadeAlpha(1f, 0f, disappearDuration));

        if (toggleObject != null)
        {
            toggleObject.SetActive(false);
        }

        if (playerTransform != null)
        {
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            float teleportDistance = Random.Range(minTeleportDistance, maxTeleportDistance);
            Vector3 perpendicular = Vector3.Cross(directionToPlayer, Vector3.up).normalized;
            float lateralOffset = (toggleZigzag ? 1f : -1f) * zigzagTeleportAmplitude;
            toggleZigzag = !toggleZigzag;
            Vector3 zigzagOffset = perpendicular * lateralOffset;

            if (Random.value < lateralTeleportChance)
            {
                transform.position += zigzagOffset;
            }
            else
            {
                transform.position += directionToPlayer * teleportDistance + zigzagOffset;
            }
        }

        if (toggleObject != null)
        {
            toggleObject.SetActive(true);
        }
        yield return StartCoroutine(FadeAlpha(0f, 1f, appearDuration));
    }

    IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        float elapsed = 0f;
        Color color = enemyMaterial.color;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newAlpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
            enemyMaterial.color = new Color(color.r, color.g, color.b, newAlpha);
            yield return null;
        }
        enemyMaterial.color = new Color(color.r, color.g, color.b, endAlpha);
    }
}




