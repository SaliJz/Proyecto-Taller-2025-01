using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    public int healthLevel;
    public int shieldLevel;

    public List<string> purchasedAbilities;
    public List<string> equippedAbilityNames;
    public int lastEquippedIndex;

    public List<string> upgradeAbilityNames;
    public List<AbilityStatsData> upgradeAbilityStats;

    public List<string> weaponUpgradeTypes;
    public List<WeaponStatsData> weaponUpgradeStats;

    public PlayerData()
    {
        healthLevel = 0;
        shieldLevel = 0;
        purchasedAbilities = new List<string>();
        equippedAbilityNames = new List<string>();

        lastEquippedIndex = 0;

        upgradeAbilityNames = new List<string>();
        upgradeAbilityStats = new List<AbilityStatsData>();

        weaponUpgradeTypes = new List<string>();
        weaponUpgradeStats = new List<WeaponStatsData>();
    }

    public Dictionary<string, AbilityStatsData> GetAbilityUpgradesDict()
    {
        var dict = new Dictionary<string, AbilityStatsData>();
        int count = Mathf.Min(upgradeAbilityNames.Count, upgradeAbilityStats.Count);
        for (int i = 0; i < count; i++)
        {
            var name = upgradeAbilityNames[i];
            if (!dict.ContainsKey(name))
                dict[name] = new AbilityStatsData { Level = upgradeAbilityStats[i].Level };
        }
        return dict;
    }

    public Dictionary<string, WeaponStatsData> GetWeaponUpgradesDict()
    {
        var dict = new Dictionary<string, WeaponStatsData>();
        int count = Mathf.Min(weaponUpgradeTypes.Count, weaponUpgradeStats.Count);
        for (int i = 0; i < count; i++)
        {
            var name = weaponUpgradeTypes[i];
            if (!dict.ContainsKey(name))
                dict[name] = new WeaponStatsData { Level = weaponUpgradeStats[i].Level };
        }
        return dict;
    }
}