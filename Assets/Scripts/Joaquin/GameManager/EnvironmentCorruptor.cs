using UnityEngine;

public class EnvironmentCorruptor : MonoBehaviour
{
    [Header("Configuración de Corrupción")]
    [SerializeField] private Material[] wallMaterials;
    [SerializeField] private Color startColor = Color.blue;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private float corruptionStartLevel = 2f;
    [SerializeField] private float corruptionEndLevel = 9f;
    /*
    private void Awake()
    {
        if (GameManager.Instance == null || wallMaterials.Length == 0) return;
        startColor = wallMaterials[0].GetColor("_EmissionColor");
    }
    */
    void Start()
    {
        if (GameManager.Instance == null || wallMaterials.Length == 0) return;

        int currentLevel = GameManager.Instance.CurrentLevelIndex;

        if (currentLevel < corruptionStartLevel)
        {
            SetMaterialColor(startColor);
            return;
        }

        float t = Mathf.InverseLerp(corruptionStartLevel, corruptionEndLevel, currentLevel);

        Color currentColor = Color.Lerp(startColor, endColor, t);
        SetMaterialColor(currentColor);
    }

    private void SetMaterialColor(Color color)
    {
        foreach (var mat in wallMaterials)
        {
            mat.SetColor("_EmissionColor", color);
        }
    }
}
