using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class UpwardExplosionTrigger : MonoBehaviour
{
    [Header("Fuerza de empuje hacia arriba")]
    [Tooltip("Magnitud del impulso vertical para echar fuera al jugador.")]
    public float explosionForce = 15f;

    [Header("Reseteo del trigger")]
    [Tooltip("Tiempo en segundos que el collider está desactivado después de empujar.")]
    public float resetDelay = 0.5f;

    private Collider triggerCollider;

    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        Rigidbody rb = other.attachedRigidbody;
        if (rb == null) return;

        // Fuerza única hacia arriba, ignorando la masa
        rb.AddForce(Vector3.up * explosionForce, ForceMode.VelocityChange);

        // Desactiva el collider para obligar al jugador a salir
        StartCoroutine(TemporaryDisableCollider());
    }

    private IEnumerator TemporaryDisableCollider()
    {
        triggerCollider.enabled = false;
        yield return new WaitForSeconds(resetDelay);
        triggerCollider.enabled = true;
    }
}
