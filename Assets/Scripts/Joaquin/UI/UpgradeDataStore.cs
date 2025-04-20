using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpgradeDataStore : MonoBehaviour
{
    public static UpgradeDataStore Instance;

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
            LoadData();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SaveData()
    {
        PlayerPrefs.SetFloat("WeaponDamage", weaponDamageMultiplier);
        PlayerPrefs.SetFloat("WeaponFireRate", weaponFireRateMultiplier);
        PlayerPrefs.SetFloat("WeaponReloadSpeed", weaponReloadSpeedMultiplier);
        PlayerPrefs.SetInt("WeaponAmmoBonus", weaponAmmoBonus);

        PlayerPrefs.SetFloat("AbilityDamage", abilityDamageMultiplier);
        PlayerPrefs.SetFloat("AbilityDuration", abilityDurationMultiplier);
        PlayerPrefs.SetFloat("AbilityCooldown", abilityCooldownReduction);

        PlayerPrefs.SetInt("PlayerMaxHealth", playerMaxHealth);
        PlayerPrefs.SetFloat("PlayerDashCooldown", playerDashCooldown);

        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        weaponDamageMultiplier = PlayerPrefs.GetFloat("WeaponDamage", 1f);
        weaponFireRateMultiplier = PlayerPrefs.GetFloat("WeaponFireRate", 1f);
        weaponReloadSpeedMultiplier = PlayerPrefs.GetFloat("WeaponReloadSpeed", 1f);
        weaponAmmoBonus = PlayerPrefs.GetInt("WeaponAmmoBonus", 0);

        abilityDamageMultiplier = PlayerPrefs.GetFloat("AbilityDamage", 1f);
        abilityDurationMultiplier = PlayerPrefs.GetFloat("AbilityDuration", 1f);
        abilityCooldownReduction = PlayerPrefs.GetFloat("AbilityCooldown", 0f);

        playerMaxHealth = PlayerPrefs.GetInt("PlayerMaxHealth", 100);
        playerDashCooldown = PlayerPrefs.GetFloat("PlayerDashCooldown", 1f);
    }

    public void ResetAllUpgrades()
    {
        weaponDamageMultiplier = 1f;
        weaponFireRateMultiplier = 1f;
        weaponReloadSpeedMultiplier = 1f;
        weaponAmmoBonus = 0;

        abilityDamageMultiplier = 1f;
        abilityDurationMultiplier = 1f;
        abilityCooldownReduction = 0f;

        playerMaxHealth = 100;
        playerDashCooldown = 1f;

        PlayerPrefs.DeleteKey("WeaponDamage");
        PlayerPrefs.DeleteKey("WeaponFireRate");
        PlayerPrefs.DeleteKey("WeaponReloadSpeed");
        PlayerPrefs.DeleteKey("WeaponAmmoBonus");

        PlayerPrefs.DeleteKey("AbilityDamage");
        PlayerPrefs.DeleteKey("AbilityDuration");
        PlayerPrefs.DeleteKey("AbilityCooldown");

        PlayerPrefs.DeleteKey("PlayerMaxHealth");
        PlayerPrefs.DeleteKey("PlayerDashCooldown");

        PlayerPrefs.Save();
    }
}
