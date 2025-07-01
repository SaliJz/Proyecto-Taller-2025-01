using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelIntroUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float displayDuration = 3f;

    private void Awake()
    {
        if (levelText != null)
        {
            levelText.text = string.Empty;
        }
    }

    private void Start()
    {
        if (GameManager.Instance == null || levelText == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int displayLevel = GameManager.Instance.CurrentLevelIndex + 1;
        levelText.text = $"Nivel {displayLevel}";
        StartCoroutine(FadeOutRoutine());
    }

    private IEnumerator FadeOutRoutine()
    {
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
        gameObject.SetActive(false);
    }
}