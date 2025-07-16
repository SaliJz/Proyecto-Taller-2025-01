using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopItemController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración del Ítem (ScriptableObject)")]
    [SerializeField] private ShopItemData itemData;

    [Header("Referencias de UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI buttonText;
    [SerializeField] private Image[] upgradeIndicators; // Cuadritos para indicar niveles

    private ShopDataController shopController;

    private void Awake()
    {
        if (itemData == null)
        {
            Debug.LogError($"ShopItemController en {gameObject.name} no tiene un 'ShopItemData' asignado.", this);
            enabled = false; // Deshabilitar el script si no hay datos
            return;
        }

        shopController = GetComponentInParent<ShopDataController>();
        if (shopController == null)
        {
            Debug.LogError($"ShopItemController en {gameObject.name} no encontró un ShopStatsController en el padre. Asegúrate de que esté configurado correctamente.", this);
            enabled = false;
            return;
        }
        actionButton.onClick.AddListener(OnActionButtonClicked);
    }

    private void OnEnable()
    {
        UpdateVisuals();
    }

    public void UpdateVisuals()
    {
        if (itemData == null) return;

        if (iconImage != null) iconImage.sprite = itemData.itemIcon;
        string itemName = itemData.itemName;
        ItemType itemType = itemData.itemType;

        int currentLevel = itemData.GetCurrentLevel(itemName);
        bool isPurchased = itemData.IsPurchased(itemName);
        bool isMaxLevel = itemData.IsMaxLevel(itemName);

        for (int i = 0; i < upgradeIndicators.Length; i++)
        {
            if (upgradeIndicators[i] != null)
                upgradeIndicators[i].enabled = (i < currentLevel);
        }

        if (itemType == ItemType.Ability && !isPurchased)
        {
            buttonText.text = $"Comprar\n{itemData.purchaseCost} F. Cod.";
            actionButton.interactable = true;
        }
        else
        {
            if (isMaxLevel)
            {
                buttonText.text = "Máximo";
                actionButton.interactable = false;
            }
            else
            {
                int nextCost = itemData.CalculateCurrentCost(itemName);
                buttonText.text = $"Mejorar\n{nextCost} F. Cod.";
                actionButton.interactable = true;
            }
        }
    }

    private void OnActionButtonClicked()
    {
        if (shopController != null)
        {
            shopController.HandleItemClick(this);
        }
        else
        {
            Debug.LogError("ShopController no está inicializado. Asegúrate de que este script esté correctamente configurado.");
        }
    }

    public ShopItemData GetItemData() => itemData;

    public void OnPointerEnter(PointerEventData eventData) => shopController?.ShowDescription(itemData.description);
    public void OnPointerExit(PointerEventData eventData) => shopController?.HideDescription();
}