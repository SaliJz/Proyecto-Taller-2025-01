using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : MonoBehaviour
{
    [SerializeField] private bool isLevel1 = false;

    [Header("Todas las habilidades posibles")]
    [SerializeField] private List<GameObject> allAbilities;

    [Header("Habilidades activas")]
    [SerializeField] private List<GameObject> activeAbilities = new List<GameObject>();

    [SerializeField] private bool allowFirstAbility = false;

    private int currentIndex = 0;
    private Dictionary<string, GameObject> abilityMap = new Dictionary<string, GameObject>();
    private VFXController vfxController;

    public List<GameObject> activedAbilities => activeAbilities;
    public GameObject CurrentAbility => activeAbilities.Count > 0 ? activeAbilities[currentIndex] : null;

    private void Awake()
    {
        foreach (var abilityGO in allAbilities)
        {
            var info = abilityGO.GetComponent<AbilityInfo>();
            if (info != null && !abilityMap.ContainsKey(info.abilityName))
            {
                abilityMap.Add(info.abilityName, abilityGO);
            }
        }
    }

    private void Start()
    {
        vfxController = FindObjectOfType<VFXController>();

        if (isLevel1) AbilityShopDataManager.ResetData();

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

        if (vfxController != null)
        {
            vfxController.DeactivateAll();
            vfxController.ActivateVFX(CurrentAbility.name);
        }

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

            GameObject abilityGO = allAbilities.Find(go => {
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

    public void ClearAbilities()
    {
        activeAbilities.Clear();
        currentIndex = 0;
        SaveToDataStore();
        UpdateAbilitiesActiveState();
    }
}