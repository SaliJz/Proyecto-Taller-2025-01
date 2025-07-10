using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class InsertKillsCount : MonoBehaviour
{
    public static InsertKillsCount Instance;
    public int playerID = 0;
    public int totalDestroyed = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void ReportDestruction(string tag)
    {
        if (tag == "Enemy")
        {
            totalDestroyed++;
            Debug.Log("Enemigos destruidos: " + totalDestroyed);
        }
    }

    public int GetDestroyedCount()
    {
        return totalDestroyed;
    }
    public void SendDataToServer()
    {
        StartCoroutine(PostKills());
    }

    IEnumerator PostKills()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", playerID);
        form.AddField("totalEnemyKills", totalDestroyed);

        using (UnityWebRequest www = UnityWebRequest.Post("http://localhost/prographp/EdenDataBase/InsertKills.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error al enviar: " + www.error);
            }
            else
            {
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
            }
        }
    }
}
