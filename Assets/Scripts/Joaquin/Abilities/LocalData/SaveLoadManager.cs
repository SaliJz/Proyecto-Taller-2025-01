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
        if (File.Exists(saveFilePath))
            File.Copy(saveFilePath, saveFilePath + ".bak", true);

        // 1. Crear un objeto PlayerData con los datos actuales del jugador
        var data = new PlayerData
        {
            healthLevel = DataManager.HealthUpgradeLevel,
            shieldLevel = DataManager.ShieldUpgradeLevel,
            purchasedAbilities = DataManager.GetPurchasedAbilities().ToList(),
            equippedAbilityNames = DataManager.GetSavedEquippedAbilityNames(),
            lastEquippedIndex = DataManager.GetSavedEquippedAbilityIndex(),

            upgradeAbilityNames = DataManager.GetAllAbilityStats().Keys.ToList(),
            upgradeAbilityStats = DataManager.GetAllAbilityStats().Values.ToList(),

            weaponUpgradeTypes = DataManager.GetAllWeaponStats().Keys.ToList(),
            weaponUpgradeStats = DataManager.GetAllWeaponStats().Values.ToList(),
        };

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
        if (!File.Exists(saveFilePath)) return;

        try
        {
            string json = File.ReadAllText(saveFilePath);
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            DataManager.LoadData(data);
        }
        catch (IOException ex)
        {
            Debug.LogError($"Error al leer el archivo de guardado: {ex.Message}");
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