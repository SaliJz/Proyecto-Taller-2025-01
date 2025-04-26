using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class SupplyBox : MonoBehaviour
{
    public enum SupplyType
    {
        CodeFragment,
        AmmoSingle,
        AmmoSemiAuto,
        AmmoAuto
    }

    [SerializeField] private SupplyType supplyType;
    [SerializeField] private Vector2 amountRange;
    [SerializeField] private GameObject pickupCanvas;
    [SerializeField] private TextMeshProUGUI pickupAmountText;
    [SerializeField] private float pickUpRange;

    private int actualAmount;
    private bool playerInRange = false;

    private void Start()
    {
        pickupCanvas.SetActive(false);

        amountRange = supplyType switch
        {
            SupplyType.CodeFragment => new Vector2(100, 200),
            SupplyType.AmmoSingle => new Vector2(15, 20),
            SupplyType.AmmoSemiAuto => new Vector2(3, 8),
            SupplyType.AmmoAuto => new Vector2(10, 20),
            _ => new Vector2(0, 0)
        };

        actualAmount = Random.Range((int)amountRange.x, (int)amountRange.y + 1);
        UpdateVisual();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("Intentando recoger el objeto");
            TrySuplyBox();
        }
    }

    private void TrySuplyBox()
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

        switch (supplyType)
        {
            case SupplyType.CodeFragment:
                HUDManager.Instance.AddInfoFragment(actualAmount);
                Debug.Log($"Recogido {actualAmount} fragmentos de código.");
                Destroy(gameObject);
                return;

            case SupplyType.AmmoSingle:
                if (weapon.currentShootingMode == Weapon.ShootingMode.Single)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de pistola.");
                break;

            case SupplyType.AmmoSemiAuto:
                if (weapon.currentShootingMode == Weapon.ShootingMode.SemiAuto)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de escopeta.");
                break;

            case SupplyType.AmmoAuto:
                if (weapon.currentShootingMode == Weapon.ShootingMode.Auto)
                    added = weapon.TryAddAmmo(actualAmount);
                Debug.Log($"Recogido {added} balas de ametralladora.");
                break;
        }

        actualAmount -= added;
        UpdateVisual();

        if (actualAmount <= 0)
        {
            Debug.Log($"Máximo de munición/fragmentos de codigo recogido");
            return; // salimos si no queda más munición
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
            if (col.gameObject.layer == weaponLayer)
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
        }

        return nearestWeapon;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            playerInRange = true;
            pickupCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            playerInRange = false;
            pickupCanvas.gameObject.SetActive(false);
        }
    }


    private void UpdateVisual()
    {
        if (pickupCanvas != null && pickupAmountText != null)
        {
            pickupAmountText.text = $"{supplyType} x{actualAmount}";
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
