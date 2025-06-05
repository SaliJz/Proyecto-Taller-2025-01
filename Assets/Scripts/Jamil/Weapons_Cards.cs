
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponMenu/New Card")]
public class Weapons_Cards : ScriptableObject
{
    public string buffDescriptionText;
    public int price;
    public int upgradeDamage;
    public int upgradeFireRate;
    public int upgradeReloadSpeed;
    public int upgradeAmmoBonus;
}