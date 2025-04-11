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
    [SerializeField] private TMPro.TMP_Text ammoText;
    [SerializeField] private TMPro.TMP_Text weaponNameText;
    [SerializeField] private Image weaponIcon;

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
    /*
    public void AddInfoFragment()
    {
        infoFragments++;
        infoFragmentsText.text = "Info Fragments: " + infoFragments;
    }
    */
}
