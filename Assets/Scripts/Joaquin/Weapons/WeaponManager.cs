using System.Collections;
using UnityEngine;


public class WeaponManager : MonoBehaviour
{
    [Header("Lógica de Armas")]
    [SerializeField] private Weapon[] weapons;

    [Header("Modelos Visuales")]
    [SerializeField] private GameObject[] weaponModels;

    [Header("Configuración de Animación por codigo")]
    [SerializeField] private bool useProceduralAnimations = true;
    [SerializeField] private bool useAnimatorAnimations;

    [Header("Configuración de control de cambio")]
    [SerializeField] public bool canChangeWeapon = true;
    [SerializeField] private bool canEquipFirstWeapon = true;

    private int currentIndex = 0;
    private Vector3[] originalModelPositions;

    public bool CanChangeWeapon
    {
        get => canChangeWeapon;
        set => canChangeWeapon = value;
    }

    public bool CanEquipFirstWeapon
    {
        get => canEquipFirstWeapon;
        set => canEquipFirstWeapon = value;
    }

    private void Start()
    {
        originalModelPositions = new Vector3[weaponModels.Length];
        for (int i = 0; i < weaponModels.Length; i++)
        {
            if (weaponModels[i] != null)
            {
                originalModelPositions[i] = weaponModels[i].transform.localPosition;
            }
        }

        int savedIndex = PlayerPrefs.GetInt("LastWeaponIndex", 0);

        if (canEquipFirstWeapon)
        {
            EquipWeaponInstant(savedIndex);
        }
    }

    private void Update()
    {
        if (!canChangeWeapon) return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) ChangeWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) ChangeWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) ChangeWeapon(2);

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            ChangeWeapon((currentIndex + 1) % weapons.Length);
        }
        else if (scroll < 0f)
        {
            ChangeWeapon((currentIndex - 1 + weapons.Length) % weapons.Length);
        }
    }

    private void ChangeWeapon(int index)
    {
        if (!canChangeWeapon || index == currentIndex || index < 0 || index >= weapons.Length) return;

        PlayerPrefs.SetInt("LastWeaponIndex", index);

        if (weapons[currentIndex].IsReloading)
        {
            weapons[currentIndex].CancelReload();
        }

        if (useProceduralAnimations)
        {
            StartCoroutine(SwitchWeaponProceduralAnimation(index));
        }
        else if (useAnimatorAnimations)
        {
            StartCoroutine(SwitchWeaponAnimatorAnimation(index));
        }
        else
        {
            EquipWeaponInstant(index);
        }
    }

    private IEnumerator SwitchWeaponAnimatorAnimation(int newIndex)
    {
        canChangeWeapon = false;

        PlayerAnimatorController.Instance?.PlaySwitchWeaponAnim(newIndex); // establece ID y bool

        yield return new WaitForSeconds(0.1f); // da tiempo a que el Animator evalúe el blend

        weapons[currentIndex].gameObject.SetActive(false);
        weaponModels[currentIndex].SetActive(false);

        currentIndex = newIndex;

        weapons[currentIndex].gameObject.SetActive(true);
        weaponModels[currentIndex].SetActive(true);

        yield return new WaitForSeconds(0.1f);

        //PlayerAnimatorController.Instance?.StopSwitchWeaponAnim(); // método que desactiva el bool

        Weapon newWeapon = weapons[currentIndex];
        UpdateHUD(newWeapon);

        canChangeWeapon = true;
    }

    private IEnumerator SwitchWeaponProceduralAnimation(int newIndex)
    {
        canChangeWeapon = false;

        Transform oldModelTransform = weaponModels[currentIndex].transform;
        Vector3 oldOriginalPos = originalModelPositions[currentIndex];
        Vector3 oldDownPos = oldOriginalPos + new Vector3(0, -0.2f, 0);

        float t = 0f;
        float duration = 0.15f;

        while (t < duration)
        {
            oldModelTransform.localPosition = Vector3.Lerp(oldOriginalPos, oldDownPos, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        weapons[currentIndex].gameObject.SetActive(false);
        weaponModels[currentIndex].SetActive(false);

        currentIndex = newIndex;

        weapons[currentIndex].gameObject.SetActive(true);
        weaponModels[currentIndex].SetActive(true);

        Transform newModelTransform = weaponModels[currentIndex].transform;
        Vector3 newOriginalPos = originalModelPositions[currentIndex];
        Vector3 newStartPosition = newOriginalPos + new Vector3(0, -0.2f, 0);
        newModelTransform.localPosition = newStartPosition;

        t = 0f;

        while (t < duration)
        {
            newModelTransform.localPosition = Vector3.Lerp(newStartPosition, newOriginalPos, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        newModelTransform.localPosition = newOriginalPos;

        Weapon newWeapon = weapons[currentIndex];
        UpdateHUD(newWeapon);

        canChangeWeapon = true;
    }

    public void EquipWeaponInstant(int index)
    {
        if (index < 0 || index >= weapons.Length) return;

        weapons[currentIndex].gameObject.SetActive(false);
        weaponModels[currentIndex].SetActive(false);

        currentIndex = index;
        weapons[currentIndex].gameObject.SetActive(true);
        weaponModels[currentIndex].SetActive(true);

        if (weaponModels[currentIndex] != null)
        {
            weaponModels[currentIndex].transform.localPosition = originalModelPositions[currentIndex];
        }

        Weapon currentWeapon = weapons[currentIndex];
        UpdateHUD(currentWeapon);
    }

    private void UpdateHUD(Weapon newWeapon)
    {
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateAmmo(currentIndex, newWeapon.CurrentAmmo, newWeapon.TotalAmmo);
            HUDManager.Instance.UpdateWeaponIcon(newWeapon.Stats.weaponIcon);
            HUDManager.Instance.UpdateWeaponName(newWeapon.Stats.weaponName);
        }
    }

    public bool NeedsAmmo(Weapon.ShootingMode mode)
    {
        foreach (Weapon weapon in weapons)
        {
            if (weapon != null && weapon.BaseMode == mode)
            {
                return weapon.TotalAmmo < (weapon.Stats.totalAmmo + UpgradeDataStore.Instance.weaponAmmoBonus);
            }
        }
        return false;
    }

    public bool TryAddAmmoToWeapon(Weapon.ShootingMode mode, int amountToAdd, out int amountActuallyAdded)
    {
        amountActuallyAdded = 0;

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null && weapon.BaseMode == mode)
            {
                if (weapon.TryAddAmmo(amountToAdd, out int added))
                {
                    amountActuallyAdded = added;

                    if (weapon == weapons[currentIndex])
                    {
                        HUDManager.Instance.UpdateAmmo(currentIndex, weapon.CurrentAmmo, weapon.TotalAmmo);
                    }
                    return true;
                }
            }
        }

        return false;
    }
}
