//using UnityEditor;
//using UnityEngine;

//public class FindMissingScripts : MonoBehaviour
//{
//    [MenuItem("Tools/Encontrar Missing Scripts")]
//    static void FindMissing()
//    {
//        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
//        int count = 0;

//        foreach (GameObject go in allObjects)
//        {
//            Component[] components = go.GetComponents<Component>();

//            for (int i = 0; i < components.Length; i++)
//            {
//                if (components[i] == null)
//                {
//                    Debug.LogWarning($"Missing Script en '{go.name}' => Escena: {go.scene.name}", go);
//                    count++;
//                }
//            }
//        }

//        if (count == 0) Debug.Log("No hay scripts faltantes en la escena.");
//        else Debug.Log($"Total de scripts faltantes encontrados: {count}");
//    }
//}