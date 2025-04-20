using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityDataStore : MonoBehaviour
{
    public static AbilityDataStore Instance;

    public List<string> abilityNames = new List<string>();
    public int currentIndex = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Clear()
    {
        abilityNames.Clear();
        currentIndex = 0;
    }
}
