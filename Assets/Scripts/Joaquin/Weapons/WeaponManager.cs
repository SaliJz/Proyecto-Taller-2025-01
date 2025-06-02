using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class WeaponManager : MonoBehaviour
{
    [SerializeField] private Weapon[] weapons;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private TMP_Text weaponNameText;
    [SerializeField] private Image weaponIconImage;

    private int currentIndex = 0;

    private void Start()
    {
        int savedIndex = PlayerPrefs.GetInt("LastWeaponIndex", 0);
        EquipWeaponInstant(savedIndex); // Equipar al inicio sin animación
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
        Weapon oldWeapon = weapons[currentIndex];
        Transform oldModel = oldWeapon.WeaponModelTransform;
        Vector3 oldOriginal = oldWeapon.OriginalLocalPosition;
        Vector3 oldDown = oldOriginal + new Vector3(0, -0.2f, 0);
        float t = 0f;
        float duration = 0.15f;

        // Bajada del arma actual
        while (t < duration)
        {
            oldModel.localPosition = Vector3.Lerp(oldOriginal, oldDown, t / duration);
            t += Time.deltaTime;
            yield return null;
        }

        oldWeapon.gameObject.SetActive(false);

        // Activar nueva arma
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == newIndex);
        }

        currentIndex = newIndex;
        Weapon newWeapon = weapons[currentIndex];
        Transform newModel = newWeapon.WeaponModelTransform;

        StartCoroutine(SlideUpWeapon(newModel, newWeapon.OriginalLocalPosition));

        // Actualizar HUD
        HUDManager.Instance.UpdateAmmo(newWeapon.CurrentAmmo, newWeapon.TotalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(newWeapon.Stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(newWeapon.Stats.weaponName);
    }

    private IEnumerator SlideUpWeapon(Transform model, Vector3 targetPosition)
    {
        Vector3 startPos = model.localPosition;
        float duration = 0.15f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            model.localPosition = Vector3.Lerp(startPos, targetPosition, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        model.localPosition = targetPosition;
    }

    private void EquipWeaponInstant(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
        Weapon currentWeapon = weapons[currentIndex];

        // Asegura que el arma esté en su posición base desde el inicio
        currentWeapon.WeaponModelTransform.localPosition = currentWeapon.OriginalLocalPosition;

        HUDManager.Instance.UpdateAmmo(currentWeapon.CurrentAmmo, currentWeapon.TotalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(currentWeapon.Stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(currentWeapon.Stats.weaponName);
    }

    // Método para obtener el índice del arma actual
    public bool TryAddAmmoToWeapon(Weapon.ShootingMode mode, int amountToAdd, out int amountActuallyAdded)
    {
        amountActuallyAdded = 0;

        foreach (Weapon weapon in weapons)
        {
            if (weapon != null && weapon.CurrentMode == mode)
            {
                if (weapon.TryAddAmmo(amountToAdd, out int added))
                {
                    amountActuallyAdded = added;

                    // Solo actualiza HUD si es el arma actualmente equipada
                    if (weapon == weapons[currentIndex])
                    {
                        HUDManager.Instance.UpdateAmmo(weapon.CurrentAmmo, weapon.TotalAmmo);
                    }
                    return true; // Devuelve verdadero si la munición se agregó con éxito
                }
            }
        }

        return false; // Devuelve falso si no hay armas coincidentes o no se pudo agregar munición
    }
}
