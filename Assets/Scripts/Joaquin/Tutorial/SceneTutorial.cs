using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActivationType
{
    ByZona,
    ByKills,
    ForTime,
    ByEventoManual,
    ByFragments,
}

[System.Serializable]
[CreateAssetMenu(menuName = "Tutorial/New Scene")]
public class SceneTutorial : ScriptableObject
{
    public string sceneName;
    public ActivationType activationType;
    public int necessarykills;
    public int necessaryFragments;
    public float necessaryTime;
    public List<DialogueData> dialogues;
}