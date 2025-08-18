using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class ShopManager : MonoBehaviour
{
    [Header("Referencias de la Tienda")]
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject shopItemPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private List<ShopItemData> allAvailableItems;

    [Header("UI del HUD")]
    [SerializeField] private TextMeshProUGUI currentInfoFragments;

    [Header("Configuración de Nivel")]
    [SerializeField] private bool isTutorial = false;
    [SerializeField] private bool buyOneAndDestroyOthers = true;

    private List<GameObject> spawnedItems = new List<GameObject>();

    private void Awake()
    {
        if (shopUI != null)
        {
            shopUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (HUDManager.Instance != null && currentInfoFragments != null && shopUI.activeSelf)
        {
            currentInfoFragments.text = $"Cantidad actual: <b>{HUDManager.Instance.CurrentFragments}</b> F. Cod.";
        }

        if (shopUI.activeSelf && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        shopUI.SetActive(true);
        if (currentInfoFragments != null)
        {
            currentInfoFragments.gameObject.SetActive(true);
        }
        GenerateShopItems();
    }

    public void CloseShop()
    {
        shopUI.SetActive(false);
        if (currentInfoFragments != null)
        {
            currentInfoFragments.gameObject.SetActive(false);
        }
        DestroySpawnedItems();

        if (!isTutorial)
        {
            GameManager.Instance?.LoadNextLevelAfterShop();
        }
    }

    private void GenerateShopItems()
    {
        DestroySpawnedItems();

        List<ShopItemData> availableItems = allAvailableItems
            .Where(item => !item.IsMaxLevel(item.itemName))
            .ToList();

        int itemsToSpawn = Mathf.Min(spawnPoints.Length, availableItems.Count);

        if (itemsToSpawn > 0)
        {
            List<ShopItemData> selectedItems = new List<ShopItemData>();
            for (int i = 0; i < itemsToSpawn; i++)
            {
                int randomIndex = Random.Range(0, availableItems.Count);
                selectedItems.Add(availableItems[randomIndex]);
                availableItems.RemoveAt(randomIndex);
            }

            for (int i = 0; i < itemsToSpawn; i++)
            {
                GameObject newItemInstance = Instantiate(shopItemPrefab, spawnPoints[i]);
                ShopItemInstance itemController = newItemInstance.GetComponent<ShopItemInstance>();
                if (itemController != null)
                {
                    itemController.Initialize(selectedItems[i], this);
                    spawnedItems.Add(newItemInstance);
                }
            }
        }
    }

    public void PurchaseItem(ShopItemData itemData)
    {
        int currentCost = itemData.CalculateCurrentCost(itemData.itemName);

        if (HUDManager.Instance.CurrentFragments < currentCost)
        {
            return;
        }

        HUDManager.Instance.DiscountInfoFragment(currentCost);
        itemData.IncrementLevel(itemData.itemName);

        if (itemData.itemType == ItemType.Ability && !itemData.IsPurchased(itemData.itemName))
        {
            DataManager.PurchaseAbility(itemData.itemName);
            GameObject abilityPrefab = FindObjectOfType<AbilityManager>().FindAbilityPrefabByName(itemData.itemName);
            if (abilityPrefab != null)
            {
                FindObjectOfType<AbilityManager>().AddOrReplaceAbility(abilityPrefab);
            }
        }

        if (buyOneAndDestroyOthers)
        {
            DestroySpawnedItems();
        }
        else
        {
            if (itemData.IsMaxLevel(itemData.itemName))
            {
                GameObject itemToDestroy = spawnedItems.FirstOrDefault(item => item.GetComponent<ShopItemInstance>().GetItemData() == itemData);
                if (itemToDestroy != null)
                {
                    spawnedItems.Remove(itemToDestroy);
                    Destroy(itemToDestroy);
                }
            }
        }
    }

    private void DestroySpawnedItems()
    {
        foreach (var item in spawnedItems)
        {
            Destroy(item);
        }
        spawnedItems.Clear();
    }
}