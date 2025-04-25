using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthBarText;

    [Header("Shield")]
    [SerializeField] private Slider shieldBar;
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("Ammo")]
    [SerializeField] private TextMeshProUGUI ammoText;

    [Header("Weapon")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIcon;

    [Header("Ability")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityStatusText;
    [SerializeField] private Image abilityIcon;

    [Header("Info Fragments")]
    [SerializeField] private RectTransform floatingTextObject;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Vector2 floatingStartPos = new Vector2(480f, 0f); // editable desde el inspector
    [SerializeField] private Vector2 floatingEndPos = new Vector2(-10, 0f);      // editable desde el inspector

    [Header("Mission UI")]
    [SerializeField] private RectTransform missionTextObject;
    [SerializeField] private float missionDisplayTime = 2f;
    [SerializeField] private Vector2 missionStartPos = new Vector2(480f, 0f); // editable desde el inspector
    [SerializeField] private Vector2 missionEndPos = new Vector2(-10f, 0f); // editable desde el inspector


    private Coroutine floatingTextCoroutine;
    private Coroutine missionCoroutine;

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
        floatingTextObject.gameObject.SetActive(false);
        missionTextObject.gameObject.SetActive(false);
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
    
    public void AddInfoFragment(int amount)
    {
        infoFragments += amount;

        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"Info Fragments: + {amount} -> {infoFragments}"));
    }

    private IEnumerator ShowFloatingText(string message)
    {
        if (floatingTextObject == null) yield break; // Asegura que el objeto no sea nulo

        TextMeshProUGUI text = floatingTextObject.GetComponentInChildren<TextMeshProUGUI>();
        floatingTextObject.gameObject.SetActive(true);
        text.text = message;

        // Setear posición inicial editable
        floatingTextObject.anchoredPosition = floatingStartPos;

        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            floatingTextObject.anchoredPosition = Vector2.Lerp(floatingStartPos, floatingEndPos, elapsed / displayDuration);
            yield return null;
        }

        floatingTextObject.gameObject.SetActive(false);
        floatingTextObject.anchoredPosition = floatingStartPos;
    }

    public void SpendFragments(int amount)
    {
        infoFragments -= amount;
        infoFragments = Mathf.Max(0, infoFragments);
        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"Info Fragments: - {amount} -> {infoFragments}"));
    }

    public void ShowMission(string message)
    {
        Debug.Log($"Mostrar misión: {message}");
        if (missionCoroutine != null)
        {
            StopCoroutine(missionCoroutine);
        }

        missionCoroutine = StartCoroutine(AnimateMissionPanel(message));
    }

    private IEnumerator AnimateMissionPanel(string message)
    {
        if (missionTextObject == null) yield break; // Asegura que el objeto no sea nulo

        TextMeshProUGUI text = missionTextObject.GetComponentInChildren<TextMeshProUGUI>();
        missionTextObject.gameObject.SetActive(true);
        text.text = message;

        // Setear posición inicial editable
        missionTextObject.anchoredPosition = missionStartPos;

        float elapsed = 0f;
        while (elapsed < missionDisplayTime)
        {
            elapsed += Time.deltaTime;
            missionTextObject.anchoredPosition = Vector2.Lerp(missionStartPos, missionEndPos, elapsed / missionDisplayTime);
            yield return null;
        }

        missionTextObject.gameObject.SetActive(false);
        missionTextObject.anchoredPosition = missionStartPos;
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
