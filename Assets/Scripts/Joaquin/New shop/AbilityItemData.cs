using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewAbilityItem", menuName = "Shop/Ability Item")]
public class AbilityItemData : ShopItemData
{
    private void OnEnable()
    {
        itemType = ItemType.Ability; // Asegurarse de que el tipo sea correcto
    }

    // Implementaci�n para obtener el nivel de la habilidad desde DataManager
    public override int GetCurrentLevel(string specificItemName)
    {
        return DataManager.GetAbilityLevel(itemName);
    }

    // Implementaci�n para incrementar el nivel de la habilidad
    public override void IncrementLevel(string specificItemName)
    {
        DataManager.IncrementAbilityLevel(itemName);
    }

    // Implementaci�n para verificar si la habilidad ha sido comprada
    public override bool IsPurchased(string specificItemName)
    {
        return DataManager.IsAbilityPurchased(itemName);
    }
}
