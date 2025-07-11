using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class InsertKillsCount : MonoBehaviour
{
    public static InsertKillsCount Instance;

    [SerializeField] int playerID;

    private int totalDestroyed = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        playerID = LOGIN.currentPlayer_id;
        Debug.Log($"InsertKillsCount → playerID sincronizado: {playerID}");
    }

    public void ReportDestruction(string tag)
    {
        if (tag == "Enemy")
        {
            totalDestroyed++;
            Debug.Log("Enemigos destruidos: " + totalDestroyed);
        }
    }

    //public int GetDestroyedCount() => totalDestroyed;

    public void SendDataToServer() => StartCoroutine(PostKills());

    private IEnumerator PostKills()
    {
        WWWForm form = new WWWForm();
        form.AddField("player_id", playerID);
        form.AddField("totalEnemyKills", totalDestroyed);

        using (UnityWebRequest www =
               UnityWebRequest.Post("http://localhost/EdenDataBase/InsertKills.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
                Debug.LogError("Error al enviar: " + www.error);
            else
                Debug.Log("Respuesta del servidor: " + www.downloadHandler.text);
        }
    }
}
