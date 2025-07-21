using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameFinisher : MonoBehaviour
{
    private void OnApplicationQuit()
    {
        if (!DataManager.IsTutorial)
        {
            SaveLoadManager.SaveGame();
        }
    }
}