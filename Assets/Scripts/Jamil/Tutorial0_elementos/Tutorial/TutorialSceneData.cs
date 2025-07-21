using System.Collections.Generic;
using UnityEngine;



[System.Serializable]
[CreateAssetMenu(menuName = "Tutorial/New Scene")]
public class TutorialSceneData : ScriptableObject
{
    public string sceneName;
    public List<DialogueData> dialogues;
}