using System;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;


public class InsertUsuario : MonoBehaviour
{
    public Action<string> OnMensajeRecibido;

    private string url = "https://progra251ch.samidareno.com/insert.php";

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

            string mensaje;
            if (www.result == UnityWebRequest.Result.Success)
            {
                mensaje = www.downloadHandler.text;
            }
            else
            {
                mensaje = "Error al conectar";
            }

            OnMensajeRecibido?.Invoke(mensaje);
            Debug.Log(mensaje);
        }
    }
}
