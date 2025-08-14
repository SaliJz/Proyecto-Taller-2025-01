using UnityEngine;

[CreateAssetMenu(fileName = "NewWeaponStats", menuName = "Weapons/WeaponStats")]
public class WeaponStats : ScriptableObject
{
    public string weaponName;
    public Sprite weaponIcon;
    public float fireRate;
    public float bulletDamage;
    public float cooldownRate;
    public Weapon.ShootingMode shootingMode;
    public float weaponSwapTime;
    public float heatPerShot;
    public float maxHeat;
    public float overheatDuration;
}