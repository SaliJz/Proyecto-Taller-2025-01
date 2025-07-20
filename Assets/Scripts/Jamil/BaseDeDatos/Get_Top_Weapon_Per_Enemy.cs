using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class Get_Top_Weapon_Per_Enemy : MonoBehaviour
{
    LevelManager_SQL levelManager_SQL;

    private void OnEnable()
    {
        StartCoroutine(WaitForLevelManager());
    }
    private string url = "http://localhost/proyecto_taller/get_top_weapon_per_enemy.php";

    private IEnumerator WaitForLevelManager()
    {
        while (LevelManager_SQL.Instance == null)
        {
            yield return null; // Espera un frame
        }

        levelManager_SQL = LevelManager_SQL.Instance;
    }
    public void Execute()
    {
        StartCoroutine(SendRequest());
    }

    private IEnumerator SendRequest()
    {
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                SelectEnemyWeaponStatModel model = JsonUtility.FromJson<SelectEnemyWeaponStatModel>(www.downloadHandler.text);
                levelManager_SQL.AssignValuesToEnemyKillData(model);
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }

}
