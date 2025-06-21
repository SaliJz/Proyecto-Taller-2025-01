using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class AbilityButtonController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Button abilityButton;
    [SerializeField] private GameObject abilityObj;
    [SerializeField] private TextMeshProUGUI abilityCostText;
    [SerializeField] private Image buttonImage;
    [SerializeField] private int cost;

    private AbilityShopController shopController;
    private AbilityInfo abilityInfo;
    private Color lastColor;

    public int Cost => cost;

    private void Awake()
    {
        shopController = GetComponentInParent<AbilityShopController>();
        abilityInfo = abilityObj.GetComponent<AbilityInfo>();
        buttonImage = abilityButton.GetComponent<Image>();

        if (abilityObj == null)
        {
            Debug.LogError("[AbilityButtonController] Ability Object is not assigned in the AbilityButtonController.");
        }
        if (abilityCostText == null)
        {
            Debug.LogError("[AbilityButtonController] Cost Text is not assigned in the AbilityButtonController.");
        }

        abilityButton.onClick.AddListener(OnClick);
    }

    private void Update()
    {
        Color targetColor;

        if (shopController.IsAbilitySelected(abilityObj))
        {
            targetColor = Color.green;
            if (shopController.IsAbilityPurchased(abilityObj))
            {
                abilityCostText.text = "Seleccionado";
            }
            else
            {
                abilityCostText.text = $"Costo: <b>{cost}</b> F. Cod. ";
            }
        }
        else if (shopController.IsAbilityEquipped(abilityObj))
        {
            targetColor = Color.blue;
            abilityCostText.text = "Habilidad equipada";
        }
        else if (shopController.IsAbilityPurchased(abilityObj))
        {
            targetColor = Color.red;
            abilityCostText.text = "Habilidad comprada";
        }
        else
        {
            targetColor = Color.white;
            abilityCostText.text = $"Costo: <b>{cost}</b> F. Cod. ";
        }

        if (targetColor != lastColor)
        {
            buttonImage.color = targetColor;
            lastColor = targetColor;
        }
    }

    public void OnClick()
    {
        bool isNowSelected = shopController.TrySelectAbility(abilityObj, cost);

        if (isNowSelected)
        {
            shopController.ShowConfirmation("Habilidad seleccionada: " + abilityInfo.abilityName);
        }
        else
        {
            shopController.ShowConfirmation("Habilidad deseleccionada: " + abilityInfo.abilityName);
        }
    }

    public GameObject GetAbilityObject()
    {
        return abilityObj;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        shopController.ShowDescription(abilityInfo.description);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        shopController.HideDescription();
    }
}