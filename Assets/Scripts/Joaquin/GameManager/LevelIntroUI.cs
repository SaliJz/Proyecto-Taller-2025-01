using System.Collections;
using TMPro;
using UnityEditor.Animations;
using UnityEngine;

public class LevelIntroUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI missionText;
    [SerializeField] private Animator animatorController;
    [SerializeField] private string missionIntroduction;
    [SerializeField] private float displayDuration = 3f;
    
    private const int TutorialLevelIndex = 0;
    private const int BossLevelIndex = 5;
    private Coroutine currentRoutine;

    private void Awake()
    {
        if (levelText == null || missionText == null || animatorController == null)
        {
            Debug.LogError("LevelIntroUI: Alguna referencia no está asignada en el inspector.");
            enabled = false;
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int displayLevel = GameManager.Instance.CurrentLevelIndex;
        SetLevelText(displayLevel);
        if (currentRoutine != null) StopCoroutine(currentRoutine);
        currentRoutine = StartCoroutine(FadeOutRoutine());
    }

    private void SetLevelText(int levelIndex)
    {
        switch (levelIndex)
        {
            case TutorialLevelIndex:
                levelText.text = $"Nivel {levelIndex} (Tutorial)";
                break;
            case BossLevelIndex:
                levelText.text = $"Nivel {levelIndex} (Jefe)";
                break;
            default:
                levelText.text = $"Nivel {levelIndex}";
                break;
        }
    }

    private IEnumerator FadeOutRoutine()
    {
        if (levelText == null) yield break;

        Debug.Log("Iniciando fade out de texto de nivel: " + levelText.text);

        levelText.gameObject.SetActive(true);

        float elapsed = 0f;
        Color originalColor = levelText.color;

        while (elapsed < displayDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / displayDuration);
            levelText.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        levelText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        levelText.gameObject.SetActive(false);
        currentRoutine = null;
        if (animatorController != null) animatorController.SetTrigger("StartIntroduction");

        yield return new WaitForSeconds(5f);

        gameObject.SetActive(false);
    }
    /*
    private IEnumerator MissionIntroductionRoutine()
    {
        if (missionText == null) yield break;

        Debug.Log("Iniciando introducción de misión: " + missionIntroduction);

        missionText.text = missionIntroduction;

        // Forzar color base con alpha 0
        Color targetColor = missionText.color;
        targetColor.a = 0f;
        missionText.color = targetColor;

        // Asegurar que el objeto esté activo
        missionText.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < displayDuration)
        {
            float alpha = Mathf.Lerp(0f, 1f, elapsed / displayDuration);
            missionText.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        missionText.color = new Color(targetColor.r, targetColor.g, targetColor.b, 1f);
        currentRoutine = null;
        if (animatorController != null) animatorController.SetTrigger("StartIntroduction");

        Debug.Log("MissionIntroductionRoutine: Fade in completado y trigger enviado.");
    }
    */
}