using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

[RequireComponent(typeof(Light), typeof(SphereCollider))]
public class MovingSpotlightEvent : MonoBehaviour
{
    private Dictionary<PlayerHealth, Coroutine> damageCoroutines = new Dictionary<PlayerHealth, Coroutine>();

    [Header("Movimiento")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float spotlightSpeed = 2f; // Se multiplicará por la velocidad del jugador
    [SerializeField] private float changePatternInterval = 5f;

    [Header("Daño")]
    [SerializeField] private float damagePerSecond = 1f;
    [SerializeField] private LayerMask playerLayer;

    [Header("Luz y Proyección")]
    [SerializeField] private float spotlightRadius = 3f;

    [Header("Referencias")]
    [SerializeField] private Sprite eventIcon; // Icono que se mostrará en el HUD

    private Light spotLight;
    private SphereCollider damageArea;
    private int currentWaypoint = 0;
    private float patternTimer;

    private void Awake()
    {
        if (spotLight == null)
            spotLight = GetComponent<Light>();

        if (damageArea == null)
            damageArea = GetComponent<SphereCollider>();

        spotLight.type = LightType.Spot;
        spotLight.spotAngle = 90f;
        spotLight.range = 20f;

        damageArea.isTrigger = true;
        damageArea.radius = spotlightRadius;
    }

    private void Start()
    {
        if (HUDManager.Instance != null && eventIcon != null)
            HUDManager.Instance.UpdateIcon(eventIcon);
    }

    private void Update()
    {
        MoveToNextWaypoint();
        patternTimer += Time.deltaTime;

        if (patternTimer >= changePatternInterval)
        {
            ChangePattern();
            patternTimer = 0f;
        }

        transform.rotation = Quaternion.LookRotation(Vector3.down);
    }

    private void MoveToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        Transform target = waypoints[currentWaypoint];
        transform.position = Vector3.MoveTowards(transform.position, target.position, spotlightSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, target.position) < 0.2f)
            currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
    }

    private void ChangePattern()
    {
        // Mezclar los waypoints para variar el patrón
        waypoints = waypoints.OrderBy(x => UnityEngine.Random.value).ToArray();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            PlayerHealth player = other.GetComponent<PlayerHealth>();
            if (player != null && !damageCoroutines.ContainsKey(player))
            {
                Coroutine c = StartCoroutine(DamageOverTime(player));
                damageCoroutines[player] = c;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerHealth player = other.GetComponent<PlayerHealth>();
        if (player != null && damageCoroutines.ContainsKey(player))
        {
            StopCoroutine(damageCoroutines[player]);
            damageCoroutines.Remove(player);
        }
    }

    private IEnumerator DamageOverTime(PlayerHealth player)
    {
        while (true)
        {
            player.TakeDamage((int)damagePerSecond);
            yield return new WaitForSeconds(1f);
        }
    }
}
