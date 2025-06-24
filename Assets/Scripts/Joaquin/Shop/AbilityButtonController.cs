using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum ButtonFunction 
{ 
    PurchaseAbility, 
    EquipAbility,
    UnequipAbility,
    UpgradeStat 
}

public class AbilityButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Configuración Principal")]
    public AbilityType AssociatedAbility;
    public string UpgradeStatName;
    [TextArea] public string descriptionFormat;

    [Header("Configuración de Coste")]
    [SerializeField] private int baseCost = 100;
    [SerializeField] private int costPerLevel = 50;

    public ButtonFunction CurrentFunction { get; private set; }
    private Button button;
    private TextMeshProUGUI buttonText;
    private Image buttonImage;
    private AbilityShopController shopController;
    private const int MAX_UPGRADE_LEVEL = 5;

    private void Awake()
    {
        shopController = GetComponentInParent<AbilityShopController>();
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonImage = GetComponent<Image>();
        button.onClick.AddListener(() => shopController.HandleButtonClick(this));
    }

    public void UpdateVisuals()
    {
        bool isPurchased = AbilityShopDataManager.IsPurchased(AssociatedAbility);

        if (string.IsNullOrEmpty(UpgradeStatName))
        {
            if (isPurchased)
            {
                CurrentFunction = AbilityShopDataManager.IsEquipped(AssociatedAbility) ? ButtonFunction.UnequipAbility : ButtonFunction.EquipAbility;
            }
            else
            {
                CurrentFunction = ButtonFunction.PurchaseAbility;
            }
        }
        else
        {
            CurrentFunction = ButtonFunction.UpgradeStat;
        }

        UpdateAppearance();
    }

    private void UpdateAppearance()
    {
        switch (CurrentFunction)
        {
            case ButtonFunction.PurchaseAbility:
                buttonImage.color = shopController.IsSelected(this) ? Color.green : Color.white;
                buttonText.text = $"{AssociatedAbility}\nCosto: {baseCost}";
                button.interactable = true;
                break;
            case ButtonFunction.EquipAbility:
                buttonImage.color = Color.red;
                buttonText.text = "Equipar";
                button.interactable = true;
                break;
            case ButtonFunction.UnequipAbility:
                buttonImage.color = Color.blue;
                buttonText.text = "Equipado";
                button.interactable = true;
                break;
            case ButtonFunction.UpgradeStat:
                if (!AbilityShopDataManager.IsPurchased(AssociatedAbility))
                {
                    SetState(Color.grey, "Bloqueado", false);
                    return;
                }

                int currentLevel = GetCurrentUpgradeLevel();
                if (currentLevel >= MAX_UPGRADE_LEVEL)
                {
                    SetState(Color.yellow, "Máximo", false);
                }
                else
                {
                    Color color = shopController.IsSelected(this) ? Color.green : Color.white;
                    SetState(color, $"{UpgradeStatName}\nCosto: {GetCurrentCost()}", true);
                }
                break;
        }
    }

    public int GetCurrentCost()
    {
        if (CurrentFunction == ButtonFunction.PurchaseAbility) return baseCost;
        if (CurrentFunction == ButtonFunction.UpgradeStat)
        {
            return baseCost + (GetCurrentUpgradeLevel() * costPerLevel);
        }
        return 0;
    }

    private int GetCurrentUpgradeLevel()
    {
        AbilityStats stats = AbilityShopDataManager.GetStats(AssociatedAbility);
        switch (UpgradeStatName)
        {
            case "Cooldown": return stats.CooldownLevel;
            case "Duration": return stats.DurationLevel;
            case "Damage": return stats.DamageLevel;
            case "Range": return stats.RangeLevel;
            case "EnemiesAffected": return stats.EnemiesAffectedLevel;
            default: return 0;
        }
    }

    private void SetState(Color color, string text, bool interactable)
    {
        buttonImage.color = color;
        buttonText.text = text;
        button.interactable = interactable;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        shopController.ShowDescription(descriptionFormat);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopController.HideDescription();
    }
}