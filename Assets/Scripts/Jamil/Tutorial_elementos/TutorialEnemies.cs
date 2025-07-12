using UnityEngine;

public class TutorialEnemies : MonoBehaviour
{
    [SerializeField] private int[] indexScenes; // Por ejemplo: [3, 6]

    public int[] IndexScenes
    {
        get { return indexScenes; }
        set { indexScenes = value; }
    }
}
