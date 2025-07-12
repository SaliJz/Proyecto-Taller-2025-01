using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AbilitySelectionUI : MonoBehaviour
{
    [Header("Elementos HUD")]
    [SerializeField] private Button passiveUpgradeButton;
    [SerializeField] private Button abilityButton;
    [SerializeField] private Button rerollButton;
    [SerializeField] private string sceneName;
    [SerializeField] private TextMeshProUGUI rerollText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Todas las habilidades y mejoras temporales")]
    [SerializeField] private List<GameObject> allAbilities;
    [SerializeField] private List<PassiveUpgradeStats> passiveUpgrades;

    private GameObject proposedAbility;
    private PassiveUpgradeStats proposedUpgrade;
    private int rerollAttempts = 3;

    private void OnEnable()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        rerollAttempts = 3; // Resetear rerolls si
        GenerateNewOptions();
    }

    private void Awake()
    {
        rerollButton.onClick.AddListener(RerollOptions);
    }

    public void GenerateNewOptions()
    {
        List<GameObject> filteredAbilities = new List<GameObject>(allAbilities);
        AbilityManager abilityMgr = FindObjectOfType<AbilityManager>();
        if (abilityMgr == null) return;

        foreach (var a in abilityMgr.activedAbilities)
        {
            filteredAbilities.Remove(a);
        }

        if (filteredAbilities.Count > 0)
        {
            proposedAbility = filteredAbilities[Random.Range(0, filteredAbilities.Count)];
            UpdateAbilityButton(proposedAbility);
        }
        else
        {
            proposedAbility = null;
            abilityButton.interactable = false;
            abilityButton.GetComponentInChildren<TextMeshProUGUI>().text = "Todas las habilidades desbloqueadas";
        }

        proposedUpgrade = passiveUpgrades[Random.Range(0, passiveUpgrades.Count)];
        UpdatePassiveButton(proposedUpgrade);

        rerollText.text = $"Rerolls restantes: {rerollAttempts}";
        promptText.text = abilityMgr.activedAbilities.Count < 2 ? "Selecciona una nueva habilidad" :
                                                        "Seleccione una habilidad para reemplazar";
    }

    private void UpdatePassiveButton(PassiveUpgradeStats upgrade)
    {
        passiveUpgradeButton.GetComponentInChildren<TextMeshProUGUI>().text = upgrade.upgradeName;
        passiveUpgradeButton.onClick.RemoveAllListeners();
        //passiveUpgradeButton.onClick.AddListener(() => ApplyUpgrade(upgrade));
    }

    private void UpdateAbilityButton(GameObject ability)
    {
        string name = ability.GetComponent<AbilityInfo>()?.abilityName ?? ability.name;
        abilityButton.GetComponentInChildren<TextMeshProUGUI>().text = name;
        abilityButton.onClick.RemoveAllListeners();
        abilityButton.onClick.AddListener(() => ApplyAbility(ability));
    }

    //private void ApplyUpgrade(PassiveUpgradeStats upgrade)
    //{
    //    UpgradeDataStore.Instance.ApplyUpgrade(upgrade);
    //    GoToNextScene();
    //}

    private void ApplyAbility(GameObject ability)
    {
        AbilityManager abilityMgr = FindObjectOfType<AbilityManager>();
        if (abilityMgr == null || ability == null) return;

        if (abilityMgr.activedAbilities.Contains(ability)) return;

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
        promptText.text = "Selecciona la habilidad para reemplazar";

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
        if (string.IsNullOrEmpty(sceneName)) // Verifica si la escena está vacía
        {
            // Si no hay escena asignada, simplemente cierra el menú de selección de habilidades
            gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            return;
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }
}
