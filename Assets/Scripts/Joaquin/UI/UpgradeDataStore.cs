using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDataStore : MonoBehaviour
{
    public static UpgradeDataStore Instance;

    [Header("Configuración de las mejoras")]
    // Multiplicadores de daño, velocidad de disparo, velocidad de recarga y bonificación de munición
    [Header("Armas")]
    public float weaponDamageMultiplier = 1f;
    public float weaponFireRateMultiplier = 1f;
    public float weaponReloadSpeedMultiplier = 1f;
    public int weaponAmmoBonus = 0;

    // Multiplicadores de daño, duración y reducción de tiempo de recarga
    [Header("Habilidades")]
    public float abilityDamageMultiplier = 1f;
    public float abilityDurationMultiplier = 1f;
    public float abilityCooldownReduction = 0f;
    public float abilityRangeMultiplier = 1f;

    // Salud máxima y tiempo de recarga de la habilidad de dash
    [Header("Jugador")]
    public int playerMaxHealth = 100;
    public float playerDashCooldown = 1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ApplyUpgrade(PassiveUpgradeStats upgrade)
    {
        float value = upgrade.isPercentage
            ? Random.Range(upgrade.valueRange.x, upgrade.valueRange.y)
            : Mathf.RoundToInt(Random.Range(upgrade.valueRange.x, upgrade.valueRange.y));

        switch (upgrade.upgradeType)
        {
            case PassiveUpgradeStats.UpgradeType.WeaponDamage:
                weaponDamageMultiplier += value;
                break;
            case PassiveUpgradeStats.UpgradeType.FireRate:
                weaponFireRateMultiplier += value;
                break;
            case PassiveUpgradeStats.UpgradeType.ReloadSpeed:
                weaponReloadSpeedMultiplier += value;
                break;
            case PassiveUpgradeStats.UpgradeType.AmmoBonus:
                weaponAmmoBonus += Mathf.RoundToInt(value);
                break;
        }

        Debug.Log($"Aplicada mejora: {upgrade.upgradeName} (+{value})");
        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (var weapon in weapons)
        {
            weapon.ApplyPassiveUpgrades();
            Debug.Log($"Arma actualizada: {weapon.name}");
        }
    }

    public void ResetTemporaryUpgrades()
    {
        weaponDamageMultiplier = 1f;
        weaponFireRateMultiplier = 1f;
        weaponReloadSpeedMultiplier = 1f;
        weaponAmmoBonus = 0;

        abilityDamageMultiplier = 1f;
        abilityDurationMultiplier = 1f;
        abilityCooldownReduction = 0f;
        abilityRangeMultiplier = 1f;

        playerMaxHealth = 100;
        playerDashCooldown = 1f;

        Weapon[] weapons = FindObjectsOfType<Weapon>();
        foreach (var weapon in weapons)
        {
            weapon.ApplyPassiveUpgrades();
            Debug.Log($"Arma actualizada: {weapon.name}");
        }
    }
}
