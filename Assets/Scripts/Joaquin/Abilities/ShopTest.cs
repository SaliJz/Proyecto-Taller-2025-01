using UnityEngine;

public class ShopTest : MonoBehaviour
{
    private ShopController shopController;

    private void Start()
    {
        shopController = FindObjectOfType<ShopController>();
        if (shopController == null)
        {
            Debug.LogError("ShopController not found in the scene.");
            return;
        }
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.L))
        {
            shopController.OpenShop();
        }
    }
}
