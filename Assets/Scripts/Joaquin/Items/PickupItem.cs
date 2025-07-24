using TMPro;
using UnityEngine;

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
    //[SerializeField] private GameObject pickupCanvas;
    //[SerializeField] private TextMeshProUGUI pickupAmountText;
    [SerializeField] private float pickUpRange = 10;
    [SerializeField] private float flySpeed = 10f;
    [SerializeField] private float rotationSpeed = 10f;
    [SerializeField] private float positionOffset = 1f;

    [SerializeField] private float lifetime = 60f;

    private int actualAmount;
    [SerializeField] private bool playerInRange = false;
    private bool isFlyingToPlayer = false;
    private Transform playerTarget;
    private Rigidbody rb;
    private WeaponManager weaponManager;

    private void Awake()
    {

    }
    private void Start()
    {
        weaponManager = FindObjectOfType<WeaponManager>();

        Invoke(nameof(DestroyPickup), lifetime);

        amountRange = pickupType switch
        {
            PickupType.CodeFragment => new Vector2(100, 200),
            PickupType.AmmoSingle => new Vector2(15, 20),
            PickupType.AmmoSemiAuto => new Vector2(3, 8),
            PickupType.AmmoAuto => new Vector2(10, 20),
            _ => new Vector2(0, 0)
        };

        // Cambia aquí: si el TutorialManager0 existe y el tipo es CodeFragment, pon 100
        if (pickupType == PickupType.CodeFragment && TutorialManager0.Instance != null)
        {
            actualAmount = 100;
        }
        else
        {
            actualAmount = Random.Range((int)amountRange.x, (int)amountRange.y + 1);
        }

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
        else // Lógica para munición
        {
            if (playerTarget != null && !isFlyingToPlayer)
            {
                float distance = Vector3.Distance(transform.position, playerTarget.position);
                if (distance <= pickUpRange)
                {
                    // Comprueba si el jugador necesita este tipo de munición
                    if (NeedsThisAmmoType())
                    {
                        isFlyingToPlayer = true;
                        if (rb != null) rb.isKinematic = true;
                    }
                }
            }

            if (isFlyingToPlayer)
            {
                FollowAndPickupAmmo();
            }
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
            Debug.Log("Fragmento de código recogido");
            HUDManager.Instance.AddInfoFragment(actualAmount);
            Destroy(gameObject);
        }
    }

    private void TryPickup()
    {
        Debug.Log("Recogiendo objeto");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;

        WeaponManager weaponManager = FindAnyObjectByType<WeaponManager>();
        if (weaponManager == null) return;

        int added = 0;

        switch (pickupType)
        {
            case PickupType.AmmoSingle:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.Single, actualAmount, out added);
                break;

            case PickupType.AmmoSemiAuto:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.SemiAuto, actualAmount, out added);
                break;

            case PickupType.AmmoAuto:
                weaponManager.TryAddAmmoToWeapon(Weapon.ShootingMode.Auto, actualAmount, out added);
                break;
        }

        Debug.Log($"Recogido {added} balas de tipo {pickupType}");

        Destroy(gameObject);
    }

    private bool NeedsThisAmmoType()
    {
        if (weaponManager == null) return false;

        return pickupType switch
        {
            PickupType.AmmoSingle => weaponManager.NeedsAmmo(Weapon.ShootingMode.Single),
            PickupType.AmmoSemiAuto => weaponManager.NeedsAmmo(Weapon.ShootingMode.SemiAuto),
            PickupType.AmmoAuto => weaponManager.NeedsAmmo(Weapon.ShootingMode.Auto),
            _ => false,
        };
    }

    private void FollowAndPickupAmmo()
    {
        if (playerTarget == null) return;

        Vector3 targetPosition = playerTarget.position + Vector3.up * positionOffset;
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, flySpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) < 0.2f)
        {
            if (!NeedsThisAmmoType())
            {
                Debug.Log($"Ya no necesita munición de tipo {pickupType}, destruyendo objeto");
                Destroy(gameObject);
                return;
            }

            TryPickup();
            // El objeto se destruirá dentro de TryPickup
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PlayerHealth>() != null)
        {
            if (pickupType != PickupType.CodeFragment)
            {
                playerInRange = true;
                playerTarget = other.transform;
                Debug.Log("Jugador dentro del rango de recogida.");
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (pickupType != PickupType.CodeFragment)
            {
                playerInRange = false;
                playerTarget = null;
                isFlyingToPlayer = false;
                if (rb != null) rb.isKinematic = false;
                Debug.Log("Jugador fuera del rango de recogida.");
            }
        }
    }

    //private void UpdateVisual()
    //{
    //    if (pickupCanvas != null && pickupAmountText != null)
    //    {
    //        if (pickupType == PickupType.CodeFragment)
    //        {
    //            pickupAmountText.text = $"F. Cod. x{actualAmount}";
    //        }
    //        else if (pickupType == PickupType.AmmoSingle)
    //        {
    //            pickupAmountText.text = $"Pistola x{actualAmount}";
    //        }
    //        else if (pickupType == PickupType.AmmoSemiAuto)
    //        {
    //            pickupAmountText.text = $"Escopeta x{actualAmount}";
    //        }
    //        else if (pickupType == PickupType.AmmoAuto)
    //        {
    //            pickupAmountText.text = $"Rifle x{actualAmount}";
    //        }
    //    }
    //}
    private void DestroyPickup()
    {
        Destroy(gameObject);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, pickUpRange);
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 2f);
    }
}