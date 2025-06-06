using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [Header("Todas las habilidades posibles")]
    [SerializeField] private List<GameObject> allAbilities;

    [Header("Habilidades activas")]
    [SerializeField] private List<GameObject> activeAbilities = new List<GameObject>();

    [SerializeField] private bool allowFirstAbility = false;
    private int currentIndex = 0;

    public List<GameObject> activedAbilities => activeAbilities;
    public GameObject CurrentAbility => activeAbilities.Count > 0 ? activeAbilities[currentIndex] : null;

    private void Start()
    {
        foreach (var ability in allAbilities)
        {
            ability.SetActive(false);
        }

        LoadFromDataStore();

        if (allowFirstAbility)
        {
            if (activeAbilities.Count == 0 && allAbilities.Count > 0)
            {
                GameObject random = allAbilities[Random.Range(0, allAbilities.Count)];
                activeAbilities.Add(random);
                currentIndex = 0;
                SaveToDataStore();
            }
        }

        UpdateAbilitiesActiveState();
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

    public void AddOrReplaceAbility(GameObject newAbility)
    {
        if (activeAbilities.Contains(newAbility)) return;

        if (activeAbilities.Count < 2)
        {
            activeAbilities.Add(newAbility);
        }
        else
        {
            activeAbilities[currentIndex] = newAbility;
        }

        currentIndex = activeAbilities.IndexOf(newAbility);
        SaveToDataStore();
        UpdateAbilitiesActiveState();
    }

    private void UpdateAbilitiesActiveState()
    {
        if (activeAbilities.Count <= 0) return;

        foreach (var ability in allAbilities)
        {
            ability.SetActive(false);
        }

        for (int i = 0; i < activeAbilities.Count; i++)
        {
            activeAbilities[i].SetActive(i == currentIndex);
        }

        HUDManager.Instance?.UpdateAbilityUI(CurrentAbility);
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
        if (AbilityDataStore.Instance == null) return;

        AbilityDataStore.Instance.AbilityNames.Clear();
        foreach (var a in activeAbilities)
        {
            AbilityDataStore.Instance.AbilityNames.Add(a.name);
        }

        // Fix for CS0200: Use a method or alternative approach to update CurrentIndex
        AbilityDataStore.Instance.SetCurrentIndex(currentIndex);
    }

    private void LoadFromDataStore()
    {
        activeAbilities.Clear();

        if (AbilityDataStore.Instance == null) return;

        foreach (string abilityName in AbilityDataStore.Instance.AbilityNames)
        {
            GameObject ability = allAbilities.Find(a => a.name == abilityName);
            if (ability != null) activeAbilities.Add(ability);
        }

        currentIndex = AbilityDataStore.Instance.CurrentIndex;
        currentIndex = Mathf.Clamp(currentIndex, 0, activeAbilities.Count - 1);
    }
}