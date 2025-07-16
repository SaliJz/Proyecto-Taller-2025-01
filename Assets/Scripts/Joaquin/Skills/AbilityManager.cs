using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static event Action<GameObject> OnAbilityStateChanged;

    [Tooltip("Marca esta casilla solo en la escena del Nivel 1 para reiniciar todo el progreso.")]
    [SerializeField] private bool isLevelTutorial = false;

    [Header("Todas las habilidades posibles")]
    [SerializeField] private List<GameObject> allAbilities;

    [Header("Habilidades activas")]
    [SerializeField] private List<GameObject> activeAbilities = new List<GameObject>();

    [SerializeField] private bool allowFirstAbility = false;

    private int currentIndex = 0;
    private Dictionary<string, GameObject> abilityMap = new Dictionary<string, GameObject>();

    public List<GameObject> activedAbilities => activeAbilities;
    public GameObject CurrentAbility => activeAbilities.Count > 0 ? activeAbilities[currentIndex] : null;

    private void Awake()
    {
        DeactivateAll();

        foreach (var abilityGO in allAbilities)
        {
            var info = abilityGO.GetComponent<AbilityInfo>();
            if (info != null && !abilityMap.ContainsKey(info.abilityName))
            {
                abilityMap.Add(info.abilityName, abilityGO);
            }
            else if (info == null)
            {
                Debug.LogWarning($"Ability prefab '{abilityGO.name}' no tiene un componente AbilityInfo.");
            }
            else if (abilityMap.ContainsKey(info.abilityName))
            {
                Debug.LogWarning($"Habilidad duplicada '{info.abilityName}' en allAbilitiesPrefabs.");
            }
        }
    }

    private void Start()
    {
        if (allowFirstAbility && activeAbilities.Count >= 0)
        {
            GameObject random = allAbilities[UnityEngine.Random.Range(0, allAbilities.Count)];
            AddOrReplaceAbility(random);
            currentIndex = 0;
        }
        else
        {
            LoadFromDataStore();
        }

        currentIndex = Mathf.Clamp(currentIndex, 0, activeAbilities.Count - 1);
        UpdateAbilitiesActiveState();

        if (activeAbilities.Count >= 1) HUDManager.Instance?.ShowAbilityUI(true);
    }

    private void OnEnable()
    {
        DataManager.OnDataChanged += ApplyUpgradesToAllAbilities;
        DataManager.OnPlayerDataLoaded += LoadFromDataStore;
    }

    private void OnDisable()
    {
        DataManager.OnDataChanged -= ApplyUpgradesToAllAbilities;
        DataManager.OnPlayerDataLoaded -= LoadFromDataStore;
    }

    private void ApplyUpgradesToAllAbilities()
    {
        foreach (var abilityGO in allAbilities)
        {
            abilityGO.SendMessage("ApplyUpgrades", SendMessageOptions.DontRequireReceiver);
        }
        Debug.Log("Todas las habilidades han actualizado sus mejoras.");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CycleAbility();
        }
    }

    public void CycleAbility()
    {
        if (activeAbilities.Count <= 1) return;

        currentIndex = (currentIndex + 1) % activeAbilities.Count;
        SaveToDataStore();
        UpdateAbilitiesActiveState();
    }

    public void RemoveAbility(GameObject abilityToRemove)
    {
        if (!activeAbilities.Contains(abilityToRemove)) return;

        if (CurrentAbility == abilityToRemove && activeAbilities.Count > 1)
        {
            CycleAbility();
        }

        activeAbilities.Remove(abilityToRemove);

        if (currentIndex >= activeAbilities.Count && activeAbilities.Count > 0)
        {
            currentIndex = 0;
        }

        SaveToDataStore();
        UpdateAbilitiesActiveState();
    }

    public void AddOrReplaceAbility(GameObject newAbility)
    {
        if (activeAbilities.Contains(newAbility))
        {
            currentIndex = activeAbilities.IndexOf(newAbility);
            UpdateAbilitiesActiveState();
            return;
        }

        if (activeAbilities.Count < 4)
        {
            activeAbilities.Add(newAbility);
        }
        else
        {
            activeAbilities.RemoveAt(0);
            activeAbilities.Add(newAbility);
        }

        currentIndex = activeAbilities.IndexOf(newAbility);
        SaveToDataStore();
        UpdateAbilitiesActiveState();
    }

    public GameObject FindAbilityPrefabByName(string name)
    {
        foreach (var abilityGO in allAbilities)
        {
            var info = abilityGO.GetComponent<AbilityInfo>();
            if (info != null && info.abilityName == name)
            {
                return abilityGO;
            }
        }
        return null;
    }

    public void SetEquippedAbilities(List<string> equippedNames)
    {
        DeactivateAll();
        activeAbilities.Clear();

        foreach (string abilityName in equippedNames)
        {
            GameObject go = null;
            foreach (var abilityGO in allAbilities)
            {
                var info = abilityGO.GetComponent<AbilityInfo>();
                if (info != null && info.abilityName == abilityName)
                {
                    go = abilityGO;
                    break;
                }
            }

            if (go != null)
            {
                activeAbilities.Add(go);
                go.SendMessage("ApplyUpgrades", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                Debug.LogWarning($"No se encontr� el GameObject para '{abilityName}'.");
            }
        }

        if (currentIndex >= activeAbilities.Count)
            currentIndex = activeAbilities.Count > 0 ? activeAbilities.Count - 1 : 0;

        UpdateAbilitiesActiveState();
    }

    private void UpdateAbilitiesActiveState()
    {
        if (activeAbilities.Count <= 0)
        {
            DeactivateAll();
            OnAbilityStateChanged?.Invoke(null);
            return;
        }

        DeactivateAll();

        if (CurrentAbility != null)
        {
            Debug.Log("Activando habilidad: " + CurrentAbility.name);
            CurrentAbility.SetActive(true);
        }
        else
        {
            Debug.Log("No hay habilidad activa.");
        }

        OnAbilityStateChanged?.Invoke(CurrentAbility);
    }

    public void ReplaceAbilityAt(int index, GameObject newAbility)
    {
        if (index < 0 || index >= activeAbilities.Count) return;

        activeAbilities[index] = newAbility;
        currentIndex = index;
        SaveToDataStore();
        UpdateAbilitiesActiveState();

    }

    private void SaveToDataStore()
    {
        var equippedNames = activeAbilities
            .Select(go => go.GetComponent<AbilityInfo>())
            .Where(info => info != null && !string.IsNullOrEmpty(info.abilityName))
            .Select(info => info.abilityName)
            .ToList();

        DataManager.SavePlayerEquippedState(equippedNames, currentIndex);
        Debug.Log($"Estado guardado: {equippedNames.Count} habilidades, �ndice {currentIndex}");
    }

    private void LoadFromDataStore()
    {
        Debug.Log("Cargando habilidades desde DataManager...");
        var saved = DataManager.GetSavedEquippedAbilityNames();
        int savedIndex = DataManager.GetSavedEquippedAbilityIndex();

        SetEquippedAbilities(saved);
        currentIndex = Mathf.Clamp(savedIndex, 0, activeAbilities.Count - 1);
        Debug.Log($"Habilidades cargadas: {activeAbilities.Count}, �ndice {currentIndex}");
    }

    private void DeactivateAll()
    {
        foreach (var go in allAbilities)
            go?.SetActive(false);
    }
}