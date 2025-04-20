using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CommandStationUI : MonoBehaviour
{
    [SerializeField] private GameObject canvas;
    [SerializeField] private TextMeshProUGUI fragmentText;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        UpdateFragmentsUI();
    }

    public void ApplyUpgrade(string upgradeID)
    {
        int price = GetPriceForUpgrade(upgradeID);

        if (HUDManager.Instance.CurrentFragments >= price)
        {
            HUDManager.Instance.SpendFragments(price);

            switch (upgradeID)
            {
                case "BulletDamage":
                    UpgradeDataStore.Instance.abilityDamageMultiplier += 0.2f;
                    break;
                case "FireRate":
                    UpgradeDataStore.Instance.weaponFireRateMultiplier += 0.2f;
                    break;
                case "ReloadSpeed":
                    UpgradeDataStore.Instance.weaponReloadSpeedMultiplier += 0.15f;
                    break;
                case "AmmoBonus":
                    UpgradeDataStore.Instance.weaponAmmoBonus += 20;
                    break;
                case "DashCooldown":
                    UpgradeDataStore.Instance.playerDashCooldown -= 0.2f;
                    break;
                case "MaxHealth":
                    UpgradeDataStore.Instance.playerMaxHealth += 20;
                    break;
                case "AbilityCooldown":
                    UpgradeDataStore.Instance.abilityCooldownReduction += 0.1f;
                    break;
            }

            UpdateFragmentsUI();
        }
        else
        {
            Debug.Log("No hay suficientes fragmentos.");
        }
    }

    public void ExitStation()
    {
        Time.timeScale = 1f;
        canvas.SetActive(false);
    }

    private int GetPriceForUpgrade(string upgradeID)
    {
        return 100; // Puedes hacer esto dinámico si quieres
    }

    private void UpdateFragmentsUI()
    {
        fragmentText.text = "Fragments: " + HUDManager.Instance.CurrentFragments;
    }
}
