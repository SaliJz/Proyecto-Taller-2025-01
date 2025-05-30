using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
[CreateAssetMenu(menuName = "Tutorial/New Scene")]
public class SceneTutorial : ScriptableObject
{
    public string sceneName;
    public List<DialogueData> dialogues;
    public UnityEvent events;
}
