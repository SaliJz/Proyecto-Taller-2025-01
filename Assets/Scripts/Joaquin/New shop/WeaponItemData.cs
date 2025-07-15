using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponItem", menuName = "Shop/Weapon Item")]
public class WeaponItemData : ShopItemData
{
    private void OnEnable()
    {
        itemType = ItemType.Weapon;
    }

    public override int GetCurrentLevel(string specificItemName)
    {
        return DataManager.GetWeaponLevel(itemName);
    }

    public override void IncrementLevel(string specificItemName)
    {
        DataManager.IncrementWeaponLevel(itemName);
    }

    public override bool IsPurchased(string specificItemName = null)
    {
        // Las armas se asumen "compradas" si están disponibles para mejorar,
        // o si tienes un sistema de desbloqueo, podrías verificarlo aquí.
        // Para este ejemplo, siempre se pueden "mejorar" si existen.
        return true;
    }
}
