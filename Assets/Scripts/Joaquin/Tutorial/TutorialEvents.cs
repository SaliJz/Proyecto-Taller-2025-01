using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEvents : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject weaponIcon;
    [SerializeField] private GameObject spawnerManager;
    [SerializeField] private GameObject spawners;
    [SerializeField] private GameObject abilitySelector;
    [SerializeField] private GameObject AbilityHolder;
    [SerializeField] private GameObject abilityIcon;

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject missionManager;

    [SerializeField] private GameObject glitchDeathCinematic;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private int normalEnemiesCount = 10;

    [SerializeField] private GameObject infoFragments;

    [SerializeField] private float timeToWait = 5f;
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
        if (spawners != null) spawners.SetActive(false);

        if (glitchDeathCinematic != null) glitchDeathCinematic.SetActive(false);

        if (infoFragments != null) infoFragments.SetActive(false);
        if (abilitySelector != null) abilitySelector.SetActive(false);
    }

    private void Update()
    {
        if (!waitForCinematic) return;

        timeToWait -= Time.fixedUnscaledDeltaTime;

        if (timeToWait <= 0)
        {
            waitForCinematic = false;

            if (glitchDeathCinematic != null && glitchDeathCinematic.activeSelf)
            {
                Debug.Log("[TutorialEvents] Terminó la espera, desactivando cinematic y avanzando el tutorial.");
                glitchDeathCinematic.SetActive(false);
                Time.timeScale = 1f;
                // Inicia el siguiente escenario (escena 5)
                TutorialManager.Instance.StartScenarioByManual(5);
            }
            else
            {
                Debug.LogWarning("[TutorialEvents] No se encontró glitchCinematic activo.");
            }
        }
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
            Debug.LogWarning("[TutorialEvents] El componente Weapon no está asignado al objeto gun.");
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
        timeToWait = 6.5f;
        waitForCinematic = true;
        Time.timeScale = 0;

        if (glitchDeathCinematic != null)
        {
            glitchDeathCinematic.SetActive(true);
        }
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
        Debug.Log("[TutorialEvents] Activando primera misión.");

        if (spawners != null) spawners.SetActive(true);
        if (spawnerManager != null) spawnerManager.SetActive(true);
        if (missionManager != null) missionManager.SetActive(true);
    }

    public void ActivateAbilitySelector()
    {
        if (abilitySelector != null) abilitySelector.SetActive(true);
    }
}