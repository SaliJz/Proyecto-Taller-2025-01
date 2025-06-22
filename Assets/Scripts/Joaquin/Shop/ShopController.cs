using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

public class ShopController : MonoBehaviour
{
    [SerializeField] private HUDManager hudManager;

    [SerializeField] private GameObject mainShopMenu;
    [SerializeField] private GameObject mainAbilityMenu;
    [SerializeField] private GameObject abilitySelectorMenu;
    [SerializeField] private GameObject abilityUpgradeMenu;
    [SerializeField] private GameObject weaponUpgradeMenu;
    [SerializeField] private GameObject generalUpgradeMenu;

    [SerializeField] private Button mainAbilityButton;
    [SerializeField] private Button mainAbilityMenuCloseButton;
    [SerializeField] private Button abilitySelector;
    [SerializeField] private Button abilityUpgrade;
    [SerializeField] private Button weaponUpgrade;
    [SerializeField] private Button generalUpgrade;

    [SerializeField] private Button skipButton;

    [SerializeField] private TextMeshProUGUI currentInfoFragments;

    private void Start()
    {
        if (mainAbilityMenu != null)
        {
            mainAbilityMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[ShopController] Main Ability Menu is not assigned in the ShopController.");
        }

        if (abilitySelectorMenu != null)
        {
            abilitySelectorMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[ShopController] Ability Selector Menu is not assigned in the ShopController.");
        }

        if (abilityUpgradeMenu != null)
        {
            abilityUpgradeMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[ShopController] Ability Selector Menu is not assigned in the ShopController.");
        }

        if (weaponUpgradeMenu != null)
        {
            weaponUpgradeMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[ShopController] Weapon Upgrade Menu is not assigned in the ShopController.");
        }

        if (generalUpgradeMenu != null)
        {
            generalUpgradeMenu.SetActive(false);
        }
        else
        {
            Debug.LogError("[ShopController] Shield Upgrade Menu is not assigned in the ShopController.");
        }

        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
            if (hudManager == null)
            {
                Debug.LogError("[AbilityShopController] HUDManager no encontrado en la escena.");
            }
        }
    }

    private void Awake()
    {
        mainAbilityButton.onClick.AddListener(OpenMainAbilityMenu);
        abilitySelector.onClick.AddListener(OpenAbilitySelectorMenu);
        abilityUpgrade.onClick.AddListener(OpenAbilityUpgradeMenu);
        weaponUpgrade.onClick.AddListener(OpenWeaponUpgradeMenu);
        generalUpgrade.onClick.AddListener(OpenGeneralUpgradeMenu);

        mainAbilityMenuCloseButton.onClick.AddListener(CloseMainAbilityMenu);

        skipButton.onClick.AddListener(CloseToShopMainMenu);
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        mainShopMenu.SetActive(true);
        currentInfoFragments.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (hudManager != null && currentInfoFragments != null)
        {
            if (HUDManager.Instance != null)
            {
                currentInfoFragments.text = $"Cantidad actual: <b>{HUDManager.Instance.CurrentFragments}</b> F. Cod.";
            }
        }
    }

    public void OpenMainAbilityMenu()
    {
        if (mainAbilityMenu != null)
        {
            mainShopMenu.SetActive(false);
            mainAbilityMenu.SetActive(true);
        }
    }

    public void CloseMainAbilityMenu()
    {
        if (mainAbilityMenu != null)
        {
            mainAbilityMenu.SetActive(false);
            mainShopMenu.SetActive(true);
        }
    }

    public void OpenAbilitySelectorMenu()
    {
        if (abilitySelectorMenu != null)
        {
            mainShopMenu.SetActive(false);
            mainAbilityMenu.SetActive(false);
            abilitySelectorMenu.SetActive(true);
        }
    }

    public void OpenAbilityUpgradeMenu()
    {
        if (abilityUpgradeMenu != null)
        {
            mainShopMenu.SetActive(false);
            mainAbilityMenu.SetActive(false);
            abilityUpgradeMenu.SetActive(true);
        }
    }

    public void OpenWeaponUpgradeMenu()
    {
        if (weaponUpgradeMenu != null)
        {
            mainShopMenu.SetActive(false);
            weaponUpgradeMenu.SetActive(true);
        }
    }

    public void OpenGeneralUpgradeMenu()
    {
        if (generalUpgradeMenu != null)
        {
            mainShopMenu.SetActive(false);
            generalUpgradeMenu.SetActive(true);
        }
    }

    public void CloseToShopMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        gameObject.SetActive(false);

        mainShopMenu.SetActive(false);
        mainAbilityMenu.SetActive(false);
        abilitySelectorMenu.SetActive(false);
        abilityUpgradeMenu.SetActive(false);
        weaponUpgradeMenu.SetActive(false);
        generalUpgradeMenu.SetActive(false);
        currentInfoFragments.gameObject.SetActive(false);
    }
}
