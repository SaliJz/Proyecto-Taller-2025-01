using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class InsertEnemyController : MonoBehaviour
{
    private string url = "https://progra251ch.samidareno.com/insert_enemies.php";

    public void Execute(EnemyType enemy_type)
    {
        StartCoroutine(SendRequest(enemy_type));
    }

    private IEnumerator SendRequest(EnemyType enemy_type)
    {
        WWWForm form = new WWWForm();
        form.AddField("enemy_type", enemy_type.currentEnemyType.ToString());
      
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                enemy_type.enemy_id  = int.Parse(www.downloadHandler.text); 
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}


