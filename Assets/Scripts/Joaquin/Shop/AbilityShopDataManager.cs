using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum AbilityType
{
    None,
    GlitchTime,
    ElectroHack,
    IgnitionCode,
    Mindjack
}

public class AbilityStats
{
    public int CooldownLevel;
    public int DurationLevel;
    public int DamageLevel;
    public int RangeLevel;
    public int EnemiesAffectedLevel;
}

public static class AbilityShopDataManager
{
    public static event Action OnAbilityShopDataChanged;

    private static readonly HashSet<string> PurchasedAbilities = new HashSet<string>();
    private static readonly Dictionary<string, AbilityStats> UpgradeLevels = new Dictionary<string, AbilityStats>();
    private static List<string> EquippedAbilityNames = new List<string>();
    private static int LastEquippedIndex = 0;

    static AbilityShopDataManager()
    {
        PurchasedAbilities = new HashSet<string>();
        UpgradeLevels = new Dictionary<string, AbilityStats>();
        EquippedAbilityNames = new List<string>();
        LastEquippedIndex = 0;
    }

    private static void EnsureAbilityExists(string abilityName)
    {
        if (!UpgradeLevels.ContainsKey(abilityName))
        {
            UpgradeLevels[abilityName] = new AbilityStats();
        }
    }

    public static void PurchaseItems(List<string> abilityNames, Dictionary<string, List<string>> upgrades)
    {
        foreach (var name in abilityNames)
        {
            if (!IsPurchased(name))
            {
                PurchasedAbilities.Add(name);
                EnsureAbilityExists(name);
            }
        }

        foreach (var pair in upgrades)
        {
            string abilityName = pair.Key;
            List<string> statsToUpgrade = pair.Value;
            EnsureAbilityExists(abilityName);

            AbilityStats stats = UpgradeLevels[abilityName];
            foreach (var statName in statsToUpgrade)
            {
                switch (statName)
                {
                    case "Cooldown": stats.CooldownLevel++; break;
                    case "Duration": stats.DurationLevel++; break;
                    case "Damage": stats.DamageLevel++; break;
                    case "Range": stats.RangeLevel++; break;
                    case "EnemiesAffected": stats.EnemiesAffectedLevel++; break;
                }
            }
        }
        NotifyDataChanged();
    }
    public static bool IsPurchased(string abilityName) => PurchasedAbilities.Contains(abilityName);

    public static void SavePlayerEquippedState(List<string> equippedNames, int currentIndex)
    {
        EquippedAbilityNames = new List<string>(equippedNames);
        LastEquippedIndex = currentIndex;
    }

    public static List<string> GetSavedEquippedAbilities()
    {
        return EquippedAbilityNames;
    }

    public static int GetSavedEquippedIndex()
    {
        return LastEquippedIndex;
    }

    public static AbilityStats GetStats(string abilityName)
    {
        EnsureAbilityExists(abilityName);
        return UpgradeLevels[abilityName];
    }

    public static void NotifyDataChanged() => OnAbilityShopDataChanged?.Invoke();

    public static void ResetData()
    {
        PurchasedAbilities.Clear();
        UpgradeLevels.Clear();
        EquippedAbilityNames.Clear();
        LastEquippedIndex = 0;
        NotifyDataChanged();
    }
}