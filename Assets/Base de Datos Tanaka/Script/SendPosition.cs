using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class SendPosition : MonoBehaviour
{
    public int player_id = 1;    
    public int level_id = 1;    
    private string url = "http://localhost/juego/insert_player_location.php";

    public void EnviarPosicionActual()
    {
        Vector3 posicion = transform.position;
        StartCoroutine(EnviarDatos(posicion));
    }

    IEnumerator EnviarDatos(Vector3 pos)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", player_id);
        form.AddField("level_id", level_id);
        form.AddField("axisX", pos.x.ToString("F3"));
        form.AddField("axisY", pos.y.ToString("F3"));
        form.AddField("axisZ", pos.z.ToString("F3"));

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error al enviar datos: " + www.error);
        }
        else
        {
            Debug.Log("Datos enviados: " + www.downloadHandler.text);
        }
    }
}

