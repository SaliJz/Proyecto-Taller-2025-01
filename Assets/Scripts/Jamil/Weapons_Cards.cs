
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponMenu/New Card")]
public class Weapons_Cards : ScriptableObject
{
    public enum UpgradeType
    {
        WeaponDamage,
        FireRate,
        ReloadSpeed,
        AmmoBonus
    }

    public enum CurrentState
    {
        Selected,
        InUse,
        Waiting,
        Used,
    }

    public int ID;
    public CurrentState currentState = CurrentState.Waiting;
    public UpgradeType upgradeType;
    public string buffDescriptionText;
    public int price;
    public int upgrade;

}