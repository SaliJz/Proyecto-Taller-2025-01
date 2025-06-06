using System.Collections;
using System.Collections.Generic;
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

    private AbilityShopController abilityShop;
    private AbilityInfo abilityInfo;
    private Color lastColor;

    public int Cost => cost;

    private void Awake()
    {
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

    private void Start()
    {


        abilityShop = GetComponentInParent<AbilityShopController>();
        abilityInfo = abilityObj.GetComponent<AbilityInfo>();
        buttonImage = abilityButton.GetComponent<Image>();
    }

    private void Update()
    {
        Color targetColor;

        if (abilityShop.PlayerHasAbility(abilityObj))
        {
            targetColor = Color.red;
            abilityCostText.text = "Habilidad ya obtenida";
        }
        else if (abilityShop.IsAbilitySelected(abilityObj))
        {
            targetColor = Color.green;
            abilityCostText.text = $"Costo: <b>{cost}</b> F. Cod. ";
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
        if (abilityShop.PlayerHasAbility(abilityObj))
        {
            abilityShop.ShowConfirmation("\n\nHabilidad ya obtenida.");
            return;
        }

        bool isNowSelected = abilityShop.TrySelectAbility(abilityObj, cost);

        if (isNowSelected)
        {
            abilityShop.ShowConfirmation("\n\nHabilidad seleccionada: " + abilityInfo.abilityName);
        }
        else
        {
            abilityShop.ShowConfirmation("\n\nHabilidad deseleccionada: " + abilityInfo.abilityName);
        }

        buttonImage.color = isNowSelected ? Color.green : Color.white;
    }

    public GameObject GetAbilityObject()
    {
        return abilityObj;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        abilityShop.ShowDescription(abilityInfo.description);
        Debug.Log($"[AbilityButtonController] Monstrando descripción por habilidad: {abilityInfo.description}");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        abilityShop.HideDescription();
    }
}