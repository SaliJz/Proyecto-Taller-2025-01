using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DeletePositions : MonoBehaviour
{
    [SerializeField] private int player_id = 1;

    private string url = "http://localhost/workshop_project/Delete_old_positions.php";

    public void Eliminar()
    {
        StartCoroutine(EliminarAntiguas());
    }

    IEnumerator EliminarAntiguas()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", player_id);

        UnityWebRequest www = UnityWebRequest.Post(url, form);
        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log("Error al eliminar: " + www.error);
        }
        else
        {
            Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
        }
    }

}
