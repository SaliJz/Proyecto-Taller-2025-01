using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Limpiar Missing Scripts")]
    static void CleanMissingScripts()
    {
        GameObject[] go = GameObject.FindObjectsOfType<GameObject>();
        int count = 0;

        foreach (GameObject g in go)
        {
            int result = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
            if (result > 0)
            {
                Debug.Log($"Removed {result} missing scripts from: {g.name}");
                count += result;
            }
        }

        Debug.Log($"Total missing scripts removed: {count}");
    }
}