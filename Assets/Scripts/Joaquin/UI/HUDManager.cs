using System.Collections;
using System.Collections.Generic;
using TMPro;
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
    [SerializeField] private TextMeshProUGUI healthBarText;
    [SerializeField] private TextMeshProUGUI ammoText;
    [SerializeField] private TextMeshProUGUI weaponNameText;
    [SerializeField] private Image weaponIcon;

    [SerializeField] private TextMeshProUGUI infoFragmentsText;

    private int infoFragments = 0;

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

        AddInfoFragment(0);

        //infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }

    public void UpdateHealth(int current, int max)
    {
        healthBar.value = (float)current / max;
    }

    public void UpdateAmmo(int current, int total)
    {
        ammoText.text = $"{current} / {total}";
    }

    public void UpdateWeaponName(string name)
    {
        weaponNameText.text = name;
    }

    public void UpdateWeaponIcon(Sprite icon)
    {
        weaponIcon.sprite = icon;
    }
    
    public void AddInfoFragment(int fragments)
    {
        infoFragments += fragments;
        infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }
    
}
