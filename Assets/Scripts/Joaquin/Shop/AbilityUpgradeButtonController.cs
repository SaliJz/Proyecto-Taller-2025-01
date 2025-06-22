using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityUpgradeButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public enum UpgradeType { Cooldown, Duration, Range }

    [SerializeField] private UpgradeType upgradeType;

    [SerializeField] private int baseCost = 100;
    [SerializeField] private int costIncreasePerLevel = 50;
    [SerializeField] private string descriptionFormat = "Mejora {0} para TODAS las habilidades.";
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image buttonImage;

    private Button upgradeButton;
    private AbilityUpgradeShopController shopController;
    private Color lastColor;

    private void Awake()
    {
        shopController = GetComponentInParent<AbilityUpgradeShopController>();
        upgradeButton = GetComponent<Button>();
        buttonImage = GetComponent<Image>();
        costText = GetComponent<TextMeshProUGUI>();

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
            case UpgradeType.Cooldown:
                currentLevel = AbilityUpgradeManager.CooldownLevel;
                maxLevel = AbilityUpgradeManager.MAX_COOLDOWN_UPGRADES;
                break;
            case UpgradeType.Duration:
                currentLevel = AbilityUpgradeManager.EffectDurationLevel;
                maxLevel = AbilityUpgradeManager.MAX_DURATION_UPGRADES;
                break;
            case UpgradeType.Range:
                currentLevel = AbilityUpgradeManager.EffectRangeLevel;
                maxLevel = AbilityUpgradeManager.MAX_RANGE_UPGRADES;
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
        int currentLevel = 0;
        switch (upgradeType)
        {
            case UpgradeType.Cooldown: currentLevel = AbilityUpgradeManager.CooldownLevel; break;
            case UpgradeType.Duration: currentLevel = AbilityUpgradeManager.EffectDurationLevel; break;
            case UpgradeType.Range: currentLevel = AbilityUpgradeManager.EffectRangeLevel; break;
        }
        return baseCost + (currentLevel * costIncreasePerLevel);
    }

    public void OnClick()
    {
        shopController.SelectUpgrade(upgradeType.ToString(), GetCurrentCost());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        shopController.ShowDescription(string.Format(descriptionFormat, upgradeType.ToString()));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopController.HideDescription();
    }
}
