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

    private static readonly HashSet<AbilityType> PurchasedAbilities = new HashSet<AbilityType>();
    private static readonly List<AbilityType> EquippedAbilities = new List<AbilityType>();
    private static readonly Dictionary<AbilityType, AbilityStats> UpgradeLevels = new Dictionary<AbilityType, AbilityStats>();

    #region Initialization
    public static void Initialize()
    {
        foreach (AbilityType type in Enum.GetValues(typeof(AbilityType)))
        {
            if (type != AbilityType.None && !UpgradeLevels.ContainsKey(type))
            {
                UpgradeLevels[type] = new AbilityStats();
            }
        }
    }

    #endregion

    #region buy and equip logic

    public static void PurchaseAbilities(IEnumerable<AbilityType> abilitiesToBuy)
    {
        foreach (var ability in abilitiesToBuy)
        {
            if (!IsPurchased(ability)) PurchasedAbilities.Add(ability);
        }
        NotifyDataChanged();
    }

    public static void EquipAbility(AbilityType type)
    {
        if (!IsPurchased(type) || IsEquipped(type)) return;
        if (EquippedAbilities.Count >= 2) EquippedAbilities.RemoveAt(0);
        EquippedAbilities.Add(type);
        NotifyDataChanged();
    }

    public static void UnequipAbility(AbilityType type)
    {
        if (EquippedAbilities.Remove(type))
        {
            NotifyDataChanged();
        }
    }

    #endregion

    #region Upgrade logic

    public static void UpgradeStats(Dictionary<AbilityType, List<string>> upgrades)
    {
        foreach (var pair in upgrades)
        {
            if (!UpgradeLevels.ContainsKey(pair.Key)) continue;
            AbilityStats stats = UpgradeLevels[pair.Key];
            foreach (var statName in pair.Value)
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

    #endregion

    #region Getters and Checkers

    public static bool IsPurchased(AbilityType type) => PurchasedAbilities.Contains(type);
    public static bool IsEquipped(AbilityType type) => EquippedAbilities.Contains(type);
    public static List<AbilityType> GetEquippedAbilities() => new List<AbilityType>(EquippedAbilities);
    public static AbilityStats GetStats(AbilityType type) => UpgradeLevels[type];


    #endregion

    #region utility methods

    public static void NotifyDataChanged() => OnAbilityShopDataChanged?.Invoke();

    public static void ResetDataForNewLevel()
    {
        PurchasedAbilities.Clear();
        EquippedAbilities.Clear();
        UpgradeLevels.Clear();
        Initialize();
        NotifyDataChanged();
    }

    #endregion
}