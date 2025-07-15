//using UnityEditor;
//using UnityEngine;

//public class MissingScriptFinderProject : MonoBehaviour
//{
//    [MenuItem("Tools/Buscar Missing Scripts en Proyecto")]
//    public static void FindAllMissingScripts()
//    {
//        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
//        string[] sceneGuids = AssetDatabase.FindAssets("t:Scene");
//        string[] allGuids = new string[prefabGuids.Length + sceneGuids.Length];
//        prefabGuids.CopyTo(allGuids, 0);
//        sceneGuids.CopyTo(allGuids, prefabGuids.Length);

//        int total = 0;

//        foreach (string guid in allGuids)
//        {
//            string path = AssetDatabase.GUIDToAssetPath(guid);
//            GameObject obj = AssetDatabase.LoadAssetAtPath<GameObject>(path);

//            if (obj == null) continue;

//            Component[] components = obj.GetComponentsInChildren<Component>(true);
//            for (int i = 0; i < components.Length; i++)
//            {
//                if (components[i] == null)
//                {
//                    Debug.LogWarning($"Missing Script en asset: {path}", obj);
//                    total++;
//                    break; // No es necesario repetir
//                }
//            }
//        }

//        if (total == 0) Debug.Log("No se encontraron scripts faltantes en prefabs ni escenas.");
//        else Debug.Log($"Total de assets con scripts faltantes: {total}");
//    }
//}