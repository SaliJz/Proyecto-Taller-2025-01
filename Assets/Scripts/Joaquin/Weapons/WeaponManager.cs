using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    [Header("Lógica de Armas")]
    [SerializeField] private Weapon[] weapons;

    [Header("Modelos Visuales")]
    [SerializeField] private GameObject[] weaponModels;

    private int currentIndex = 0;
    private Vector3[] originalModelPositions;

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

        for (int i = 0; i < weapons.Length; i++)
        {
            if (weapons[i] != null && weaponModels[i] != null)
            {
                weapons[i].Initialize(weaponModels[i].transform, originalModelPositions[i]);
            }
        }

        int savedIndex = PlayerPrefs.GetInt("LastWeaponIndex", 0);
        EquipWeaponInstant(savedIndex);
    }

    private void Update()
    {
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
        if (index == currentIndex) return;
        PlayerPrefs.SetInt("LastWeaponIndex", index);

        if (weapons[currentIndex].IsReloading)
        {
            weapons[currentIndex].CancelReload();
        }

        StartCoroutine(SwitchWeaponAnimation(index));
    }

    private IEnumerator SwitchWeaponAnimation(int newIndex)
    {
        GameObject oldWeaponModel = weaponModels[currentIndex];
        Transform oldModelTransform = oldWeaponModel.transform;
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

        GameObject newWeaponModel = weaponModels[currentIndex];
        Transform newModelTransform = newWeaponModel.transform;
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
        if (HUDManager.Instance != null)
        {
            HUDManager.Instance.UpdateAmmo(newWeapon.CurrentAmmo, newWeapon.TotalAmmo);
            HUDManager.Instance.UpdateWeaponIcon(newWeapon.Stats.weaponIcon);
            HUDManager.Instance.UpdateWeaponName(newWeapon.Stats.weaponName);
        }
    }

    public void EquipWeaponInstant(int index)
     {
        currentIndex = index;
        for (int i = 0; i < weapons.Length; i++)
        {
            bool isActive = (i == index);
            weapons[i].gameObject.SetActive(isActive);
            weaponModels[i].SetActive(isActive);

            if (isActive)
            {
                weaponModels[i].transform.localPosition = originalModelPositions[i];
            }
        }
        
        Weapon currentWeapon = weapons[currentIndex];
        HUDManager.Instance.UpdateAmmo(currentWeapon.CurrentAmmo, currentWeapon.TotalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(currentWeapon.Stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(currentWeapon.Stats.weaponName);
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
                        HUDManager.Instance.UpdateAmmo(weapon.CurrentAmmo, weapon.TotalAmmo);
                    }
                    return true;
                }
            }
        }

        return false;
    }
}
