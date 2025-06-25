using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class AbilityShopController : MonoBehaviour
{
    [Header("Referencias Generales")]
    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private GameObject MainShopMenu;
    [SerializeField] private TextMeshProUGUI descriptionText;

    [Header("Botones de Compra")]
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private Button finalizePurchaseButton;

    [Header("Panel de Confirmación de Salida")]
    [SerializeField] private GameObject returnConfirmationPanel;
    [SerializeField] private Button returnButton;
    [SerializeField] private Button confirmReturnButton;
    [SerializeField] private Button cancelReturnButton;

    private readonly List<AbilityButtonController> selectedItems = new List<AbilityButtonController>();

    private void Awake()
    {
        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
            {
                Debug.LogError("AbilityManager not found in the scene. Please assign it in the inspector.");
            }
        }

        AbilityShopDataManager.Initialize();
        finalizePurchaseButton.onClick.AddListener(FinalizePurchase);
        returnButton.onClick.AddListener(() => returnConfirmationPanel.SetActive(true));
        confirmReturnButton.onClick.AddListener(ConfirmReturn);
        cancelReturnButton.onClick.AddListener(() => returnConfirmationPanel.SetActive(false));
    }

    private void OnEnable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged += UpdateAllButtonVisuals;
        UpdateAllButtonVisuals();
        UpdateTotalCost();
        returnConfirmationPanel.SetActive(false);
    }

    private void OnDisable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged -= UpdateAllButtonVisuals;
    }

    public void HandleButtonClick(AbilityButtonController button)
    {
        if (button.CurrentFunction == ButtonFunction.EquipAbility)
        {
            AbilityShopDataManager.EquipAbility(button.AssociatedAbility);
            UpdatePlayerEquippedAbilities();
            return;
        }
        if (button.CurrentFunction == ButtonFunction.UnequipAbility)
        {
            AbilityShopDataManager.UnequipAbility(button.AssociatedAbility);
            UpdatePlayerEquippedAbilities();
            return;
        }

        ToggleSelection(button);
    }

    private void ToggleSelection(AbilityButtonController button)
    {
        if (selectedItems.Contains(button)) selectedItems.Remove(button);
        else selectedItems.Add(button);
        UpdateTotalCost();
        button.UpdateVisuals();
    }

    private void FinalizePurchase()
    {
        int totalCost = CalculateTotalCost();
        if (totalCost <= 0 || HUDManager.Instance.CurrentFragments < totalCost) return;

        HUDManager.Instance.DiscountInfoFragment(totalCost);

        List<AbilityType> abilitiesToBuy = new List<AbilityType>();
        Dictionary<AbilityType, List<string>> upgradesToBuy = new Dictionary<AbilityType, List<string>>();

        foreach (var button in selectedItems)
        {
            if (button.CurrentFunction == ButtonFunction.PurchaseAbility)
            {
                abilitiesToBuy.Add(button.AssociatedAbility);
            }
            else if (button.CurrentFunction == ButtonFunction.UpgradeStat)
            {
                if (!upgradesToBuy.ContainsKey(button.AssociatedAbility))
                {
                    upgradesToBuy[button.AssociatedAbility] = new List<string>();
                }
                upgradesToBuy[button.AssociatedAbility].Add(button.UpgradeStatName);
            }
        }

        if (abilitiesToBuy.Count > 0) AbilityShopDataManager.PurchaseAbilities(abilitiesToBuy);
        if (upgradesToBuy.Count > 0) AbilityShopDataManager.UpgradeStats(upgradesToBuy);

        selectedItems.Clear();
        UpdateTotalCost();
    }

    private void ConfirmReturn()
    {
        selectedItems.Clear();
        returnConfirmationPanel.SetActive(false);
        MainShopMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public bool IsSelected(AbilityButtonController button) => selectedItems.Contains(button);
    public void ShowDescription(string text) => descriptionText.text = text;
    public void HideDescription() => descriptionText.text = string.Empty;

    private void UpdateAllButtonVisuals()
    {
        var allButtons = GetComponentsInChildren<AbilityButtonController>(true);
        foreach (var button in allButtons)
        {
            if (button != null) button.UpdateVisuals();
        }
    }

    private void UpdateTotalCost()
    {
        totalCostText.text = $"Coste Total: {CalculateTotalCost()} F. Cod.";
    }

    private int CalculateTotalCost()
    {
        int total = 0;
        foreach (var button in selectedItems) total += button.GetCurrentCost();
        return total;
    }

    private void UpdatePlayerEquippedAbilities()
    {
        abilityManager.SetEquippedAbilities(AbilityShopDataManager.GetEquippedAbilities());
    }
}