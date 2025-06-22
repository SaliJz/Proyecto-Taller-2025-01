using System;
using UnityEngine;

public static class AbilityUpgradeManager
{
    public static event Action OnUpgradesChanged;
    public static bool IsNivel1 { get; set; } = true;

    public static int CooldownLevel { get; set; }
    public static int EffectDurationLevel { get; set; }
    public static int EffectRangeLevel { get; set; }

    public const float COOLDOWN_REDUCTION = 1.0f;
    public const float DURATION_INCREASE_PERCENT = 0.05f;
    public const float RANGE_INCREASE_PERCENT = 0.05f;

    public static readonly int MAX_COOLDOWN_UPGRADES = 4;
    public static readonly int MAX_DURATION_UPGRADES = 4;
    public static readonly int MAX_RANGE_UPGRADES = 4;

    public static void ResetUpgrades()
    {
        if (IsNivel1)
        {
            CooldownLevel = 0;
            EffectDurationLevel = 0;
            EffectRangeLevel = 0;

            Debug.Log("Mejoras globales de habilidades reiniciadas para el Nivel 1.");
            IsNivel1 = false;
            NotifyUpgradesChanged();
        }
    }

    public static void NotifyUpgradesChanged()
    {
        OnUpgradesChanged?.Invoke();
    }
}