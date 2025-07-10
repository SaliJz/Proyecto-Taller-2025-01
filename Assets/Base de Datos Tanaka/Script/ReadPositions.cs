using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class ReadPositions : MonoBehaviour
{
    public int player_id = 1;

    private string url = "http://localhost/workshop_project/get_player_locations.php";

    void Start()
    {
        StartCoroutine(LeerDatos());
    }

    IEnumerator LeerDatos()
    {
        string finalUrl = url + "?player_id=" + player_id;

        UnityWebRequest www = UnityWebRequest.Get(finalUrl);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error al leer datos: " + www.error);
        }
        else
        {
            Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
        }
    }
}
