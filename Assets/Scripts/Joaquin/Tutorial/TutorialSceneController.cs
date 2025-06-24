using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialSceneController : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject weaponIcon;
    [SerializeField] private GameObject spawnerManager;
    [SerializeField] private GameObject spawners;
    //[SerializeField] private GameObject abilitySelector;
    [SerializeField] private GameObject AbilityHolder;
    [SerializeField] private GameObject abilityIcon;

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject missionManager;

    [SerializeField] private GameObject GlitchDeathCinematicContainer;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private int normalEnemiesCount = 10;

    [SerializeField] private GameObject infoFragments;

    [SerializeField] private float cinematicDuration = 5f;
    [SerializeField] private bool waitForCinematic = false; // Si se debe esperar por la cinemática de muerte glitch

    private Weapon gunWeapon; // Referencia al componente Weapon del objeto gun

    private void Awake()
    {
        if (gun != null)
        {
            gunWeapon = gun.GetComponent<Weapon>();
        }
    }

    private void Start()
    {
        if (weaponManager != null) weaponManager.enabled = false;

        if (AbilityHolder != null) AbilityHolder.SetActive(false);
        if (abilityIcon != null) abilityIcon.SetActive(false);

        if (missionManager != null) missionManager.SetActive(false);

        if (gun != null) gun.SetActive(false);
        if (weaponIcon != null) weaponIcon.SetActive(false);

        if (spawnerManager != null) spawnerManager.SetActive(false);
        //if (spawners != null) spawners.SetActive(false);

        if (GlitchDeathCinematicContainer != null) GlitchDeathCinematicContainer.SetActive(false);

        if (infoFragments != null) infoFragments.SetActive(false);
        //if (abilitySelector != null) abilitySelector.SetActive(false);
    }

    public void ActiveGun()
    {
        if (gun != null) gun.SetActive(true);
        if (weaponIcon != null) weaponIcon.SetActive(true);

        if (gunWeapon != null)
        {
            HUDManager.Instance.UpdateAmmo(gunWeapon.CurrentAmmo, gunWeapon.TotalAmmo);
            HUDManager.Instance.UpdateWeaponIcon(gunWeapon.Stats.weaponIcon);
            HUDManager.Instance.UpdateWeaponName(gunWeapon.Stats.weaponName);
        }
        else
        {
            Debug.LogWarning("[tutorialSceneController] El componente Weapon no está asignado al objeto gun.");
        }
    }

    public void ActiveRifleAndShotgun()
    {
        if (weaponManager != null)
        {
            weaponManager.enabled = true;
        }

        if (AbilityHolder != null) AbilityHolder.SetActive(true);
        if (abilityIcon != null) abilityIcon.SetActive(true);
    }

    public void SpawnNormalEnemies()
    {
        int count = Mathf.Min(normalEnemiesCount, normalEnemies.Length);
        for (int i = 0; i < count; i++)
        {
            if (normalEnemies[i] != null) normalEnemies[i].SetActive(true);
        }
    }

    public void PlayGlitchDeathCinematic()
    {
        Time.timeScale = 0;
        GlitchDeathCinematicContainer.SetActive(true);
    }

    public void StopGlitchDeathCinematic()
    {           
        GlitchDeathCinematicContainer.SetActive(false);
        Time.timeScale = 1f;
         // Inicia el siguiente escenario (escena 5)        
    }

    public void ActivateInfoFragments()
    {
        if (infoFragments != null)
        {
            infoFragments.SetActive(true);
        }
    }

    public void ActivateFirstMission()
    {
        Debug.Log("[tutorialSceneController] Activando primera misión.");

        spawners.SetActive(true);
        spawnerManager.SetActive(true);
        missionManager.SetActive(true);
    }

    public void ActivateAbilitySelector()
    {
        //if (abilitySelector != null) abilitySelector.SetActive(true);
    }
}