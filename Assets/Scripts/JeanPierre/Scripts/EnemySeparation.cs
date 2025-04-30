using UnityEngine;

[RequireComponent(typeof(Collider))]
public class EnemySeparation : MonoBehaviour
{
    [Tooltip("Distancia mínima a mantener con otros enemigos")]
    public float separationRadius = 2f;
    [Tooltip("Velocidad de separación")]
    public float separationStrength = 3f;

    void Update()
    {
        Collider[] others = Physics.OverlapSphere(transform.position, separationRadius);
        Vector3 repulsion = Vector3.zero;
        int neighbourCount = 0;

        foreach (var col in others)
        {
            if (col.gameObject != gameObject && col.CompareTag("Enemy"))
            {
                Vector3 diff = transform.position - col.transform.position;
                if (diff.sqrMagnitude > 0f)
                {
                    repulsion += diff.normalized / diff.magnitude;
                    neighbourCount++;
                }
            }
        }

        if (neighbourCount > 0)
        {
            repulsion /= neighbourCount;
            transform.position += repulsion * separationStrength * Time.deltaTime;
        }
    }

    // Opcional: ver el radio de separación en el editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
    }
}
