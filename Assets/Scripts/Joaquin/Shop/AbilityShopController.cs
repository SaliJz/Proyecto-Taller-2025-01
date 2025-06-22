using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityShopController : MonoBehaviour
{
    [SerializeField] private GameObject abilitySelectorMenu;
    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private TextMeshProUGUI descriptionPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private Button finalizeButton;
    [SerializeField] private Button equipButton;
    [SerializeField] private Button returnButton;

    [SerializeField] private float confirmationDelay = 1.5f;

    [SerializeField] private GameObject returnPanel;
    [SerializeField] private GameObject mainAbilityMenu;
    [SerializeField] private TextMeshProUGUI confirmationPanelText;
    [SerializeField] private Button confirmationButton;
    [SerializeField] private Button cancelButton;

    private List<GameObject> selectedAbilities = new();
    private static List<GameObject> purchasedAbilities = new();
    private static List<GameObject> equippedAbilities = new();
    private Coroutine confirmationCoroutine;
    private int totalCost = 0;

    public List<GameObject> SelectedAbilities => selectedAbilities;

    private void Awake()
    {
        finalizeButton.onClick.AddListener(FinalizePurchase);
        equipButton.onClick.AddListener(EquipAbilities);
        returnButton.onClick.AddListener(Return);

        confirmationButton.onClick.AddListener(ConfirmationButton);
        cancelButton.onClick.AddListener(CancelButton);
    }

    private void Start()
    {
        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
            {
                Debug.LogError("[AbilityShopController] AbilityManager no encontrado en la escena.");
            }
        }

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

    public bool TrySelectAbility(GameObject ability, int cost)
    {
        if (selectedAbilities.Contains(ability))
        {
            selectedAbilities.Remove(ability);
            if (!IsAbilityPurchased(ability)) totalCost -= cost;
            UpdateTotalCost();
            return false;
        }

        selectedAbilities.Add(ability);
        if (!IsAbilityPurchased(ability)) totalCost += cost;
        UpdateTotalCost();
        return true;
    }

    public bool IsAbilitySelected(GameObject ability)
    {
        return selectedAbilities.Contains(ability);
    }

    public bool IsAbilityPurchased(GameObject ability)
    {
        return purchasedAbilities.Contains(ability);
    }

    public bool IsAbilityEquipped(GameObject ability)
    {
        return equippedAbilities.Contains(ability);
    }

    public void FinalizePurchase()
    {
        if (selectedAbilities.Count == 0)
        {
            ShowConfirmation("No has seleccionado ninguna habilidad para comprar.");
            return;
        }

        if (HUDManager.Instance.CurrentFragments < totalCost)
        {
            ShowConfirmation("No tienes suficientes <b>F. Cod.</b> para comprar esta(s) habilidades.");
            return;
        }

        int newPurchases = 0;
        foreach (var ability in selectedAbilities)
        {
            if (IsAbilityPurchased(ability))
            {
                ShowConfirmation("Habilidad(es) ya comprada(s).");
                continue;
            }
            purchasedAbilities.Add(ability);
            newPurchases++;
        }

        if (newPurchases > 0)
        {
            ShowConfirmation("Habilidad(es) comprada(s) con éxito!");
            HUDManager.Instance.DiscountInfoFragment(totalCost);
        }
        else
        {
            ShowConfirmation("Ninguna habilidad nueva para comprar seleccionada.");
        }

        selectedAbilities.Clear();
        totalCost = 0;
        UpdateTotalCost();
    }

    public void EquipAbilities()
    {
        if (selectedAbilities.Count > 2)
        {
            ShowConfirmation("No se puede equipar más de 2 habilidades.");
            return;
        }

        if (selectedAbilities.Count == 0)
        {
            ShowConfirmation("No has seleccionado ninguna habilidad para equipar.");
            return;
        }

        foreach (var ability in selectedAbilities)
        {
            if (!IsAbilityPurchased(ability))
            {
                ShowConfirmation("Habilidad(es) no comprada(s).");
                return;
            }
        }

        equippedAbilities.Clear();
        abilityManager.ClearAbilities();

        foreach (var ability in selectedAbilities)
        {
            equippedAbilities.Add(ability);
            abilityManager.AddOrReplaceAbility(ability);
        }

        ShowConfirmation("Habilidades equipadas con éxito.");
        selectedAbilities.Clear();
    }

    public void ShowDescription(string abilityDescription)
    {
        if (descriptionPanel != null)
        {
            descriptionPanel.gameObject.SetActive(true);
            descriptionPanel.text = abilityDescription;
        }
    }

    public void HideDescription()
    {
        descriptionPanel.gameObject.SetActive(false);
    }

    private void UpdateTotalCost()
    {
        totalCostText.text = $"Total: <b>{totalCost}</b> F. Cod.";
    }

    private void Return()
    {
        if (returnPanel != null) returnPanel.SetActive(true);

        bool hasUnnequipedAbilities = purchasedAbilities.Any(p => !equippedAbilities.Contains(p));

        if (hasUnnequipedAbilities && equippedAbilities.Count < 2)
        {
            confirmationPanelText.text = "Tienes <b>habilidades compradas</b> sin <b>equipar</b>. ¿Seguro que deseas volver?";
        }
        else if (HUDManager.Instance.CurrentFragments >= 100) 
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
        if (abilitySelectorMenu != null) abilitySelectorMenu.SetActive(false);
        if (returnPanel != null) returnPanel.SetActive(false);
        if (mainAbilityMenu != null) mainAbilityMenu.SetActive(true);

        selectedAbilities.Clear();
        totalCost = 0;
        UpdateTotalCost();
        if (confirmationCoroutine != null) StopCoroutine(confirmationCoroutine);
        HideDescription();
    }

    private void CancelButton()
    {
        if (returnPanel != null) returnPanel.SetActive(false);
    }

    public void ShowConfirmation(string message)
    {
        if (confirmationCoroutine != null) StopCoroutine(confirmationCoroutine);
        confirmationCoroutine = StartCoroutine(ShowConfirmationCoroutine(message));
    }

    public IEnumerator ShowConfirmationCoroutine(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(confirmationDelay);
        confirmationText.gameObject.SetActive(false);
    }
}