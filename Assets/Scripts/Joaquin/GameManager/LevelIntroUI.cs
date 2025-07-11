using System.Collections;
using TMPro;
using UnityEngine;

public class LevelIntroUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private float displayDuration = 3f;

    private void Start()
    {
        if (GameManager.Instance == null || levelText == null)
        {
            gameObject.SetActive(false);
            return;
        }

        int displayLevel = GameManager.Instance.CurrentLevelIndex;
        if (GameManager.Instance.CurrentLevelIndex <= 0) levelText.text = $"Nivel {displayLevel} (Tutorial)";
        else if (GameManager.Instance.CurrentLevelIndex >= 5) levelText.text = $"Nivel {displayLevel} (Jefe)";
        else levelText.text = $"Nivel {displayLevel}";
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