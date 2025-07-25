using System.Collections.Generic;
using UnityEngine;

public class AbilityDataStore : MonoBehaviour
{
    public static AbilityDataStore Instance { get; private set; }

    private List<string> abilityNames = new List<string>();
    private int currentIndex = 0;

    [SerializeField] private bool allowClear = false;

    public List<string> AbilityNames => abilityNames;
    public int CurrentIndex => currentIndex;

    private void Start()
    {
        if (allowClear)
        {
            Clear();
        }
    }

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

    public void SetCurrentIndex(int index)
    {
        currentIndex = index;
    }

    public void Clear()
    {
        abilityNames.Clear();
        currentIndex = 0;
    }
}
