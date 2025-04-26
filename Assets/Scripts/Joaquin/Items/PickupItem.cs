using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static SupplyBox;

public class PickupItem : MonoBehaviour
{
    public enum PickupType 
    { 
        CodeFragment, 
        AmmoSingle, 
        AmmoSemiAuto, 
        AmmoAuto 
    }

    [SerializeField] private PickupType pickupType;
    [SerializeField] private Vector2 amountRange;
    [SerializeField] private GameObject pickupCanvas;
    [SerializeField] private TextMeshProUGUI pickupAmountText;
    [SerializeField] private float pickUpRange = 5;
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float positionOffset = 1f;

    private int actualAmount;
    [SerializeField] private bool playerInRange = false;
    private bool isFlyingToPlayer = false;
    private Transform playerTarget;
    private Rigidbody rb;

    private void Start()
    {
        amountRange = pickupType switch
        {
            PickupType.CodeFragment => new Vector2(100, 200),
            PickupType.AmmoSingle => new Vector2(15, 20),
            PickupType.AmmoSemiAuto => new Vector2(3, 8),
            PickupType.AmmoAuto => new Vector2(10, 20),
            _ => new Vector2(0, 0)
        };

        actualAmount = Random.Range((int)amountRange.x, (int)amountRange.y + 1);
        UpdateVisual();

        // Buscar Rigidbody del hijo
        rb = GetComponentInChildren<Rigidbody>();

        if (rb != null)
        {
            switch (pickupType)
            {
                case PickupType.CodeFragment:
                    Debug.Log("Rigidbody encontrado, configurando propiedades para fragmento de código");
                    rb.mass = 0.1f; // Más "ligero"
                    rb.drag = 1f;
                    rb.angularDrag = 1f;
                    break;

                case PickupType.AmmoSingle:
                case PickupType.AmmoSemiAuto:
                case PickupType.AmmoAuto:
                    Debug.Log("Rigidbody encontrado, configurando propiedades para munición");
                    rb.mass = 0.3f;
                    rb.drag = 1f;
                    rb.angularDrag = 1f;
                    rb.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ |
                                     RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
                    break;
            }
        }

        // Si el objeto es un fragmento de código, lo volamos hacia el jugador
        if (pickupType == PickupType.CodeFragment)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                Debug.Log("Jugador encontrado.");
                playerTarget = player.transform;
            }
        }
    }

    private void Update()
    {
        if (pickupType == PickupType.CodeFragment)
        {
            if (!isFlyingToPlayer && playerTarget != null)
            {
                float distance = Vector3.Distance(transform.position, playerTarget.position);
                if (distance <= pickUpRange)
                {
                    isFlyingToPlayer = true;
                    if (rb != null) rb.isKinematic = true;
                }
            }

            FollowAtPlayer(); // llamado constante desde Update
        }
        else if (playerInRange)
        {
            TryPickup(); // solo para munición
        }
    }

    private void FollowAtPlayer()
    {
        if (!isFlyingToPlayer || playerTarget == null) return;

        Vector3 targetPosition = playerTarget.position + Vector3.up * positionOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        Vector3 lookDirection = targetPosition - transform.position;
        if (lookDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            HUDManager.Instance.AddInfoFragment(actualAmount);
            Destroy(gameObject);
        }
    }

    private void TryPickup()
    {
        Collider playerCollider = Physics.OverlapSphere(transform.position, pickUpRange).FirstOrDefault(c => c.CompareTag("Player"));

        if (playerCollider != null)
        {
            Debug.Log("Jugador encontrado en el rango de recogida");
        }
        else
        {
            Debug.Log("No se encontró al jugador en el rango de recogida");
            return;
        }

        Weapon weapon = FindWeaponNearby(transform.position, pickUpRange);
        if (weapon != null)
        {
            Debug.Log("Weapon encontrado en la jerarquía y capa especificada: " + weapon.name);
        }
        else
        {
            Debug.Log("No se encontró ningún Weapon en la jerarquía y capa especificada.");
            return;
        }
        int added = 0;

        switch (pickupType)
        {
            case PickupType.AmmoSingle:
                if (weapon.currentShootingMode == Weapon.ShootingMode.Single)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de pistola.");
                break;

            case PickupType.AmmoSemiAuto:
                if (weapon.currentShootingMode == Weapon.ShootingMode.SemiAuto)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de escopeta.");
                break;

            case PickupType.AmmoAuto:
                if (weapon.currentShootingMode == Weapon.ShootingMode.Auto)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de ametralladora.");
                break;
        }

        actualAmount -= added;
        UpdateVisual();

        if (actualAmount <= 0)
        {
            Destroy(gameObject);
        }
    }

    private Weapon FindWeaponNearby(Vector3 origin, float radius)
    {
        int weaponLayer = LayerMask.NameToLayer("Weapon");

        // Recorremos todos los colliders del radio
        Collider[] colliders = Physics.OverlapSphere(origin, radius);

        float shortestDistance = Mathf.Infinity;
        Weapon nearestWeapon = null;

        foreach (var col in colliders)
        {
            Weapon weapon = col.GetComponent<Weapon>();
            if (weapon != null)
            {
                float distance = Vector3.Distance(origin, col.transform.position);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestWeapon = weapon;
                }
            }
        }

        return nearestWeapon;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            if (pickupType != PickupType.CodeFragment)
            {
                playerInRange = true;
            }
        }
    }

    private void UpdateVisual()
    {
        if (pickupCanvas != null && pickupAmountText != null)
        {
            pickupAmountText.text = $"{pickupType} x{actualAmount}";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
