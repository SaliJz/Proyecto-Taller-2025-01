using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using static LevelManager_SQL;

public class Insert_Weapon : MonoBehaviour
{
    private string url = "https://progra251ch.samidareno.com/insert_weapon.php";
 
    public void Execute(WeaponType weaponType)
    {
        StartCoroutine(SendRequest(weaponType));
    }

    private IEnumerator SendRequest(WeaponType weaponType)
    {
        WWWForm form = new WWWForm();
        form.AddField("weapon_name",weaponType.currentWeaponType.ToString());
        form.AddField("player_id",1);/*LOGIN.currentPlayer_id*/
        form.AddField("bullet_spent",0); 

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                weaponType.weapon_id = int.Parse(www.downloadHandler.text);
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}
