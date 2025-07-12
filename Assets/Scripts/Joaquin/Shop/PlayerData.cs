using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerData
{
    // Datos de GeneralUpgradeManager
    public int healthLevel;
    public int shieldLevel;

    // Datos de AbilityShopDataManager
    public List<string> purchasedAbilities;
    public List<string> equippedAbilityNames;
    public int lastEquippedIndex;

    // JsonUtility no puede serializar diccionarios directamente.
    // El truco es usar dos listas paralelas: una para las claves (nombres de habilidad)
    // y otra para los valores (sus estadísticas).
    public List<string> upgradeAbilityNames;
    public List<AbilityStats> upgradeAbilityStats;

    public PlayerData()
    {
        healthLevel = 0;
        shieldLevel = 0;
        purchasedAbilities = new List<string>();
        equippedAbilityNames = new List<string>();
        lastEquippedIndex = 0;
        upgradeAbilityNames = new List<string>();
        upgradeAbilityStats = new List<AbilityStats>();
    }
}
