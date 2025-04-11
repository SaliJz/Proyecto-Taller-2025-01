using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private TMPro.TMP_Text weaponNameText;
    [SerializeField] private UnityEngine.UI.Image weaponIconImage;

    private int currentIndex = 0;

    void Start()
    {
        EquipWeapon(0);
    }

    void Update()
    {
        // Con teclas numéricas
        if (Input.GetKeyDown(KeyCode.Alpha1)) EquipWeapon(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) EquipWeapon(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) EquipWeapon(2);

        // Con scroll del mouse
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            currentIndex = (currentIndex + 1) % weapons.Length;
            EquipWeapon(currentIndex);
        }
        else if (scroll < 0f)
        {
            currentIndex = (currentIndex - 1 + weapons.Length) % weapons.Length;
            EquipWeapon(currentIndex);
        }
    }

    void EquipWeapon(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
        Weapon currentWeapon = weapons[currentIndex];
        WeaponStats stats = currentWeapon.Stats;

        // Actualiza HUD
        HUDManager.Instance.UpdateAmmo(currentWeapon.currentAmmo, currentWeapon.totalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(stats.weaponName);
    }
}
