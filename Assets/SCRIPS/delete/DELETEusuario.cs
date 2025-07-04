using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class DELETEusuario : MonoBehaviour
{
    string url = "http://localhost/login/delete.php";

    public void BorrarUsuario(int player_id)
    {
        StartCoroutine(Enviar(player_id));
    }

    IEnumerator Enviar(int player_id)
    {
        WWWForm form = new WWWForm();

        form.AddField("player_id", player_id);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        Debug.Log(www.downloadHandler.text);
    }
}
