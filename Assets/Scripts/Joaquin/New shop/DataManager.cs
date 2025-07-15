using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[System.Serializable]
public class AbilityStatsData
{
    public int Level = 0;
}

[System.Serializable]
public class WeaponStatsData
{
    public int Level = 0;
}

[System.Serializable]
public class AbilitySaveData
{
    public string Name;
    public AbilityStatsData Stats;
}

[System.Serializable]
public class WeaponSaveData
{
    public string Name;
    public WeaponStatsData Stats;
}

public static class DataManager
{
    public static event Action OnDataChanged;
    public static event Action OnPlayerDataLoaded;

    // Habilidades
    private static HashSet<string> purchasedAbilities = new HashSet<string>();
    private static Dictionary<string, AbilityStatsData> abilityUpgradeLevels = new Dictionary<string, AbilityStatsData>();
    private static List<string> currentlyEquippedAbilityNames = new List<string>();
    private static int lastEquippedAbilityIndex = 0;

    // Vida y escudo
    public static int HealthUpgradeLevel { get; private set; }
    public static int ShieldUpgradeLevel { get; private set; }

    // Armas
    private static Dictionary<string, WeaponStatsData> weaponUpgradeLevels = new Dictionary<string, WeaponStatsData>();

    // --- MÉTODOS DE ARMAS ---

    private static void EnsureWeaponExists(string weaponName)
    {
        if (!weaponUpgradeLevels.ContainsKey(weaponName))
        {
            weaponUpgradeLevels[weaponName] = new WeaponStatsData();
        }
    }

    // Nuevo método para incrementar el nivel global de un arma
    public static void IncrementWeaponLevel(string weaponName)
    {
        EnsureWeaponExists(weaponName); // Asegurarse de que el arma existe
        weaponUpgradeLevels[weaponName].Level++;
        NotifyDataChanged();
    }

    // Nuevo método para obtener las estadísticas del arma (con el nivel global)
    public static WeaponStatsData GetWeaponStats(string weaponName)
    {
        EnsureWeaponExists(weaponName);
        return weaponUpgradeLevels[weaponName];
    }

    public static Dictionary<string, WeaponStatsData> GetAllWeaponStats() => weaponUpgradeLevels;

    // Obtener el nivel global de un arma
    public static int GetWeaponLevel(string weaponName)
    {
        if (weaponUpgradeLevels.TryGetValue(weaponName, out var stats))
        {
            return stats.Level;
        }
        return 0;
    }

    // --- MÉTODOS DE HABILIDADES ---

    private static void EnsureAbilityExists(string abilityName)
    {
        if (!abilityUpgradeLevels.ContainsKey(abilityName))
        {
            abilityUpgradeLevels[abilityName] = new AbilityStatsData();
        }
    }

    public static void PurchaseAbility(string name)
    {
        if (!purchasedAbilities.Contains(name))
        {
            purchasedAbilities.Add(name);
            EnsureAbilityExists(name); // Asegura que los stats existen y se inicializan
            NotifyDataChanged();
        }
    }

    // Incrementar el nivel global de una habilidad
    public static void IncrementAbilityLevel(string name)
    {
        EnsureAbilityExists(name);
        abilityUpgradeLevels[name].Level++;
        NotifyDataChanged();
    }

    public static bool IsAbilityPurchased(string name) => purchasedAbilities.Contains(name);

    public static HashSet<string> GetPurchasedAbilities() => purchasedAbilities;

    public static Dictionary<string, AbilityStatsData> GetAllAbilityStats() => abilityUpgradeLevels;

    public static int GetSavedEquippedAbilityIndex() // Renombrado
    {
        return lastEquippedAbilityIndex;
    }

    public static List<string> GetSavedEquippedAbilityNames() // Nuevo método para obtener solo los nombres
    {
        return currentlyEquippedAbilityNames;
    }

    public static AbilityStatsData GetAbilityStats(string abilityName) // Renombrado para consistencia
    {
        EnsureAbilityExists(abilityName);
        return abilityUpgradeLevels[abilityName];
    }

    // Obtener el nivel global de una habilidad
    public static int GetAbilityLevel(string abilityName)
    {
        if (abilityUpgradeLevels.TryGetValue(abilityName, out var stats))
        {
            return stats.Level;
        }
        return 0;
    }

    public static void SavePlayerEquippedState(List<string> equippedNames, int currentIndex) // Recibe nombres de habilidades equipadas
    {
        currentlyEquippedAbilityNames = equippedNames;
        lastEquippedAbilityIndex = currentIndex;
        SaveLoadManager.SaveGame(); // Asumo que SaveGame lee de DataManager
    }

    // --- MÉTODOS DE MEJORAS GENERALES (VIDA/ESCUDO) ---

    public static void IncrementHealthLevel()
    {
        HealthUpgradeLevel++;
        NotifyDataChanged();
    }

    public static void IncrementShieldLevel()
    {
        ShieldUpgradeLevel++;
        NotifyDataChanged();
    }

    // --- CARGA Y NOTIFICACIÓN ---

    public static void LoadData(PlayerData data)
    {
        // Cargar datos de mejoras generales
        HealthUpgradeLevel = data.healthLevel;
        ShieldUpgradeLevel = data.shieldLevel;

        // Cargar datos de habilidades
        purchasedAbilities = new HashSet<string>(data.purchasedAbilities ?? new List<string>());
        abilityUpgradeLevels = data.GetAbilityUpgradesDict();
        currentlyEquippedAbilityNames = data.equippedAbilityNames ?? new List<string>();
        lastEquippedAbilityIndex = GetSavedEquippedAbilityIndex();

        // Cargar datos de armas
        weaponUpgradeLevels = data.GetWeaponUpgradesDict();

        OnDataChanged?.Invoke();
        OnPlayerDataLoaded?.Invoke();
    }

    public static void NotifyDataChanged()
    {
        OnDataChanged?.Invoke();
    }

    public static void ResetData()
    {
        // 1) Limpia todos los datos en memoria
        purchasedAbilities.Clear();
        abilityUpgradeLevels.Clear();
        weaponUpgradeLevels.Clear();
        HealthUpgradeLevel = 0;
        ShieldUpgradeLevel = 0;

        // 2) Limpia lo que había guardado de equipamiento
        currentlyEquippedAbilityNames.Clear();
        lastEquippedAbilityIndex = 0;

        // 3) Borra el json para que LoadGame() no lo recupere
        SaveLoadManager.DeleteSaveFile();

        // 4) Notifica a todo el mundo
        NotifyDataChanged();
        OnPlayerDataLoaded?.Invoke();

        Debug.Log("Datos reiniciados y archivo de guardado eliminado.");
    }
}