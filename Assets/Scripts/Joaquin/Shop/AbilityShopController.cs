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

    [Header("UI de Feedback")]
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private float confirmationDisplayTime = 1.5f;

    private Coroutine confirmationCoroutine;
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

        finalizePurchaseButton.onClick.AddListener(FinalizePurchase);
        returnButton.onClick.AddListener(() => returnConfirmationPanel.SetActive(true));
        confirmReturnButton.onClick.AddListener(ConfirmReturn);
        cancelReturnButton.onClick.AddListener(() => returnConfirmationPanel.SetActive(false));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (returnConfirmationPanel.activeSelf)
            {
                returnConfirmationPanel.SetActive(false);
            }
            else
            {
                returnConfirmationPanel.SetActive(true);
            }
        }
    }

    private void Start()
    {
        UpdateAllButtonVisuals();
        UpdateTotalCost();
        descriptionText.text = string.Empty;
        if (returnConfirmationPanel != null) returnConfirmationPanel.SetActive(false);
    }

    private void OnEnable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged += UpdateAllButtonVisuals;
    }

    private void OnDisable()
    {
        AbilityShopDataManager.OnAbilityShopDataChanged -= UpdateAllButtonVisuals;
    }

    public void HandleButtonClick(AbilityButtonController button)
    {
        if (button.CurrentFunction == ButtonFunction.EquipAbility)
        {
            abilityManager.AddOrReplaceAbility(button.AbilityPrefab);
            UpdateAllButtonVisuals();
            ShowConfirmation("¡Habilidad equipada con éxito!");
            return;
        }
        if (button.CurrentFunction == ButtonFunction.UnequipAbility)
        {
            abilityManager.RemoveAbility(button.AbilityPrefab);
            UpdateAllButtonVisuals();
            ShowConfirmation("¡Habilidad desequipada con éxito!");
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
        if (totalCost <= 0)
        {
            ShowConfirmation("Nada seleccionado para comprar.");
            return;
        }
        if (HUDManager.Instance.CurrentFragments < totalCost)
        {
            ShowConfirmation("No tienes suficientes F. Cod.");
            return;
        }

        var itemsToProcess = new List<AbilityButtonController>(selectedItems);
        selectedItems.Clear();
        UpdateTotalCost();

        HUDManager.Instance.DiscountInfoFragment(totalCost);

        List<string> abilitiesToBuy = new List<string>();
        Dictionary<string, List<string>> upgradesToBuy = new Dictionary<string, List<string>>();

        foreach (var button in itemsToProcess)
        {
            string abilityName = button.AbilityPrefab.GetComponent<AbilityInfo>().abilityName;
            if (button.CurrentFunction == ButtonFunction.PurchaseAbility)
            {
                abilitiesToBuy.Add(abilityName);
            }
            else if (button.CurrentFunction == ButtonFunction.UpgradeStat)
            {
                if (!upgradesToBuy.ContainsKey(abilityName))
                {
                    upgradesToBuy[abilityName] = new List<string>();
                }
                upgradesToBuy[abilityName].Add(button.UpgradeStatName);
            }
        }

        AbilityShopDataManager.PurchaseItems(abilitiesToBuy, upgradesToBuy);
        ShowConfirmation("¡Compra realizada con éxito!");
    }

    private void ConfirmReturn()
    {
        selectedItems.Clear();
        returnConfirmationPanel.SetActive(false);
        MainShopMenu.SetActive(true);
        gameObject.SetActive(false);
    }

    public bool IsPurchased(AbilityButtonController button) => AbilityShopDataManager.IsPurchased(button.AbilityPrefab.GetComponent<AbilityInfo>().abilityName);
    public bool IsEquipped(AbilityButtonController button)
    {
        if (abilityManager == null)
        {
            Debug.LogError("abilityManager es null");
            return false;
        }

        if (abilityManager.activedAbilities == null)
        {
            Debug.LogError("activedAbilities es null");
            return false;
        }

        if (button.AbilityPrefab == null)
        {
            Debug.LogError("AbilityPrefab no está asignado en el botón: " + button.name);
            return false;
        }

        return abilityManager.activedAbilities.Contains(button.AbilityPrefab);
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

    public void ShowConfirmation(string message)
    {
        if (confirmationText == null) return;

        if (confirmationCoroutine != null)
        {
            StopCoroutine(confirmationCoroutine);
        }
        confirmationCoroutine = StartCoroutine(ShowConfirmationCoroutine(message));
    }

    private IEnumerator ShowConfirmationCoroutine(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSeconds(confirmationDisplayTime);
        confirmationText.gameObject.SetActive(false);
    }
}