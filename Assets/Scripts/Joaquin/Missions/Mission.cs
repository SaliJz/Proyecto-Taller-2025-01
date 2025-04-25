using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/New Mission")]
public class Mission : ScriptableObject
{
    public string missionName;
    public string enemyTag;
    public int targetAmount;

    [HideInInspector] public int currentAmount;

    public bool IsCompleted => currentAmount >= targetAmount;

    public void RegisterKill(string killedTag)
    {
        if (killedTag == enemyTag || enemyTag == "Any")
        {
            currentAmount++;
        }
    }

    public void ResetProgress()
    {
        currentAmount = 0;
    }
}
