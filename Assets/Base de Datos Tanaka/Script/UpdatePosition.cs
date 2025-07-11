using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class UpdatePosition : MonoBehaviour
{
    public int player_level_id = 1;
    public GameObject jugador;

    private string url = "http://localhost/workshop_project/Update_player_position.php";

    public void Actualizar()
    {
        Vector3 pos = jugador.transform.position;
        StartCoroutine(EnviarUpdate(pos));
    }

    IEnumerator EnviarUpdate(Vector3 pos)
    {
        WWWForm form = new WWWForm();
        form.AddField("player_level_id", player_level_id);
        form.AddField("axisX", pos.x.ToString("F3"));
        form.AddField("axisY", pos.y.ToString("F3"));
        form.AddField("axisZ", pos.z.ToString("F3"));

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error al actualizar datos: " + www.error);
        }
        else
        {
            Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
        }
    }

}
