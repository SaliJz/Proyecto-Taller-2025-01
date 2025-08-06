using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    [HideInInspector] public Transform player;
    [HideInInspector] public float speed;
    [HideInInspector] public int damage;
    [HideInInspector] public Transform firePoint; 

    [Header("Carga antes del disparo")]
    public float chargeTime = 1f;

    private Renderer rend;
    private Rigidbody rb;
    private float chargeTimer = 0f;
    private Vector3 initialScale;
    private bool isCharging = true;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        rb = GetComponent<Rigidbody>();

        rb.isKinematic = true; 
        rb.useGravity = false;

        initialScale = Vector3.one * 0.1f;
        transform.localScale = initialScale;
        if (rend) rend.material.color = Color.white;
    }

    private void Update()
    {
        if (isCharging)
        {
            if (firePoint != null)
            {
                transform.position = firePoint.position;
                transform.rotation = firePoint.rotation;
            }

            chargeTimer += Time.deltaTime;
            float t = chargeTimer / chargeTime;
            transform.localScale = Vector3.Lerp(initialScale, Vector3.one, t);

            if (rend)
            {
                float colorT = Mathf.PingPong(Time.time * 3f, 1f);
                Color c = Color.Lerp(Color.red, Color.yellow, colorT);
                c.a = Mathf.Lerp(0.7f, 1f, colorT);
                rend.material.color = c;
            }

            if (chargeTimer >= chargeTime)
            {
                isCharging = false;
                FireProjectile();
            }
        }
    }

    private void FireProjectile()
    {
        rb.isKinematic = false; 
        Vector3 direction = (player != null)
            ? (player.position - transform.position).normalized
            : transform.forward;
        rb.velocity = direction * speed;

        Destroy(gameObject, 5f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(damage, transform.position);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            Destroy(gameObject);
        }
    }
}