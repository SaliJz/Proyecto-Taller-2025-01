using UnityEngine;

[CreateAssetMenu(menuName = "Upgrades/Passive Upgrade")]
public class PassiveUpgradeStats : ScriptableObject
{
    public string upgradeName;
    [TextArea] public string description;

    public enum UpgradeType
    {
        WeaponDamage,
        FireRate,
        ReloadSpeed,
        AmmoBonus
    }

    public UpgradeType upgradeType;

    [Tooltip("Si la mejora es porcentual (true) o un valor absoluto (false)")]
    public bool isPercentage = true;

    [Tooltip("Rango mínimo y máximo del valor a aplicar (0.2 - 0.5 equivale a 20%-50%)")]
    public Vector2 valueRange;
}
