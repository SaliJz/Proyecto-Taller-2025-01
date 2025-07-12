using TMPro;
using UnityEngine;

public class DeathBodyController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI textMesh;

    public void SetText(string text)
    {
        if (textMesh != null)
        {
            textMesh.text = text;
        }
        else
        {
            Debug.LogWarning("TextMeshPro no está asignado en el prefab.");
        }
    }
}
