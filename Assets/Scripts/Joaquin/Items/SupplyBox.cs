using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using static PickupItem;

public class SupplyBox : MonoBehaviour
{
    public enum SupplyType
    {
        CodeFragment,
        Single,
        SemiAuto,
        Auto
    }

    [SerializeField] private SupplyType supplyType;
    [SerializeField] private Vector2 amountRange;
    [SerializeField] private GameObject supplyCanvas;
    [SerializeField] private TextMeshProUGUI supplyAmountText;
    [SerializeField] private float supplyRange;
    [SerializeField] private float refillDelay = 5f;
    [SerializeField] private bool autoRefill;

    private int actualAmount;
    private bool playerInRange = false;

    private void Start()
    {
        supplyCanvas.SetActive(false);

        amountRange = supplyType switch
        {
            SupplyType.CodeFragment => new Vector2(100, 200),
            SupplyType.Single => new Vector2(15, 20),
            SupplyType.SemiAuto => new Vector2(3, 8),
            SupplyType.Auto => new Vector2(10, 20),
            _ => new Vector2(0, 0)
        };

        actualAmount = Random.Range((int)amountRange.x, (int)amountRange.y + 1);
        UpdateVisual();
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.E))
        {
            Log("Intentando recoger el objeto");
            TrySuplyBox();
        }
    }

    private void TrySuplyBox()
    {
        Log("Recogiendo objeto");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        WeaponManager weaponManager = FindAnyObjectByType<WeaponManager>();
        if (weaponManager == null) return;
        int added = 0;

        switch (supplyType)
        {
            case SupplyType.Single:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.Single, actualAmount, out added);
                break;

            case SupplyType.SemiAuto:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.SemiAuto, actualAmount, out added);
                break;

            case SupplyType.Auto:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.Auto, actualAmount, out added);
                break;
        }

        Log($"Recogido {added} balas de tipo {supplyType}");

        actualAmount -= added;
        UpdateVisual();

        if (actualAmount <= 0)
        {
            if (autoRefill)
            {
                StartCoroutine(RefillAfterDelay());
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator RefillAfterDelay()
    {
        supplyCanvas.SetActive(false);
        yield return new WaitForSeconds(refillDelay);
        actualAmount = Random.Range((int)amountRange.x, (int)amountRange.y + 1);
        UpdateVisual();
    }
    /*
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
    */
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            playerInRange = true;
            supplyCanvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            playerInRange = false;
            supplyCanvas.gameObject.SetActive(false);
        }
    }


    private void UpdateVisual()
    {
        if (supplyCanvas != null && supplyAmountText != null)
        {
            if (supplyType == SupplyType.CodeFragment)
            {
                supplyAmountText.text = $"F. Cod. x{actualAmount}";
            }
            else if (supplyType == SupplyType.Single)
            {
                supplyAmountText.text = $"Pistola x{actualAmount}";
            }
            else if (supplyType == SupplyType.SemiAuto)
            {
                supplyAmountText.text = $"Escopeta x{actualAmount}";
            }
            else if (supplyType == SupplyType.Auto)
            {
                supplyAmountText.text = $"Rifle x{actualAmount}";
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, supplyRange);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}
