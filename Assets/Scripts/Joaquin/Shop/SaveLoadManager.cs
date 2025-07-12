using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SaveLoadManager
{
    private static readonly string saveFilePath = Path.Combine(Application.persistentDataPath, "playerdata.json");

    public static void SaveGame()
    {
        PlayerData data = new PlayerData();

        // 1. Recopilar datos de los Managers
        data.healthLevel = GeneralUpgradeManager.HealthLevel;
        data.shieldLevel = GeneralUpgradeManager.ShieldLevel;

        data.purchasedAbilities = AbilityShopDataManager.GetPurchasedAbilities().ToList();
        data.equippedAbilityNames = AbilityShopDataManager.GetSavedEquippedAbilities();
        data.lastEquippedIndex = AbilityShopDataManager.GetSavedEquippedIndex();

        // Convertir el diccionario de mejoras a listas
        var upgrades = AbilityShopDataManager.GetUpgradeLevels();
        data.upgradeAbilityNames = upgrades.Keys.ToList();
        data.upgradeAbilityStats = upgrades.Values.ToList();

        // 2. Convertir a JSON y guardar en archivo
        string json = JsonUtility.ToJson(data, true); // "true" para formato legible
        try
        {
            File.WriteAllText(saveFilePath, json);
            Debug.Log("¡Partida guardada exitosamente en " + saveFilePath);
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al guardar el archivo: {ex.Message}");
        }
    }

    public static void LoadGame()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string json = File.ReadAllText(saveFilePath);
                PlayerData data = JsonUtility.FromJson<PlayerData>(json);

                // 1. Aplicar los datos cargados a los Managers
                GeneralUpgradeManager.HealthLevel = data.healthLevel;
                GeneralUpgradeManager.ShieldLevel = data.shieldLevel;

                // Reconstruir los datos de habilidades
                AbilityShopDataManager.LoadDataFromSave(
                    data.purchasedAbilities,
                    data.equippedAbilityNames,
                    data.lastEquippedIndex,
                    data.upgradeAbilityNames,
                    data.upgradeAbilityStats
                );

                // Notificar a la UI y otros sistemas que los datos cambiaron
                GeneralUpgradeManager.NotifyStatsChanged();
                AbilityShopDataManager.NotifyDataChanged();

                Debug.Log("Partida cargada exitosamente.");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error al leer el archivo de guardado: {ex.Message}");
            }
        }
        else
        {
            Debug.Log("No se encontró archivo de guardado. Se iniciará una nueva partida.");
        }
    }

    public static void DeleteSaveFile()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
                Debug.Log("Archivo de guardado eliminado.");
            }
            catch (IOException ex)
            {
                Debug.LogError($"Error al eliminar el archivo de guardado: {ex.Message}");
            }
        }
    }
}
