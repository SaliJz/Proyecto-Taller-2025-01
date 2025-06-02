using UnityEngine;

// Este script destruye la bala cuando atraviesa cualquier objeto con el layer "Ground".
// Asegúrate de que el GameObject tenga un Rigidbody y un Collider configurado como "Is Trigger".

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class BulletDestroyOnGround : MonoBehaviour
{
    [Tooltip("Selecciona el layer Ground en el inspector para que la bala se destruya al colisionar.")]
    [SerializeField]
    private LayerMask groundLayerMask;

    private void Awake()
    {
        // Garantizar que el collider esté como trigger
        Collider col = GetComponent<Collider>();
        if (!col.isTrigger)
        {
            Debug.LogWarning("El Collider no estaba marcado como Trigger. Marcándolo automáticamente.");
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Comprueba si el objeto atravesado está en el layer Ground
        if ((groundLayerMask.value & (1 << other.gameObject.layer)) != 0)
        {
            Destroy(gameObject);
        }
    }
}
