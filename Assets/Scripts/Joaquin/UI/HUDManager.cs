using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    #region Datos

    public static HUDManager Instance
    {
        get;
        private set;
    }

    [Header("HUD Elements")]
    [Header("Health")]
    [SerializeField] private Slider healthBar;
    [SerializeField] private TextMeshProUGUI healthBarText;

    [Header("Shield")]
    [SerializeField] private Slider shieldBar;
    [SerializeField] private TextMeshProUGUI shieldText;

    [Header("Ammo")]
    [SerializeField] private List<TextMeshProUGUI> ammoTexts;

    [Header("Weapon")]
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIcon;

    [Header("Ability")]
    [SerializeField] private TextMeshProUGUI abilityNameText;
    [SerializeField] private TextMeshProUGUI abilityStatusText;
    [SerializeField] private Image abilityIcon;
    [SerializeField] private Image abilityIconBackground;
    [SerializeField] private Image abilityCooldownFill;
    [SerializeField] private bool showAbilityUI = false; // Si se debe mostrar la HUD de habilidades

    [Header("Info Fragments")]
    [SerializeField] private RectTransform floatingInfoFragmentsText;
    [SerializeField] private TextMeshProUGUI floatingInfoFragmentsTextContent;
    [SerializeField] private TextMeshProUGUI currentInfoFragments;
    private bool activecurrentInfoFragment = false;

    [SerializeField] private float displayDuration = 2f;
    [SerializeField] private Vector2 floatingStartPos = new Vector2(480f, 0f);
    [SerializeField] private Vector2 floatingEndPos = new Vector2(-10, 0f);
    [SerializeField] private bool isTutorial = false;
 
    [Header("Mission UI")]
    [SerializeField] private RectTransform missionTextObject;
    [SerializeField] private GameObject missionPanel;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private bool animateMission = false;
    [SerializeField] private float missionDisplayTime = 2f;
    [SerializeField] private Vector2 missionStartPos = new Vector2(480f, 0f);
    [SerializeField] private Vector2 missionEndPos = new Vector2(-10f, 0f);
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Slider missionProgressSlider;
    [SerializeField] private TextMeshProUGUI missionProgressText;

    [Header("Events")]
    [SerializeField] private Image eventIcon;

    private Coroutine floatingTextCoroutine;
    private Coroutine missionCoroutine;

    private static int infoFragments = 2000;
    public int CurrentFragments => infoFragments;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
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
    }

    private void Start()
    {
        if (isTutorial)
        {             
            infoFragments = 100;
        }

        if (ammoTexts != null)
        {
            foreach (var text in ammoTexts)
            {
                text.gameObject.SetActive(false);
            }
        }

        if (floatingInfoFragmentsText != null) floatingInfoFragmentsText.gameObject.SetActive(false);

        if (missionPanel != null) missionPanel.SetActive(false);
        if (missionText != null) missionText.gameObject.SetActive(false);
        if (timerText != null) timerText.gameObject.SetActive(false);
        if (missionProgressSlider != null) missionProgressSlider.gameObject.SetActive(false);
        if (missionProgressText != null) missionProgressText.gameObject.SetActive(false);

        if (eventIcon != null) eventIcon.gameObject.SetActive(false);

        if (!showAbilityUI)
        {
            if (abilityIcon != null) abilityIcon.gameObject.SetActive(false);
            if (abilityIconBackground != null) abilityIconBackground.gameObject.SetActive(false);
            if (abilityCooldownFill != null) abilityCooldownFill.gameObject.SetActive(false);
        }
        //if (abilityNameText != null) abilityNameText.gameObject.SetActive(false);
        //if (abilityStatusText != null) abilityStatusText.gameObject.SetActive(false);

        if (weaponIcon != null) weaponIcon.gameObject.SetActive(false);
        if (weaponNameText != null) weaponNameText.gameObject.SetActive(false);

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

    private void OnEnable()
    {
        AbilityManager.OnAbilityStateChanged += UpdateAbilityUI;
    }

    private void OnDisable()
    {
        AbilityManager.OnAbilityStateChanged -= UpdateAbilityUI;
    }

    public void ShowAbilityUI(bool show)
    {
        showAbilityUI = show;
    }

    #endregion

    #region Bars and Text Updates

    public void UpdateHealth(int current, int max)
    {
        if (healthBar != null && healthBarText != null)
        {
            Debug.Log($"Updating health: {current}/{max}");

            float healthPercentage = (float)current / max;
            healthBar.value = healthPercentage;
            healthBarText.text = $"{healthPercentage * 100} %";
        }
    }

    public void UpdateShield(int current, int max)
    {
        if (shieldBar != null && shieldText != null)
        {
            Debug.Log($"Updating shield: {current}/{max}");

            float shieldPercentage = (float)current / max;
            shieldBar.value = shieldPercentage;
            shieldText.text = $"{shieldPercentage * 100} %";
        }
    }

    #endregion

    #region Weapon and Ammo Updates

    public void UpdateAmmo(int weaponIndex, int current, int total)
    {
        if (ammoTexts == null || weaponIndex < 0 || weaponIndex >= ammoTexts.Count) return;

        // Desactiva todos los textos de munici�n
        foreach (var text in ammoTexts)
        {
            text.gameObject.SetActive(false);
        }

        // Activa y actualiza solo el texto del arma actual
        var currentAmmoText = ammoTexts[weaponIndex];
        currentAmmoText.gameObject.SetActive(true);
        currentAmmoText.text = $"{current} / {total}";
    }

    public void UpdateWeaponName(string name)
    {
        if (weaponNameText != null || name != null)
        {
            //if (!weaponNameText.gameObject.activeSelf) weaponNameText.gameObject.SetActive(true);
            weaponNameText.text = name;
        }
    }

    public void UpdateWeaponIcon(Sprite icon)
    {
        if (weaponIcon)
        {
            if (!weaponIcon.gameObject.activeSelf) weaponIcon.gameObject.SetActive(true);
            weaponIcon.sprite = icon;
        }
    }

    #endregion 

    #region Info Fragments
    private void Update()
    {
        if (floatingInfoFragmentsText.gameObject.activeInHierarchy && !activecurrentInfoFragment)
        {
            activecurrentInfoFragment = true;
            currentInfoFragments.transform.gameObject.SetActive(true);
        }
    }
    public void AddInfoFragment(int amount)
    {
        infoFragments += amount;
        infoFragments = Mathf.Max(0, infoFragments);
        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        if (floatingInfoFragmentsText.gameObject.activeInHierarchy)
        {
            floatingTextCoroutine = StartCoroutine(ShowFloatingText($"|F. Cod.: + {amount}"));
            currentInfoFragments.text = $"|F. Cod. {infoFragments.ToString("N0")}"; // Formatear con separador de miles
        }

        //Debug.Log($"F. Cod.: + {amount} -> {infoFragments}");
    }

    public void DiscountInfoFragment(int amount)
    {
        infoFragments -= amount;
        infoFragments = Mathf.Max(0, infoFragments);
        if (floatingTextCoroutine != null)
        {
            StopCoroutine(floatingTextCoroutine);
        }

        floatingTextCoroutine = StartCoroutine(ShowFloatingText($"|F. Cod.: - {amount}"));
        currentInfoFragments.text = $"|F. Cod. {infoFragments.ToString("N0")}"; // Formatear con separador de miles
        Debug.Log($"F. Cod.: - {amount} -> {infoFragments}");
    }

    private IEnumerator ShowFloatingText(string message)
    {
        if (floatingInfoFragmentsText == null) yield break; // Asegura que el objeto no sea nulo

        TextMeshProUGUI text = floatingInfoFragmentsText.GetComponentInChildren<TextMeshProUGUI>();
        floatingInfoFragmentsText.gameObject.SetActive(true);
        text.text = message;

        // Setear posici�n inicial editable
        floatingInfoFragmentsText.anchoredPosition = floatingStartPos;

        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            elapsed += Time.deltaTime;
            floatingInfoFragmentsText.anchoredPosition = Vector2.Lerp(floatingStartPos, floatingEndPos, elapsed / displayDuration);
            yield return null;
        }

        floatingInfoFragmentsText.gameObject.SetActive(false);
        floatingInfoFragmentsText.anchoredPosition = floatingStartPos;
    }

    public void SpendFragments(int amount)
    {
        infoFragments -= amount;
        infoFragments = Mathf.Max(0, infoFragments);
        if (floatingInfoFragmentsText.gameObject.activeInHierarchy)
        {
            floatingTextCoroutine = StartCoroutine(ShowFloatingText($"F. Cod.: + {amount} -> {infoFragments}"));
        }
    }

    #endregion

    #region Mission UI

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
        float normalizedProgress = Mathf.Clamp01(progress / totalProgress);

        if (missionProgressSlider != null)
        {
            missionProgressSlider.value = normalizedProgress;
        }

        if (missionProgressText != null)
        {
            missionProgressText.text = $"{normalizedProgress * 100:F0}%";
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
        //Debug.Log($"[HUDManager] Mostrando mensaje: {message}");

        missionPanel?.SetActive(true);
        missionText.gameObject?.SetActive(true);
        timerText.gameObject?.SetActive(isTimer);

        if (missionTextObject == null)
        {
            Debug.LogWarning("No se ha asignado missionTextObject");
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

        missionTextObject.gameObject.SetActive(true);
        floatingInfoFragmentsTextContent.text = message;

        // Setear posici�n inicial editable
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
        //if (abilityNameText != null || abilityName != null)
        //{
        //    if (!abilityNameText.gameObject.activeSelf) abilityNameText.gameObject.SetActive(true);
        //    abilityNameText.text = abilityName;
        //}

        switch (abilityName)
        {
            case "GlitchTime":
                abilityIcon.color = Color.gray;
                break;
            case "ElectroHack":
                abilityIcon.color = Color.cyan;
                break;
            case "IgnitionCode":
                abilityIcon.color = Color.red;
                break;
            case "Mindjack":
                abilityIcon.color = Color.green;
                break;
            default:
                abilityIcon.color = Color.white;
                break;
        }

        if (isReady)
        {
            //if (abilityNameText != null)
            //{
            //    if (!abilityStatusText.gameObject.activeSelf) abilityStatusText.gameObject.SetActive(true);
            //    abilityStatusText.text = $"�Listo!";
            //}

            if (abilityCooldownFill != null)
            {
                abilityCooldownFill.fillAmount = 0f;
                if (abilityCooldownFill.gameObject.activeSelf) abilityCooldownFill.gameObject.SetActive(false);
            }
        }
        else
        {
            //if (abilityNameText != null)
            //{
            //    if (!abilityStatusText.gameObject.activeSelf) abilityStatusText.gameObject.SetActive(true);
            //    abilityStatusText.text = $"Cooldown - {Mathf.Ceil(cooldownRemaining)}s";
            //}

            if (abilityCooldownFill != null)
            {
                if (!abilityCooldownFill.gameObject.activeSelf) abilityCooldownFill.gameObject.SetActive(true);
                abilityCooldownFill.fillAmount = cooldownRemaining / cooldownTotal;
            }
        }
    }

    public void UpdateAbilityUI(GameObject abilityObj)
    {
        if (abilityObj == null)
        {
            if (abilityIcon) abilityIcon.gameObject.SetActive(false);
            if (abilityIconBackground) abilityIconBackground.gameObject.SetActive(false);
            //if (abilityNameText) abilityNameText.gameObject.SetActive(false);
            //if (abilityStatusText) abilityStatusText.gameObject.SetActive(false);
            Debug.LogWarning("Ability object is null, hiding ability UI.");
            return;
        }

        Debug.Log($"Updating ability UI for: {abilityObj.name}");

        if (abilityIcon) abilityIcon.gameObject.SetActive(true);
        if (abilityIconBackground) abilityIconBackground.gameObject.SetActive(true);

        AbilityInfo info = abilityObj.GetComponent<AbilityInfo>();

        if (info)
        {
            if (abilityIcon) abilityIcon.sprite = info.icon;
            //if (abilityNameText)
            //{
            //    abilityNameText.gameObject.SetActive(true);
            //    abilityNameText.text = info.abilityName;
            //}
        }
    }

    #endregion

    #region Events

    public void UpdateIcon(Sprite newIcon)
    {
        if (eventIcon != null || newIcon != null)
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

    #endregion
}