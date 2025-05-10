using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Missions/New Mission")]
public class Mission : ScriptableObject
{
    public string missionName;
    public List<KillCondition> killConditions = new();

    public bool IsCompleted => killConditions.All(k => k.IsMet);

    public void RegisterKill(string tag, string name, string tipo)
    {
        foreach (var cond in killConditions)
        {
            cond.Register(tag, name, tipo);
        }
    }

    public void ResetProgress()
    {
        foreach (var cond in killConditions)
        {
            cond.Reset();
        }
    }
}

[System.Serializable]
public class KillCondition
{
    public enum ConditionType { ByTag, ByName, ByType}

    public ConditionType conditionType;
    public string value;
    public int requiredAmount;

    [HideInInspector] public int currentAmount;

    public bool IsMet => currentAmount >= requiredAmount;

    public bool CheckMatch(string tag, string name, string tipo)
    {
        switch (conditionType)
        {
            case ConditionType.ByTag: return tag == value || value == "Any";
            case ConditionType.ByName: return name == value;
            case ConditionType.ByType: return tipo == value;
        }
        return false;
    }

    public void Register(string tag, string name, string tipo)
    {
        if (CheckMatch(tag, name, tipo))
        {
            currentAmount++;
            Log($"[Misión] Se registró un kill válido. Actual: " +
                $"{currentAmount}/{requiredAmount} ({conditionType}, valor esperado: {value})");
        }
        else
        {
            Log($"[Misión] Kill no coincide. tag: " +
                $"{tag}, name: {name}, tipo: {tipo} - Esperado: {conditionType} = {value}");
        }
    }

    public void Reset()
    {
        currentAmount = 0;
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}