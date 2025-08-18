using TMPro;
using UnityEngine;

public class ShopItemInstance : MonoBehaviour
{
    [Header("Referencias del Prefab")]
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;
    [SerializeField] private GameObject interactionPrompt;

    [Header("Colores de Texto")]
    [SerializeField] private Color healthColor = Color.red;
    [SerializeField] private Color shieldColor = Color.blue;
    [SerializeField] private Color abilityColor;
    [SerializeField] private Color weaponColor;
    [SerializeField] private Color priceColor = Color.yellow;

    private ShopItemData itemData;
    private ShopManager shopManager;

    private void Awake()
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(false);
        }
    }

    public void Initialize(ShopItemData data, ShopManager manager)
    {
        itemData = data;
        shopManager = manager;
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (itemData == null) return;

        int currentCost = itemData.CalculateCurrentCost(itemData.itemName);
        itemNameText.text = itemData.itemName;
        itemPriceText.text = $"{currentCost} F. Cod.";

        switch (itemData.itemType)
        {
            case ItemType.Health:
                itemNameText.color = healthColor;
                break;
            case ItemType.Shield:
                itemNameText.color = shieldColor;
                break;
            case ItemType.Ability:
                itemNameText.color = abilityColor;
                break;
            case ItemType.Weapon:
                itemNameText.color = weaponColor;
                break;
        }

        itemPriceText.color = priceColor;
    }

    public void BuyItem()
    {
        if (shopManager != null)
        {
            shopManager.PurchaseItem(itemData);
        }
    }

    public ShopItemData GetItemData()
    {
        return itemData;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(true);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (interactionPrompt != null)
            {
                interactionPrompt.SetActive(false);
            }
        }
    }
}