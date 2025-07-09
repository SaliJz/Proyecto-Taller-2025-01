using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ActivationType
{
    NoActivationType,
    ByZona,
    ByKills,
    ForTime,
    ByEventoManual,
    ByFragments,
}
[System.Serializable]
[CreateAssetMenu(menuName = "Tutorial/New Dialogue")]
public class DialogueData : ScriptableObject
{
    [TextArea(1, 5)]
    public string dialogueText;
    public ActivationType activationType;
    public int necessarykills;
    public int necessaryFragments;
    public float necessaryTime;
    public AudioClip dialogueVoice;
    [HideInInspector] public bool isActive = false;
}