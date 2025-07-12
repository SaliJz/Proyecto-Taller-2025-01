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
    public GameObject AbilityPrefab;
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
    private AbilityInfo abilityInfo;
    private const int MAX_UPGRADE_LEVEL = 5;

    private void Awake()
    {
        shopController = GetComponentInParent<AbilityShopController>();
        button = GetComponent<Button>();
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonImage = GetComponent<Image>();

        if (AbilityPrefab == null)
        {
            Debug.LogError($"[AbilityButtonController] El botón '{name}' no tiene asignado el AbilityPrefab.");
            return;
        }

        abilityInfo = AbilityPrefab.GetComponent<AbilityInfo>();
        if (abilityInfo == null)
        {
            Debug.LogError($"[AbilityButtonController] El prefab asignado en '{name}' no tiene componente AbilityInfo.");
            return;
        }

        button.onClick.AddListener(() => shopController.HandleButtonClick(this));
    }

    public void UpdateVisuals()
    {
        if (AbilityPrefab == null || shopController == null)
        {
            Debug.LogWarning($"[{name}] No se puede actualizar visuales porque falta asignar AbilityPrefab o ShopController.");
            return;
        }

        bool isPurchased = shopController.IsPurchased(this);
        bool isEquipped = shopController.IsEquipped(this);

        if (string.IsNullOrEmpty(UpgradeStatName))
        {
            if (isPurchased)
            {
                CurrentFunction = isEquipped ? ButtonFunction.UnequipAbility : ButtonFunction.EquipAbility;
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
                SetState(shopController.IsSelected(this) ? Color.green : Color.white, $"Comprar\nCosto: {baseCost}", true);
                break;
            case ButtonFunction.EquipAbility:
                SetState(Color.red, "Equipar", true);
                break;
            case ButtonFunction.UnequipAbility:
                SetState(Color.blue, "Equipado", true);
                break;
            case ButtonFunction.UpgradeStat:
                if (!shopController.IsPurchased(this))
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
                    SetState(color, $"\nCosto: {GetCurrentCost()} F. Cod.", true);
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
        AbilityStats stats = AbilityShopDataManager.GetStats(abilityInfo.abilityName);

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

    public void OnPointerEnter(PointerEventData eventData) => shopController.ShowDescription(descriptionFormat);
    public void OnPointerExit(PointerEventData eventData) => shopController.HideDescription();
}