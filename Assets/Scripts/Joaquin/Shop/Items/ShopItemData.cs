using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ItemType { Ability, Health, Shield, Weapon }

public abstract class ShopItemData : ScriptableObject
{
    [Header("Configuraci�n General del �tem")]
    public string itemName; // Nombre exacto que lo identifica (ej: 'GlitchTime', 'Health')
    public Sprite itemIcon;
    [TextArea] public string description;
    public ItemType itemType;

    [Header("Configuraci�n de Costo y Niveles")]
    public int purchaseCost = 100; // Costo inicial si es una compra (solo para habilidades si no son gratuitas al inicio)
    public int baseUpgradeCost = 50;
    public int costIncreasePerLevel = 25;
    public int maxLevel = 10;

    // M�todos para obtener informaci�n, que ser�n implementados por clases derivadas
    public abstract int GetCurrentLevel(string specificItemName = null);
    public abstract void IncrementLevel(string specificItemName = null);
    public abstract bool IsPurchased(string specificItemName = null); // Para habilidades que se compran primero

    // Calcula el costo actual del �tem
    public int CalculateCurrentCost(string specificItemName = null)
    {
        int currentLevel = GetCurrentLevel(specificItemName);
        bool isPurchased = IsPurchased(specificItemName);

        if (itemType == ItemType.Ability && !isPurchased)
        {
            return purchaseCost;
        }
        else
        {
            return baseUpgradeCost + (currentLevel * costIncreasePerLevel);
        }
    }

    public bool IsMaxLevel(string specificItemName = null)
    {
        return GetCurrentLevel(specificItemName) >= maxLevel;
    }
}
