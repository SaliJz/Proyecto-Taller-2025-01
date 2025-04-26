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
    private Transform playerTransform;
    public GameObject toggleObject;

    private Renderer enemyRenderer;
    private Material enemyMaterial;
    private bool canMoveContinuously = false;
    private bool toggleZigzag = false;
    private Vector3 previousLateralOffset = Vector3.zero;

    private EnemyAbilityReceiver abilityReceiver;

    void Awake()
    {
        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
        if (playerGO != null)
            playerTransform = playerGO.transform;
        else
            Debug.LogError("No se encontró ningún GameObject con tag 'Player'.");
    }

    void Start()
    {
        if (abilityReceiver == null)
        {
            abilityReceiver = GetComponent<EnemyAbilityReceiver>();
        }
        else
        {
            Debug.LogWarning("No se encontró el componente EnemyAbilityReceiver en " + gameObject.name);
        }

        enemyRenderer = GetComponent<Renderer>();
        if (enemyRenderer == null)
        {
            Debug.LogError("No se encontró un componente Renderer en " + gameObject.name);
            return;
        }
        enemyMaterial = enemyRenderer.material;

        if (playerTransform == null)
            Debug.LogError("playerTransform no fue asignado en Awake().");

        StartCoroutine(TeleportMovementRoutine());
    }

    void Update()
    {
        if (playerTransform != null)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.deltaTime
            );
        }

        if (canMoveContinuously)
        {
            float lateralOsc = continuousZigzagAmplitude *
                               Mathf.Sin(Time.time * continuousZigzagFrequency * 2f * Mathf.PI);

            float speed = abilityReceiver ? abilityReceiver.CurrentSpeed : moveSpeed;

            Vector3 lateralOffset = transform.right * lateralOsc;
            Vector3 forwardMove = transform.forward * speed * Time.deltaTime;
            transform.position += forwardMove + (lateralOffset - previousLateralOffset);
            previousLateralOffset = lateralOffset;
        }
    }

    IEnumerator TeleportMovementRoutine()
    {
        while (true)
        {
            int teleportCount = Random.Range(minTeleports, maxTeleports + 1);
            for (int i = 0; i < teleportCount; i++)
                yield return StartCoroutine(DoTeleport());

            canMoveContinuously = true;
            float advanceTime = Random.Range(minAdvanceTime, maxAdvanceTime);
            yield return new WaitForSeconds(advanceTime);
            canMoveContinuously = false;
        }
    }

    IEnumerator DoTeleport()
    {
        // Sólo toggle, sin cambiar alfa
        if (toggleObject != null)
            toggleObject.SetActive(false);

        if (playerTransform != null)
        {
            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
            float teleportDistance = Random.Range(minTeleportDistance, maxTeleportDistance);
            Vector3 perp = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
            float lateral = (toggleZigzag ? 1f : -1f) * zigzagTeleportAmplitude;
            toggleZigzag = !toggleZigzag;
            Vector3 zigzagOffset = perp * lateral;

            if (Random.value < lateralTeleportChance)
                transform.position += zigzagOffset;
            else
                transform.position += dirToPlayer * teleportDistance + zigzagOffset;
        }

        if (toggleObject != null)
            toggleObject.SetActive(true);

        // Espera igual al tiempo de aparición
        yield return new WaitForSeconds(appearDuration);
    }

    // FadeAlpha vaciado para no tocar la alfa
    IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
    {
        yield return new WaitForSeconds(duration);
    }
}
 




//using UnityEngine;
//using System.Collections;

//public class EnemigoExplosion : MonoBehaviour
//{
//    [Header("Teletransportación")]
//    public float minTeleportDistance = 1f;
//    public float maxTeleportDistance = 3f;
//    public int minTeleports = 3;
//    public int maxTeleports = 4;
//    public float zigzagTeleportAmplitude = 0.5f;
//    public float lateralTeleportChance = 0.3f;

//    [Header("Movimiento Continuo")]
//    public float minAdvanceTime = 2f;
//    public float maxAdvanceTime = 4f;
//    public float moveSpeed = 3f;
//    public float rotationSpeed = 180f;

//    [Header("Movimiento Continuo - Zigzag")]
//    public float continuousZigzagAmplitude = 1f;
//    public float continuousZigzagFrequency = 2f;

//    [Header("Transición de Opacidad (Efecto Espectral)")]
//    public float disappearDuration = 0.1f;
//    public float appearDuration = 0.1f;

//    [Header("Referencias")]
//    // Se asigna automáticamente en Awake usando el tag "Player"
//    private Transform playerTransform;
//    public GameObject toggleObject;

//    // Componentes internos
//    private Renderer enemyRenderer;
//    private Material enemyMaterial;
//    private bool canMoveContinuously = false;
//    private bool toggleZigzag = false;
//    private Vector3 previousLateralOffset = Vector3.zero;

//    void Awake()
//    {
//        // Busca el GameObject con tag "Player" y obtiene su Transform
//        GameObject playerGO = GameObject.FindGameObjectWithTag("Player");
//        if (playerGO != null)
//        {
//            playerTransform = playerGO.transform;
//        }
//        else
//        {
//            Debug.LogError("No se encontró ningún GameObject con tag 'Player'. " +
//                           "Asegúrate de que tu jugador tenga ese tag asignado.");
//        }
//    }

//    void Start()
//    {
//        // Obtiene el Renderer y material del enemigo
//        enemyRenderer = GetComponent<Renderer>();
//        if (enemyRenderer == null)
//        {
//            Debug.LogError("No se encontró un componente Renderer en " + gameObject.name);
//            return;
//        }
//        enemyMaterial = enemyRenderer.material;

//        if (playerTransform == null)
//        {
//            Debug.LogError("playerTransform no fue asignado en Awake().");
//        }

//        StartCoroutine(TeleportMovementRoutine());
//    }

//    void Update()
//    {
//        // Hace que el enemigo rote suavemente para mirar al jugador
//        if (playerTransform != null)
//        {
//            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
//            Quaternion targetRot = Quaternion.LookRotation(dirToPlayer, Vector3.up);
//            transform.rotation = Quaternion.RotateTowards(
//                transform.rotation,
//                targetRot,
//                rotationSpeed * Time.deltaTime
//            );
//        }

//        // Movimiento continuo con zigzag
//        if (canMoveContinuously)
//        {
//            float lateralOsc = continuousZigzagAmplitude *
//                               Mathf.Sin(Time.time * continuousZigzagFrequency * 2f * Mathf.PI);
//            Vector3 lateralOffset = transform.right * lateralOsc;
//            Vector3 forwardMove = transform.forward * moveSpeed * Time.deltaTime;
//            transform.position += forwardMove + (lateralOffset - previousLateralOffset);
//            previousLateralOffset = lateralOffset;
//        }
//    }

//    IEnumerator TeleportMovementRoutine()
//    {
//        while (true)
//        {
//            // Teletransportaciones consecutivas
//            int teleportCount = Random.Range(minTeleports, maxTeleports + 1);
//            for (int i = 0; i < teleportCount; i++)
//            {
//                yield return StartCoroutine(DoTeleport());
//            }

//            // Luego avanza continuamente un rato
//            canMoveContinuously = true;
//            float advanceTime = Random.Range(minAdvanceTime, maxAdvanceTime);
//            yield return new WaitForSeconds(advanceTime);
//            canMoveContinuously = false;
//        }
//    }

//    IEnumerator DoTeleport()
//    {
//        // Desaparece
//        yield return StartCoroutine(FadeAlpha(1f, 0f, disappearDuration));

//        if (toggleObject != null)
//            toggleObject.SetActive(false);

//        // Calcula nueva posición respecto al jugador
//        if (playerTransform != null)
//        {
//            Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;
//            float teleportDistance = Random.Range(minTeleportDistance, maxTeleportDistance);
//            Vector3 perp = Vector3.Cross(dirToPlayer, Vector3.up).normalized;
//            float lateral = (toggleZigzag ? 1f : -1f) * zigzagTeleportAmplitude;
//            toggleZigzag = !toggleZigzag;
//            Vector3 zigzagOffset = perp * lateral;

//            if (Random.value < lateralTeleportChance)
//                transform.position += zigzagOffset;
//            else
//                transform.position += dirToPlayer * teleportDistance + zigzagOffset;
//        }

//        // Reaparece
//        if (toggleObject != null)
//            toggleObject.SetActive(true);

//        yield return StartCoroutine(FadeAlpha(0f, 1f, appearDuration));
//    }

//    IEnumerator FadeAlpha(float startAlpha, float endAlpha, float duration)
//    {
//        float elapsed = 0f;
//        Color col = enemyMaterial.color;
//        while (elapsed < duration)
//        {
//            elapsed += Time.deltaTime;
//            float a = Mathf.Lerp(startAlpha, endAlpha, elapsed / duration);
//            enemyMaterial.color = new Color(col.r, col.g, col.b, a);
//            yield return null;
//        }
//        enemyMaterial.color = new Color(col.r, col.g, col.b, endAlpha);
//    }
//}



