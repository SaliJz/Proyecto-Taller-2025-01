using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

public class LOGIN : MonoBehaviour
{
    public static int currentPlayer_id=0;
    public static LOGINMODE loginmode;
    private string url = "https://progra251ch.samidareno.com/select.php";

    public void Execute(string player_name, string password)
    {
        StartCoroutine(SendRequest(player_name, password));
    }

    private IEnumerator SendRequest(string player_name, string password)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_name", player_name);
        form.AddField("password", password);

        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                loginmode=JsonUtility.FromJson<LOGINMODE>(www.downloadHandler.text);
                currentPlayer_id = loginmode.data;
                Debug.Log(currentPlayer_id);
                if(loginmode.message=="success")
                {
                    SceneManager.LoadScene("MenuPrincipal");
                }
            }
            else
            {
                Debug.Log(www.error);
            }
        }
    }
}


 public class LOGINMODE
{
    public string message;
    public int data;
} 