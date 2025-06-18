using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "WeaponMenuCorregido/New Card")]
public class Weapon_Card_Corregido : ScriptableObject
{
    public string buffDescriptionText;
    public int price;
    public float damageBuff;
    public float fireRatioBuff;
    public float ReloadSpeedBuff;
    public int AmmoBonus;
}
