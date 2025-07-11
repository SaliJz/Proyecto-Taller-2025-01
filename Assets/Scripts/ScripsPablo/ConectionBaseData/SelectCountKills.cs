using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;

public class Select : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI[] playerTexts;
    [System.Serializable]
    public class PlayerData
    {
        public int player_id;
        public string player_name;
        public int total_enemies;
    }
    [System.Serializable]
    public class LeaderboardResponse
    {
        public List<PlayerData> data;
    }
    void Start()
    {
        StartCoroutine(GetTopPlayers());
    }

    IEnumerator GetTopPlayers()
    {
        UnityWebRequest www = UnityWebRequest.Get("https://progra251ch.samidareno.com/CountKill.php");

        yield return www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            foreach (var text in playerTexts)
            {
                text.text = "Error al obtener datos.";
            }
        }
        else
        {
            string json = www.downloadHandler.text;
            LeaderboardResponse response = JsonUtility.FromJson<LeaderboardResponse>(json);

            for (int i = 0; i < playerTexts.Length; i++)
            {
                if (i < response.data.Count)
                {
                    var player = response.data[i];
                    playerTexts[i].text = $"{i + 1}. {player.player_name} - {player.total_enemies} kills";
                }
                else
                {
                    playerTexts[i].text = "";
                }
            }
        }
    }
}
