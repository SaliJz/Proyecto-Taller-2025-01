using System.Collections;
using TMPro;
using UnityEngine;

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
    private WeaponManager weaponManager;

    private void Start()
    {
        weaponManager = FindAnyObjectByType<WeaponManager>();

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
            TrySuplyBox();
        }
    }

    private void TrySuplyBox()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

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
        if (supplyCanvas == null || supplyAmountText == null) return;

        string textToShow = "";
        switch (supplyType)
        {
            case SupplyType.CodeFragment:
                textToShow = $"F. Cod. x{actualAmount}";
                break;
            case SupplyType.Single:
                textToShow = $"Pistola x{actualAmount}";
                break;
            case SupplyType.SemiAuto:
                textToShow = $"Escopeta x{actualAmount}";
                break;
            case SupplyType.Auto:
                textToShow = $"Rifle x{actualAmount}";
                break;
        }
        supplyAmountText.text = textToShow;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, supplyRange);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}
