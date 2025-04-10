using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HUDManager : MonoBehaviour
{
    public static HUDManager Instance 
    { 
        get; 
        private set; 
    }

    [SerializeField] private Slider healthBar;
    [SerializeField] private TMPro.TMP_Text healthBarText;

    //[SerializeField] private Text infoFragmentsText;

    //private int infoFragments = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        healthBar.value = 1;
        healthBarText.text = (healthBar.value * 100).ToString();

        //infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }

    public void UpdateHealth(int current, int max)
    {
        healthBar.value = (float)current / max;
    }
    /*
    public void AddInfoFragment()
    {
        infoFragments++;
        infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }
    */
}
