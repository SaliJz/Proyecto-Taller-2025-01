using System;
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
    [SerializeField] private Image abilityCooldownFill;

    [Header("Info Fragments")]
    [SerializeField] private RectTransform floatingTextObject;
    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Vector2 floatingStartPos = new Vector2(480f, 0f); // editable desde el inspector
    [SerializeField] private Vector2 floatingEndPos = new Vector2(-10, 0f);      // editable desde el inspector
    [SerializeField] private TextMeshProUGUI currentFragmentsText;
    [SerializeField] private bool isNivel1 = false;

    [Header("Mission UI")]
    [SerializeField] private RectTransform missionTextObject;
    [SerializeField] private bool animateMission = false;
    [SerializeField] private float missionDisplayTime = 2f;
    [SerializeField] private Vector2 missionStartPos = new Vector2(480f, 0f); // editable desde el inspector
    [SerializeField] private Vector2 missionEndPos = new Vector2(-10f, 0f); // editable desde el inspector
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider missionProgressSlider;
    [SerializeField] private TextMeshProUGUI missionProgressText;

    [Header("Events")]
    [SerializeField] private Image eventIcon;

    private Coroutine floatingTextCoroutine;
    private Coroutine missionCoroutine;

    private static int infoFragments = 10000;
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
        if (isNivel1)
        {             
            infoFragments = 100;
        }

        floatingTextObject.gameObject.SetActive(false);

        if (!animateMission)
        {
            missionTextObject.anchoredPosition = missionEndPos;
            missionTextObject.gameObject.SetActive(true);
        }
        else
        {
            missionTextObject.gameObject.SetActive(false);
        }
    }

    public void UpdateHealth(int current, int max)
    {
        healthBar.value = (float)current / max;
        healthBarText.text = $"{current}";
    }

    public void UpdateShield(int current, int max)
    {
        if (shieldBar != null)
        {
            shieldBar.value = (float)current / max;
        }
        if (shieldText != null)
        {
            shieldText.text = $"{current}";
        }
    }

    public void UpdateAmmo(int current, int total)
    {
        ammoText.text = $"{current} / {total}";
    }

    public void UpdateWeaponName(string name)
    {
        if (weaponNameText == null || name == null) return; // Asegura que el objeto no sea nulo

        if (!weaponNameText.gameObject.activeSelf) weaponNameText.gameObject.SetActive(true);
        weaponNameText.text = name;
    }

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIcon == null || icon == null) return; // Asegura que el objeto y el icono no sean nulos
        
        if (!weaponIcon.gameObject.activeSelf) weaponIcon.gameObject.SetActive(true);
        weaponIcon.sprite = icon;
    }
    
    public void AddInfoFragment(int amount)
    {
        infoFragments += amount;
        infoFragments = Mathf.Max(0, infoFragments);
        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"F. Cod.: + {amount} -> {infoFragments}"));
        Debug.Log($"F. Cod.: + {amount} -> {infoFragments}");
    }

    public void DiscountInfoFragment(int amount)
    {
        infoFragments -= amount;
        infoFragments = Mathf.Max(0, infoFragments);
        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"F. Cod.: - {amount} -> {infoFragments}"));
        Debug.Log($"F. Cod.: - {amount} -> {infoFragments}");
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
        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"F. Cod.: - {amount} -> {infoFragments}"));
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            time = Mathf.Max(time, 0f);
            int hours = (int)(time / 3600);
            int minutes = (int)((time % 3600) / 60);
            int seconds = (int)(time % 60);
            timerText.text = $"{hours:00}:{minutes:00}:{seconds:00}";
        }
    }

    public void UpdateMissionProgress(float progress, float totalProgress, bool isEnemy = false)
    {
        missionProgressSlider?.gameObject.SetActive(true);
        float normalizedProgress = Mathf.Clamp01(progress / totalProgress) * 100;

        if (missionProgressSlider != null)
        {
            missionProgressSlider.value = normalizedProgress;
        }

        if (missionProgressText != null)
        {
            missionProgressText.text = $"{normalizedProgress:F0}%";
        }

        if (isEnemy)
        {
            missionProgressSlider.fillRect.GetComponent<Image>().color = Color.red; // Cambiar color a rojo si es enemigo
        }
        else
        {
            missionProgressSlider.fillRect.GetComponent<Image>().color = Color.green; // Cambiar color a verde si no es enemigo
        }
    }

    public void HideMission()
    {
        missionTextObject.gameObject?.SetActive(false);
        timerText.gameObject?.SetActive(false); 
    }

    public void ShowMission(string message, bool isTimer = false)
    {
        timerText.gameObject?.SetActive(isTimer);

        if (missionTextObject == null)
        {
            return;
        }

        if (!animateMission)
        {
            var text = missionTextObject.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null) text.text = message;
            return;
        }

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

    public void UpdateAbilityStatus(string abilityName, float cooldownRemaining, bool isReady, float cooldownTotal = 1f)
    {
        if (abilityNameText != null && !string.IsNullOrEmpty(abilityName))
        {
            if (!abilityNameText.gameObject.activeSelf) abilityNameText.gameObject.SetActive(true);
            abilityNameText.text = abilityName;
        }
        else
        {
            if (abilityNameText != null && abilityNameText.gameObject.activeSelf)
            {
                abilityNameText.gameObject.SetActive(false);
            }
        }

        if (abilityCooldownFill != null && cooldownRemaining <= 0)
        {
            abilityCooldownFill.gameObject.SetActive(false);
        }

        if (isReady)
        {
            abilityStatusText.text = $"¡Listo!";
            if (abilityCooldownFill != null)
            {
                abilityCooldownFill.fillAmount = 0f;
            }
        }
        else
        {
            abilityStatusText.text = $"Cooldown - {Mathf.Ceil(cooldownRemaining)}s";
            if (abilityCooldownFill != null)
            {
                abilityCooldownFill.fillAmount = cooldownRemaining / cooldownTotal;
            }
        }
    }

    public void UpdateAbilityUI(GameObject abilityObj)
    {
        if (abilityIcon != null && abilityObj != null)
        {
            if (abilityIcon.gameObject.activeSelf) abilityIcon.gameObject.SetActive(false);
        }

        AbilityInfo info = abilityObj.GetComponent<AbilityInfo>();

        if (info != null)
        {
            if (!abilityIcon.gameObject.activeSelf) abilityIcon.gameObject.SetActive(true);
            abilityIcon.sprite = info.icon;
            if (!abilityNameText.gameObject.activeSelf) abilityNameText.gameObject.SetActive(true);
            abilityNameText.text = info.abilityName;
        }
    }

    public void UpdateIcon(Sprite newIcon)
    {
        if (eventIcon != null && newIcon != null)
        {
            if (!eventIcon.gameObject.activeSelf) eventIcon.gameObject.SetActive(true);
            eventIcon.sprite = newIcon;
        }
    }

    public void HideIcon()
    {
        if (eventIcon != null)
        {
            if (eventIcon.gameObject.activeSelf) eventIcon.gameObject.SetActive(false);
        }
    }
}
