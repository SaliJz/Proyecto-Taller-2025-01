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
        int savedIndex = PlayerPrefs.GetInt("LastWeaponIndex", 0);
        EquipWeaponInstant(savedIndex); // Equipar al inicio sin animación
    }

    void Update()
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

    void ChangeWeapon(int index)
    {
        if (index == currentIndex) return;
        PlayerPrefs.SetInt("LastWeaponIndex", index);

        StartCoroutine(SwitchWeaponAnimation(index));
    }

    IEnumerator SwitchWeaponAnimation(int newIndex)
    {
        Weapon oldWeapon = weapons[currentIndex];
        Transform oldModel = oldWeapon.weaponModelTransform;
        Vector3 oldOriginal = oldWeapon.originalLocalPosition;
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
        Transform newModel = newWeapon.weaponModelTransform;

        StartCoroutine(SlideUpWeapon(newModel, newWeapon.originalLocalPosition));

        // Actualizar HUD
        HUDManager.Instance.UpdateAmmo(newWeapon.currentAmmo, newWeapon.totalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(newWeapon.Stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(newWeapon.Stats.weaponName);
    }

    IEnumerator SlideUpWeapon(Transform model, Vector3 targetPosition)
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

    void EquipWeaponInstant(int index)
    {
        for (int i = 0; i < weapons.Length; i++)
        {
            weapons[i].gameObject.SetActive(i == index);
        }

        currentIndex = index;
        Weapon currentWeapon = weapons[currentIndex];

        // Asegura que el arma esté en su posición base desde el inicio
        currentWeapon.weaponModelTransform.localPosition = currentWeapon.originalLocalPosition;

        HUDManager.Instance.UpdateAmmo(currentWeapon.currentAmmo, currentWeapon.totalAmmo);
        HUDManager.Instance.UpdateWeaponIcon(currentWeapon.Stats.weaponIcon);
        HUDManager.Instance.UpdateWeaponName(currentWeapon.Stats.weaponName);
    }
}
