using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(menuName = "Tutorial/New Dialogue")]
public class DialogueData : ScriptableObject
{
    [TextArea(2, 5)]
    public string dialogueText;
    public AudioClip dialogueVoice;
    public float dialogueDuration;
}
