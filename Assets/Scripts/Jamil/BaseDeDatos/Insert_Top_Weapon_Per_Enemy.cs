using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Insert_Top_Weapon_Per_Enemy : MonoBehaviour
{
    private string url = "http://localhost/proyecto_taller/insert_enemies_weapons.php";

    public void Execute(int enemy_id,int weapon_id)
    {
        StartCoroutine(SendRequest(enemy_id,weapon_id));
    }

    private IEnumerator SendRequest(int enemy_id,int weapon_id)
    {
        WWWForm form = new WWWForm();
        form.AddField("enemy_id", enemy_id);
        form.AddField("weapon_id", weapon_id);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Insertado con exito");
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}
