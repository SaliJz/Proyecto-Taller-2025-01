using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance 
    { 
        get; 
        private set; 
    }

    [Header("UI Elements")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthBarText;

    [SerializeField] private Slider shieldBar;
    [SerializeField] private TextMeshProUGUI shieldText;

    [SerializeField] private TextMeshProUGUI ammoText;

    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIcon;

    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityStatusText;
    [SerializeField] private Image abilityIcon;

    [SerializeField] private TextMeshProUGUI infoFragmentsText;

    [Header("Floating Info Fragment Text")]
    [SerializeField] private GameObject floatingTextObject;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private float floatSpeed = 50f;

    private Coroutine floatingTextCoroutine;

    private int infoFragments = 0;
    public int CurrentFragments => infoFragments;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        floatingTextObject.SetActive(false);
    }

    public void UpdateHealth(int current, int max)
    {
        healthBar.value = (float)current / max;
        healthBarText.text = $"{current} / {max}";
    }

    public void UpdateShield(int current, int max)
    {
        if (shieldBar != null)
        {
            shieldBar.value = (float)current / max;
        }
        if (shieldText != null)
        {
            shieldText.text = $"{current} / {max}";
        }
    }

    public void UpdateAmmo(int current, int total)
    {
        ammoText.text = $"{current} / {total}";
    }

    public void UpdateWeaponName(string name)
    {
        weaponNameText.text = name;
    }

    public void UpdateWeaponIcon(Sprite icon)
    {
        weaponIcon.sprite = icon;
    }
    
    public void AddInfoFragment(int fragments)
    {
        infoFragments += fragments;
        infoFragmentsText.text = "Info Fragments: " + infoFragments;

        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"Info Fragments: + {fragments}"));
    }

    private IEnumerator ShowFloatingText(string message)
    {
        if (floatingTextObject == null) yield break; // Asegura que el objeto no sea nulo

        TextMeshProUGUI text = floatingTextObject.GetComponent<TextMeshProUGUI>();
        Vector3 originalPosition = floatingTextObject.transform.localPosition;

        floatingTextObject.SetActive(true);
        text.text = message;

        float elapsed = 0f;

        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            floatingTextObject.transform.localPosition = originalPosition + Vector3.right * (floatSpeed * (elapsed / displayDuration));
            yield return null;
        }

        floatingTextObject.SetActive(false);
        floatingTextObject.transform.localPosition = originalPosition;
    }

    public void SpendFragments(int amount)
    {
        infoFragments -= amount;
        infoFragments = Mathf.Max(0, infoFragments);
        infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }

    public void UpdateAbilityStatus(string abilityName, float cooldownRemaining, bool isReady)
    {
        if (isReady)
        {
            abilityStatusText.text = $"¡Listo!";
        }
        else
        {
            abilityStatusText.text = $"Cooldown - {Mathf.Ceil(cooldownRemaining)}s";
        }
    }

    public void UpdateAbilityUI(GameObject abilityObj)
    {
        AbilityInfo info = abilityObj.GetComponent<AbilityInfo>();
        if (info != null)
        {
            abilityIcon.sprite = info.icon;
            abilityNameText.text = info.abilityName;
        }
    }
}
