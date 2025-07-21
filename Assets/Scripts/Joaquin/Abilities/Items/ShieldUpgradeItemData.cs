using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewShieldUpgradeItem", menuName = "Shop/Shield Upgrade Item")]
public class ShieldUpgradeItemData : ShopItemData
{
    private void OnEnable()
    {
        itemType = ItemType.Shield;
    }

    public override int GetCurrentLevel(string specificItemName = null)
    {
        return DataManager.ShieldUpgradeLevel;
    }

    public override void IncrementLevel(string specificItemName = null)
    {
        DataManager.IncrementShieldLevel();
    }

    public override bool IsPurchased(string specificItemName = null)
    {
        // Las mejoras de escudo no tienen un estado de "comprado" inicial
        return true;
    }
}
