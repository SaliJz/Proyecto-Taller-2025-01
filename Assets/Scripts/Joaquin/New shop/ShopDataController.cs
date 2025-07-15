using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopDataController : MonoBehaviour
{
    [SerializeField] private AbilityManager abilityManager;

    [Header("Referencias de UI")]
    [SerializeField] private Button returnButton;
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private GameObject mainShopMenu;

    [Header("Feedback")]
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private float confirmationDisplayTime = 2f;
    private Coroutine confirmationCoroutine;

    private ShopItemController[] allShopItems;

    private void Awake()
    {
        returnButton.onClick.AddListener(CloseShop);
        allShopItems = GetComponentsInChildren<ShopItemController>();
    }

    private void OnEnable()
    {
        DataManager.OnDataChanged += UpdateAllItemVisuals;
        UpdateAllItemVisuals();
    }

    private void OnDisable()
    {
        DataManager.OnDataChanged -= UpdateAllItemVisuals;
    }

    public void HandleItemClick(ShopItemController item)
    {
        ShopItemData itemData = item.GetItemData();
        string itemName = itemData.itemName;
        ItemType itemType = itemData.itemType;
        int cost = itemData.CalculateCurrentCost(itemName);

        if (HUDManager.Instance.CurrentFragments < cost)
        {
            ShowConfirmation("No tienes suficientes fragmentos.");
            return;
        }

        HUDManager.Instance.DiscountInfoFragment(cost);

        if (itemType == ItemType.Ability)
        {
            if (!itemData.IsPurchased(itemName))
            {
                itemData.IncrementLevel(itemName);
                DataManager.PurchaseAbility(itemName);
                EquipAbilityByName(itemName);
                ShowConfirmation($"Habilidad '{itemName}' comprada y equipada!");
            }
            else
            {
                itemData.IncrementLevel(itemName);
                ShowConfirmation($"Habilidad '{itemName}' mejorada a Nivel {itemData.GetCurrentLevel(itemName)}!");
            }
        }
        else if (itemType == ItemType.Health || itemType == ItemType.Shield || itemType == ItemType.Weapon)
        {
            itemData.IncrementLevel(itemName);
            ShowConfirmation($"¡Mejora aplicada a {itemName}!");
        }
    }

    private void EquipAbilityByName(string abilityName)
    {
        GameObject abilityToEquip = abilityManager.FindAbilityPrefabByName(abilityName);
        if (abilityToEquip != null)
        {
            abilityManager.AddOrReplaceAbility(abilityToEquip);
            Debug.Log($"Habilidad '{abilityName}' equipada.");
        }
        else
        {
            Debug.LogError($"No se encontró el prefab de la habilidad '{abilityName}' en la lista 'allAbilities' del AbilityManager.");
        }
    }

    private void UpdateAllItemVisuals()
    {
        foreach (var item in allShopItems)
        {
            item.UpdateVisuals();
        }
    }

    private void CloseShop()
    {
        shopPanel.SetActive(false);
        mainShopMenu.SetActive(true);
        HideDescription();
    }

    public void ShowConfirmation(string message)
    {
        if (confirmationText == null) return;
        if (confirmationCoroutine != null) StopCoroutine(confirmationCoroutine);
        confirmationCoroutine = StartCoroutine(ShowConfirmationCoroutine(message));
    }

    private IEnumerator ShowConfirmationCoroutine(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(confirmationDisplayTime);
        confirmationText.gameObject.SetActive(false);
        confirmationCoroutine = null;
    }

    public void ShowDescription(string description)
    {
        if (descriptionText == null) return;
        descriptionText.text = description;
        descriptionText.gameObject.SetActive(true);
    }

    public void HideDescription()
    {
        if (descriptionText == null) return;
        descriptionText.text = string.Empty;
        descriptionText.gameObject.SetActive(false);
    }
}