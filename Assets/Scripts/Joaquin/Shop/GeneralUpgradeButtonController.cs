using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GeneralUpgradeButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum UpgradeType { Health, Shield }

    [SerializeField] private UpgradeType upgradeType;

    [SerializeField] private int baseCost = 200;
    [SerializeField] private int costIncreasePerLevel = 100;
    [SerializeField] private string descriptionFormat = "Mejora {0} para TODAS las habilidades.";
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image buttonImage;

    private Button upgradeButton;
    private GeneralUpgradeShopController shopController;
    private Color lastColor;

    private void Awake()
    {
        shopController = GetComponentInParent<GeneralUpgradeShopController>();
        upgradeButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        costText = GetComponentInChildren<TextMeshProUGUI>();

        upgradeButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        UpdateUI();

        Color targetColor;

        if (shopController.IsUpgradeSelected(upgradeType.ToString()))
        {
            targetColor = Color.green;
        }
        else
        {
            targetColor = Color.white;
        }

        if (targetColor != lastColor)
        {
            buttonImage.color = targetColor;
            lastColor = targetColor;
        }
    }

    private void UpdateUI()
    {
        if (costText == null) return;

        int currentLevel = 0;
        int maxLevel = 0;

        switch (upgradeType)
        {
            case UpgradeType.Health:
                currentLevel = GeneralUpgradeManager.HealthLevel;
                maxLevel = GeneralUpgradeManager.MAX_HEALTH_UPGRADES;
                break;
            case UpgradeType.Shield:
                currentLevel = GeneralUpgradeManager.ShieldLevel;
                maxLevel = GeneralUpgradeManager.MAX_SHIELD_UPGRADES;
                break;
        }

        if (currentLevel >= maxLevel)
        {
            costText.text = "Máximo alcanzado";
            upgradeButton.interactable = false;
        }
        else
        {
            costText.text = $"Costo: {GetCurrentCost()} F. Cod.";
            upgradeButton.interactable = true;
        }
    }

    private int GetCurrentCost()
    {
        int currentLevel = upgradeType == UpgradeType.Health ? GeneralUpgradeManager.HealthLevel : GeneralUpgradeManager.ShieldLevel;
        return baseCost + (currentLevel * costIncreasePerLevel);
    }

    public void OnClick() => shopController.SelectUpgrade(upgradeType.ToString(), GetCurrentCost());
    public void OnPointerEnter(PointerEventData eventData) => shopController.ShowDescription(string.Format(descriptionFormat, upgradeType.ToString()));
    public void OnPointerExit(PointerEventData eventData) => shopController.HideDescription();
}
