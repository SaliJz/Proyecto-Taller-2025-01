using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class InsertUsuario : MonoBehaviour
{
    private string url = "http://localhost/login/insert.php";

    public void Execute(string player_name, string password)
    {
        StartCoroutine(SendRequest(player_name, password));
    }

    private IEnumerator SendRequest(string player_name, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_name", player_name);
        form.AddField("password", password);

        using(UnityWebRequest www = UnityWebRequest.Post(url,form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(www.downloadHandler.text);
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}
