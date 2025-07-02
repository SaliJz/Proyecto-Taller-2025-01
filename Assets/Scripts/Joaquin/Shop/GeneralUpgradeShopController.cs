using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GeneralUpgradeShopController : MonoBehaviour
{
    [SerializeField] private GameObject generalUpgradeMenu;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private TextMeshProUGUI descriptionPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button finalizeButton;
    [SerializeField] private Button returnButton;

    [SerializeField] private float confirmationDelay = 1.5f;

    [SerializeField] private GameObject returnPanel;
    [SerializeField] private GameObject mainShopMenu;
    [SerializeField] private TextMeshProUGUI confirmationPanelText;
    [SerializeField] private Button confirmationButton;
    [SerializeField] private Button cancelButton;

    private Dictionary<string, int> selectedUpgrades = new Dictionary<string, int>();
    private Coroutine confirmationCoroutine;
    private int totalCost = 0;

    private void Awake()
    {
        finalizeButton.onClick.AddListener(FinalizePurchase);
        returnButton.onClick.AddListener(Return);

        confirmationButton.onClick.AddListener(ConfirmationButton);
        cancelButton.onClick.AddListener(CancelButton);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (returnPanel != null && returnPanel.activeSelf)
            {
                cancelButton.onClick.Invoke();
            }
            else
            {
                Return();
            }
        }
    }

    private void Start()
    {
        if (returnPanel != null)
        {
            returnPanel.SetActive(false);
        }
        if (confirmationText != null)
        {
            confirmationText.gameObject.SetActive(false);
        }
        if (totalCostText != null)
        {
            totalCostText.text = "Total: <b>0</b> F. Cod.";
        }
        if (descriptionPanel != null)
        {
            descriptionPanel.text = "";
            descriptionPanel.gameObject.SetActive(false);
        }
        UpdateTotalCost();
    }

    public void SelectUpgrade(string upgradeType, int cost)
    {
        if (selectedUpgrades.ContainsKey(upgradeType))
        {
            totalCost -= cost;
            selectedUpgrades.Remove(upgradeType);
        }
        else
        {
            totalCost += cost;
            selectedUpgrades[upgradeType] = cost;
        }
        UpdateTotalCost();
    }

    public bool IsUpgradeSelected(string type) => selectedUpgrades.ContainsKey(type);

    public void FinalizePurchase()
    {
        if (selectedUpgrades.Count == 0)
        {
            ShowConfirmation("No has seleccionado ninguna mejora.");
            return;
        }

        if (HUDManager.Instance.CurrentFragments < totalCost)
        {
            ShowConfirmation("No tienes suficientes fragmentos para comprar estas mejoras.");
            return;
        }

        HUDManager.Instance.DiscountInfoFragment(totalCost);

        foreach (var upgrade in selectedUpgrades)
        {
            switch (upgrade.Key)
            {
                case "Health": GeneralUpgradeManager.HealthLevel++; break;
                case "Shield": GeneralUpgradeManager.ShieldLevel++; break;
            }
        }

        GeneralUpgradeManager.NotifyStatsChanged();

        ShowConfirmation("Mejora(s) comprada(s) con éxito!");
        totalCost = 0;
        selectedUpgrades.Clear();
        UpdateTotalCost();
    }

    public void ShowDescription(string description)
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.gameObject.SetActive(true);
            descriptionPanel.text = description;
        }
    }
    public void HideDescription() => descriptionPanel.gameObject.SetActive(false);

    private void UpdateTotalCost() => totalCostText.text = $"Total: <b>{totalCost}</b> F. Cod.";

    private void Return()
    {
        if (returnPanel != null) returnPanel.SetActive(true);

        if (HUDManager.Instance.CurrentFragments >= 100)
        {
            confirmationPanelText.text = "Tienes <b>suficientes fragmentos</b> para <b>comprar</b>. ¿Seguro que deseas volver?";
        }
        else
        {
            confirmationPanelText.text = "¿Seguro que deseas volver?";
        }
    }

    private void ConfirmationButton()
    {
        if (generalUpgradeMenu != null) generalUpgradeMenu.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
        if (mainShopMenu != null) mainShopMenu.SetActive(true);

        selectedUpgrades.Clear();
        totalCost = 0;
        UpdateTotalCost();
        if (confirmationCoroutine != null) StopCoroutine(confirmationCoroutine);
        HideDescription();
    }

    private void CancelButton()
    {
        if (returnPanel != null) returnPanel.SetActive(false);
    }

    private void ShowConfirmation(string message)
    {
        if (confirmationCoroutine != null) StopCoroutine(confirmationCoroutine);
        confirmationCoroutine = StartCoroutine(ShowConfirmationCoroutine(message));
    }

    private IEnumerator ShowConfirmationCoroutine(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(confirmationDelay);
        confirmationText.gameObject.SetActive(false);
    }
}