using UnityEngine;
using UnityEditor;

public class RemoveMissingScripts : MonoBehaviour
{
    [MenuItem("Tools/Limpiar Missing Scripts")]
    static void CleanMissingScripts()
    {
        if (Application.isPlaying)
        {
            Debug.LogWarning("No se puede limpiar scripts faltantes mientras el juego está en modo Play.");
            return;
        }

        int count = 0;
        var sceneObjects = GameObject.FindObjectsOfType<GameObject>();
        var prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        int total = sceneObjects.Length + prefabGuids.Length;
        int current = 0;

        // Limpiar objetos en la escena
        foreach (GameObject g in sceneObjects)
        {
            int result = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(g);
            if (result > 0)
            {
                Debug.Log($"Removed {result} missing scripts from: {g.name}");
                count += result;
            }
            EditorUtility.DisplayProgressBar("Limpiando Missing Scripts", $"Procesando {g.name}", (float)++current / total);
        }

        // Limpiar prefabs en Assets
        foreach (string guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
            {
                int result = GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);
                if (result > 0)
                {
                    Debug.Log($"Removed {result} missing scripts from prefab: {prefab.name} ({path})");
                    count += result;
                    EditorUtility.SetDirty(prefab);
                }
            }
            EditorUtility.DisplayProgressBar("Limpiando Missing Scripts", $"Procesando prefab {path}", (float)++current / total);
        }

        AssetDatabase.SaveAssets();
        EditorUtility.ClearProgressBar();
        EditorUtility.DisplayDialog("Limpieza completada", $"Total missing scripts removed: {count}", "OK");
    }
}