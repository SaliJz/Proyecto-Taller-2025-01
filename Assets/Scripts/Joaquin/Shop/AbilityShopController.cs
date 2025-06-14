using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AbilityShopController : MonoBehaviour
{
    [SerializeField] private GameObject shopPanel;
    [SerializeField] private AbilityManager abilityManager;
    [SerializeField] private HUDManager hudManager;
    [SerializeField] private TextMeshProUGUI totalCostText;
    [SerializeField] private TextMeshProUGUI currentInfoFragments;
    [SerializeField] private TextMeshProUGUI descriptionPanel;
    [SerializeField] private TextMeshProUGUI confirmationText;
    [SerializeField] private float confirmationDelay = 1.5f;
    [SerializeField] private Button finalizeButton;
    [SerializeField] private Button returnButton;

    private List<GameObject> selectedAbilities = new();
    private Coroutine confirmationCoroutine;
    private bool pauseGame = false;
    private int totalCost = 0;

    public List<GameObject> SelectedAbilities => selectedAbilities;

    private void Awake()
    {
        finalizeButton.onClick.AddListener(FinalizePurchase);
        returnButton.onClick.AddListener(Return);
    }

    private void Start()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("[AbilityShopController] Shop Panel no asignado en el AbilityShopController.");
        }

        if (abilityManager == null)
        {
            abilityManager = FindObjectOfType<AbilityManager>();
            if (abilityManager == null)
            {
                Debug.LogError("[AbilityShopController] AbilityManager no encontrado en la escena.");
            }
        }

        if (hudManager == null)
        {
            hudManager = FindObjectOfType<HUDManager>();
            if (hudManager == null)
            {
                Debug.LogError("[AbilityShopController] HUDManager no encontrado en la escena.");
            }
        }

        if (confirmationText != null)
        {
            confirmationText.gameObject.SetActive(false);
        }

        if (totalCostText != null)
        {
            totalCostText.text = "Total: <b>0</b> F. Cod.";
        }

        if (descriptionPanel != null)
        {
            descriptionPanel.text = "";
            descriptionPanel.gameObject.SetActive(false);
        }

        UpdateTotalCost();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!pauseGame) PauseGame();
            else RestartGame();
        }

        if (hudManager != null && currentInfoFragments != null)
        {
            if (HUDManager.Instance != null)
            {
                currentInfoFragments.text = $"Cantidad actual: <b>{HUDManager.Instance.CurrentFragments}</b> F. Cod.";
            }
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f;
        pauseGame = true;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if(shopPanel != null) shopPanel.SetActive(true);
    }

    private void RestartGame()
    {
        Time.timeScale = 1f;
        pauseGame = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        shopPanel.SetActive(false);
        Return();
    }

    public void ShowConfirmation(string message)
    {
        if (confirmationCoroutine != null)
        {
            StopCoroutine(confirmationCoroutine);
        }
        confirmationCoroutine = StartCoroutine(ShowConfirmationCoroutine(message));
    }

    public IEnumerator ShowConfirmationCoroutine(string message)
    {
        confirmationText.text = message;
        confirmationText.gameObject.SetActive(true);
        yield return new WaitForSecondsRealtime(confirmationDelay);
        confirmationText.gameObject.SetActive(false);
    }

    public bool TrySelectAbility(GameObject ability, int cost)
    {
        if (selectedAbilities.Contains(ability))
        {
            selectedAbilities.Remove(ability);
            totalCost -= cost;
            UpdateTotalCost();
            return false;
        }

        if (selectedAbilities.Count >= 2)
        {
            GameObject removed = selectedAbilities[0];
            AbilityButtonController removedButton = GetButtonControllerForAbility(removed);
            if (removedButton != null)
            {
                totalCost -= removedButton.Cost;
            }
            selectedAbilities.RemoveAt(0);
        }

        selectedAbilities.Add(ability);
        totalCost += cost;
        UpdateTotalCost();
        return true;
    }

    private AbilityButtonController GetButtonControllerForAbility(GameObject ability)
    {
        return GetComponentsInChildren<AbilityButtonController>()
            .FirstOrDefault(button => button.GetAbilityObject() == ability);
    }

    public bool PlayerHasAbility(GameObject ability)
    {
        return abilityManager.activedAbilities.Contains(ability);
    }

    public bool IsAbilitySelected(GameObject ability)
    {
        return selectedAbilities.Contains(ability);
    }

    public void ShowDescription(string abilityDescription)
    {
        if ( descriptionPanel != null)
        {
            descriptionPanel.text = abilityDescription;
            descriptionPanel.gameObject.SetActive(true);
        }
    }

    public void HideDescription()
    {
        descriptionPanel.gameObject.SetActive(false);
    }

    private void UpdateTotalCost()
    {
        totalCostText.text = $"Total: <b>{totalCost}</b> F. Cod.";
    }

    public void FinalizePurchase()
    {
        if (HUDManager.Instance.CurrentFragments < totalCost)
        {
            ShowConfirmation("No tienes suficientes <b>F. Cod.</b> para comprar esta(s) habilidades.");
            return;
        }

        ShowConfirmation("Habilidades compradas con éxito!");

        HUDManager.Instance.DiscountInfoFragment(totalCost);

        List<GameObject> current = abilityManager.activedAbilities;
        int currentCount = current.Count;

        for (int i = 0; i < selectedAbilities.Count; i++)
        {
            int replaceIndex = currentCount - 1 - i;
            if (replaceIndex >= 0)
            {
                abilityManager.ReplaceAbilityAt(replaceIndex, selectedAbilities[i]);
            }
            else
            {
                abilityManager.AddOrReplaceAbility(selectedAbilities[i]);
            }
        }

        selectedAbilities.Clear();
        totalCost = 0;
        UpdateTotalCost();
    }

    private void Return()
    {
        selectedAbilities.Clear();
        totalCost = 0;
        UpdateTotalCost();
        StopAllCoroutines();
        HideDescription();
    }
}