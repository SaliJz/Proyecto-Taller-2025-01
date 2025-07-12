using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    public static event Action<GameObject> OnAbilityStateChanged;

    [Tooltip("Marca esta casilla SOLO en la escena del Nivel 1 para reiniciar todo el progreso.")]
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
        foreach (var ability in allAbilities)
        {
            ability.SetActive(false);
        }

        if (isLevelTutorial) AbilityShopDataManager.ResetData();

        LoadFromDataStore();

        currentIndex = Mathf.Clamp(currentIndex, 0, activeAbilities.Count - 1);

        UpdateAbilitiesActiveState();

        if (activeAbilities.Count >= 1) HUDManager.Instance?.ShowAbilityUI(true);
    }

    private void Start()
    {
        if (allowFirstAbility)
        {
            if (activeAbilities.Count >= 0)
            {
                GameObject random = allAbilities[UnityEngine.Random.Range(0, allAbilities.Count)];
                activeAbilities.Add(random);
                currentIndex = 0;
                SaveToDataStore();
            }
        }

        foreach (var abilityGO in allAbilities)
        {
            var info = abilityGO.GetComponent<AbilityInfo>();
            if (info != null && !abilityMap.ContainsKey(info.abilityName))
            {
                abilityMap.Add(info.abilityName, abilityGO);
            }
        }
    }

    private void OnEnable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged += ApplyUpgradesToAllAbilities;
    }

    private void OnDisable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged -= ApplyUpgradesToAllAbilities;
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
        if (activeAbilities.Contains(abilityToRemove))
        {
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
    }

    public void AddOrReplaceAbility(GameObject newAbility)
    {
        if (activeAbilities.Contains(newAbility))
        {
            currentIndex = activeAbilities.IndexOf(newAbility);
            UpdateAbilitiesActiveState();
            return;
        }

        if (activeAbilities.Count < 2)
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

    public void SetEquippedAbilities(List<AbilityType> equippedTypes)
    {
        activeAbilities.Clear();

        foreach (AbilityType type in equippedTypes)
        {
            if (type == AbilityType.None) continue;

            GameObject abilityGO = allAbilities.Find(go =>
            {
                AbilityInfo info = go.GetComponent<AbilityInfo>();
                return info != null && info.abilityName == type.ToString();
            });

            if (abilityGO != null)
            {
                activeAbilities.Add(abilityGO);
            }
            else
            {
                Debug.LogWarning($"No se pudo encontrar un GameObject de habilidad correspondiente al tipo: {type.ToString()}");
            }
        }

        if (currentIndex >= activeAbilities.Count)
        {
            currentIndex = activeAbilities.Count > 0 ? activeAbilities.Count - 1 : 0;
        }

        UpdateAbilitiesActiveState();
    }

    private void UpdateAbilitiesActiveState()
    {
        if (activeAbilities.Count <= 0)
        {
            OnAbilityStateChanged?.Invoke(null);
            return;
        }

        foreach (var ability in allAbilities)
        {
            if (ability != null) ability.SetActive(false);
        }

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
        if (index >= 0 && index < activeAbilities.Count)
        {
            activeAbilities[index] = newAbility;
            currentIndex = index;
            SaveToDataStore();
            UpdateAbilitiesActiveState();
        }
    }

    private void SaveToDataStore()
    {
        List<string> equippedNames = new List<string>();
        foreach (var abilityGO in activeAbilities)
        {
            equippedNames.Add(abilityGO.GetComponent<AbilityInfo>().abilityName);
        }
        AbilityShopDataManager.SavePlayerEquippedState(equippedNames, currentIndex);
    }

    private void LoadFromDataStore()
    {
        activeAbilities.Clear();
        List<string> savedNames = AbilityShopDataManager.GetSavedEquippedAbilities();

        foreach (string abilityName in savedNames)
        {
            GameObject ability = allAbilities.Find(a => a.GetComponent<AbilityInfo>().abilityName == abilityName);
            if (ability != null)
            {
                activeAbilities.Add(ability);
            }
        }

        currentIndex = AbilityShopDataManager.GetSavedEquippedIndex();
        currentIndex = Mathf.Clamp(currentIndex, 0, activeAbilities.Count > 0 ? activeAbilities.Count - 1 : 0);
    }
}