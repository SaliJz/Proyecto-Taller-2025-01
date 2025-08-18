using UnityEngine;

public class PlayerShopInteraction : MonoBehaviour
{
    private ShopItemInstance currentShopItem;

    private void Update()
    {
        if (currentShopItem != null && Input.GetKeyDown(KeyCode.E))
        {
            currentShopItem.BuyItem();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        ShopItemInstance shopItem = other.GetComponent<ShopItemInstance>();
        if (shopItem != null)
        {
            currentShopItem = shopItem;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        ShopItemInstance shopItem = other.GetComponent<ShopItemInstance>();
        if (shopItem != null && shopItem == currentShopItem)
        {
            currentShopItem = null;
        }
    }
}