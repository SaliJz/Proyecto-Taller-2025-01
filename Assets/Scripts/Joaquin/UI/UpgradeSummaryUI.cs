using TMPro;
using UnityEngine;

public class UpgradeSummaryUI : MonoBehaviour
{
    [Header("Texto de resumen")]
    [SerializeField] private TextMeshProUGUI statsText;

    private void OnEnable()
    {
        if (UpgradeDataStore.Instance != null)
        {
            statsText.text = GetFormattedStats();
        }
    }

    private string GetFormattedStats()
    {
        var u = UpgradeDataStore.Instance;

        return
        $"<b>--- Weapon Upgrades ---</b>\n" +
        $"Damage Multiplier: <b>{u.weaponDamageMultiplier:F2}</b>\n" +
        $"Fire Rate Multiplier: <b>{u.weaponFireRateMultiplier:F2}</b>\n" +
        $"Reload Speed Multiplier: <b>{u.weaponReloadSpeedMultiplier:F2}</b>\n" +
        $"Extra Ammo: <b>+{u.weaponAmmoBonus}</b>\n\n" +

        $"<b>--- Ability Upgrades ---</b>\n" +
        $"Ability Damage Multiplier: <b>{u.abilityDamageMultiplier:F2}</b>\n" +
        $"Ability Duration Multiplier: <b>{u.abilityDurationMultiplier:F2}</b>\n" +
        $"Cooldown Reduction: <b>{u.abilityCooldownReduction * 100:F0}%</b>\n\n" +

        $"<b>--- Player Upgrades ---</b>\n" +
        $"Max Health: <b>{u.playerMaxHealth}</b>\n" +
        $"Dash Cooldown: <b>{u.playerDashCooldown:F2}s</b>";
    }
}
