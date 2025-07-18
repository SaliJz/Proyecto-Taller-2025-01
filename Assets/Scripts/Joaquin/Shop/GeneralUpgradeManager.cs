using System;
using UnityEngine;

public class GeneralUpgradeManager : MonoBehaviour
{
    public static event Action OnPlayerStatsChanged;
    public static bool IsTutorial { get; set; } = true;

    private const int BASE_HEALTH = 100;
    private const int BASE_SHIELD = 20;

    private const int HEALTH_INCREASE_PER_LEVEL = 20;
    private const int SHIELD_INCREASE_PER_LEVEL = 10;

    public static int HealthLevel { get; set; }
    public static int ShieldLevel { get; set; }

    public static readonly int MAX_HEALTH_UPGRADES = 5;
    public static readonly int MAX_SHIELD_UPGRADES = 5;

    public static int CurrentMaxHealth => BASE_HEALTH + (HealthLevel * HEALTH_INCREASE_PER_LEVEL);
    public static int CurrentMaxShield => BASE_SHIELD + (ShieldLevel * SHIELD_INCREASE_PER_LEVEL);

    public static void ResetUpgrades()
    {
        if (IsTutorial)
        {
            HealthLevel = 0;
            ShieldLevel = 0;

            Debug.Log("Mejoras generales reiniciadas a nivel 0.");

            NotifyStatsChanged();
            SaveLoadManager.DeleteSaveFile();
        }
        else
        {
            Debug.LogWarning("No se pueden reiniciar las mejoras generales en el Nivel 1.");
        }
    }

    public static void NotifyStatsChanged()
    {
        OnPlayerStatsChanged?.Invoke();
    }
}
