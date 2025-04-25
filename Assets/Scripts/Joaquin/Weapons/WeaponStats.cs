using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Weapons/WeaponStats")]
public class WeaponStats : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;
    public int maxAmmoPerClip;
    public int totalAmmo;
    public float reloadTime;
    public Weapon.ShootingMode shootingMode;
    public float weaponSwapTime;
    public float fireRate;
    public float bulletDamage;
}
