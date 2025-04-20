using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AbilitySelectionUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button passiveUpgradeButton;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private string sceneName;
    [SerializeField] private TextMeshProUGUI rerollText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("All Abilities and Upgrades")]
    [SerializeField] private List<GameObject> allAbilities;
    [SerializeField] private List<string> passiveUpgrades;

    private GameObject proposedAbility;
    private string proposedUpgrade;
    private int rerollAttempts = 3;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Awake()
    {
        rerollButton.onClick.AddListener(RerollOptions);
        passiveUpgradeButton.onClick.AddListener(() => ApplyUpgrade(proposedUpgrade));
        abilityButton.onClick.AddListener(() => ApplyAbility(proposedAbility));

        Time.timeScale = 0f;
        GenerateNewOptions();
    }

    public void GenerateNewOptions()
    {
        AbilityManager abilityMgr = FindObjectOfType<AbilityManager>();
        if (abilityMgr == null) return;

        // Asignar mejora pasiva
        proposedUpgrade = passiveUpgrades[Random.Range(0, passiveUpgrades.Count)];
        UpdatePassiveButton(proposedUpgrade);

        // Asignar habilidad propuesta
        List<GameObject> filteredAbilities = new List<GameObject>(allAbilities);
        foreach (var a in abilityMgr.activedAbilities)
        {
            filteredAbilities.Remove(a);
        }

        if (filteredAbilities.Count > 0)
        {
            proposedAbility = filteredAbilities[Random.Range(0, filteredAbilities.Count)];
        }
        else
        {
            // Ya tiene todas las habilidades
            proposedAbility = abilityMgr.activedAbilities[Random.Range(0, abilityMgr.activedAbilities.Count)];
        }

        UpdateAbilityButton(proposedAbility);

        rerollText.text = $"Rerolls left: {rerollAttempts}";

        promptText.text = abilityMgr.activedAbilities.Count < 2 ? "Select a new skill" : "Select a skill to replace";
    }

    private void UpdatePassiveButton(string upgrade)
    {
        passiveUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = upgrade;
        passiveUpgradeButton.onClick.RemoveAllListeners();
        passiveUpgradeButton.onClick.AddListener(() => ApplyUpgrade(upgrade));
    }

    private void UpdateAbilityButton(GameObject ability)
    {
        string name = ability.GetComponent<AbilityInfo>()?.abilityName ?? ability.name;
        abilityButton.GetComponentInChildren<TextMeshProUGUI>().text = name;

        abilityButton.onClick.RemoveAllListeners();
        abilityButton.onClick.AddListener(() => ApplyAbility(ability));
    }

    private void ApplyUpgrade(string upgrade)
    {
        switch (upgrade)
        {
            case "Bullet Damage +25%":
                UpgradeDataStore.Instance.abilityDamageMultiplier += 0.25f;
                break;
            case "Faster Fire Rate":
                UpgradeDataStore.Instance.weaponFireRateMultiplier += 0.2f;
                break;
            case "Reload Speed +20%":
                UpgradeDataStore.Instance.weaponReloadSpeedMultiplier += 0.2f;
                break;
            case "Extra Ammo +30":
                UpgradeDataStore.Instance.weaponAmmoBonus += 30;
                break;
        }

        GoToNextScene();
    }

    private void ApplyAbility(GameObject ability)
    {
        AbilityManager abilityMgr = FindObjectOfType<AbilityManager>();
        if (abilityMgr == null) return;

        if (abilityMgr.activedAbilities.Contains(ability))
        {
            GoToNextScene(); // Ya la tiene
            return;
        }

        if (abilityMgr.activedAbilities.Count < 2)
        {
            abilityMgr.AddOrReplaceAbility(ability);
            GoToNextScene();
        }
        else
        {
            StartCoroutine(WaitForReplacement(ability));
        }
    }

    private IEnumerator WaitForReplacement(GameObject newAbility)
    {
        AbilityManager abilityMgr = FindObjectOfType<AbilityManager>();

        promptText.text = "Select skill to replace";

        var active = abilityMgr.activedAbilities;

        string nameA = active[0].GetComponent<AbilityInfo>()?.abilityName ?? active[0].name;
        string nameB = active[1].GetComponent<AbilityInfo>()?.abilityName ?? active[1].name;

        abilityButton.GetComponentInChildren<TextMeshProUGUI>().text = nameA;
        passiveUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = nameB;

        bool selectionMade = false;

        abilityButton.onClick.RemoveAllListeners();
        passiveUpgradeButton.onClick.RemoveAllListeners();

        abilityButton.onClick.AddListener(() =>
        {
            abilityMgr.ReplaceAbilityAt(0, newAbility);
            selectionMade = true;
        });

        passiveUpgradeButton.onClick.AddListener(() =>
        {
            abilityMgr.ReplaceAbilityAt(1, newAbility);
            selectionMade = true;
        });

        yield return new WaitUntil(() => selectionMade);
        GoToNextScene();
    }

    public void RerollOptions()
    {
        if (rerollAttempts > 0)
        {
            rerollAttempts--;
            GenerateNewOptions();
        }
    }

    private void GoToNextScene()
    {
        Time.timeScale = 1f;
        if (sceneName == "")
        {
            Debug.Log("No hay escena asignada para cargar después de la selección.");
            return;
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
