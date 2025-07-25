using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewHealthUpgradeItem", menuName = "Shop/Health Upgrade Item")]
public class HealthUpgradeItemData : ShopItemData
{
    private void OnEnable()
    {
        itemType = ItemType.Health;
    }

    public override int GetCurrentLevel(string specificItemName = null)
    {
        return DataManager.HealthUpgradeLevel;
    }

    public override void IncrementLevel(string specificItemName = null)
    {
        DataManager.IncrementHealthLevel();
    }

    public override bool IsPurchased(string specificItemName = null)
    {
        // Las mejoras de vida no tienen un estado de "comprado" inicial
        return true;
    }
}
