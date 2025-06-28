using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ShopController : MonoBehaviour
{
    [SerializeField] private HUDManager hudManager;

    [SerializeField] private GameObject mainShopMenu;
    [SerializeField] private GameObject abilityShopMenu;
    [SerializeField] private GameObject weaponShopMenu;
    [SerializeField] private GameObject generalShopMenu;

    [SerializeField] private Button abilityShopButton;
    [SerializeField] private Button weaponShopButton;
    [SerializeField] private Button generalShopButton;

    [SerializeField] private Button skipButton;

    [SerializeField] private string nextScene = "";

    [SerializeField] private TextMeshProUGUI currentInfoFragments;

    private bool juegoPausado = false;

    private void Start()
    {
        if (mainShopMenu != null) mainShopMenu.SetActive(false);
        if (abilityShopMenu != null) abilityShopMenu.SetActive(false);
        if (weaponShopMenu != null) weaponShopMenu.SetActive(false);
        if (generalShopMenu != null) generalShopMenu.SetActive(false);

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
        abilityShopButton.onClick.AddListener(OpenAbilityShopMenu);
        weaponShopButton.onClick.AddListener(OpenWeaponShopMenu);
        generalShopButton.onClick.AddListener(OpenGeneralShopMenu);

        skipButton.onClick.AddListener(CloseToShopMainMenu);
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

    public void PauseGame()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;
        mainShopMenu.SetActive(true);
        currentInfoFragments.gameObject.SetActive(true);
        juegoPausado = true;
    }

    public void RestartGame()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
        mainShopMenu.SetActive(false);
        currentInfoFragments.gameObject.SetActive(false);
        juegoPausado = false;
    }

    public void OpenAbilityShopMenu()
    {
        if (abilityShopMenu != null)
        {
            mainShopMenu.SetActive(false);
            abilityShopMenu.SetActive(true);
        }
    }

    public void OpenWeaponShopMenu()
    {
        if (weaponShopMenu != null)
        {
            mainShopMenu.SetActive(false);
            weaponShopMenu.SetActive(true);
        }
    }

    public void OpenGeneralShopMenu()
    {
        if (generalShopMenu != null)
        {
            mainShopMenu.SetActive(false);
            generalShopMenu.SetActive(true);
        }
    }

    public void CloseToShopMainMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        SceneManager.LoadScene(nextScene);
        gameObject.SetActive(false);
    }
}