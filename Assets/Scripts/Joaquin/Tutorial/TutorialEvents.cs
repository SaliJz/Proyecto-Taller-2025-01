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

    [SerializeField] private WeaponManager weaponManager;

    [SerializeField] private GameObject missionManager;

    [SerializeField] private GameObject glitchDeathCinematic;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private int normalEnemiesCount = 10;

    [SerializeField] private GameObject infoFragments;

    [SerializeField] private float timeToWait = 5f;
    [SerializeField] private bool waitForCinematic = false; // Si se debe esperar por la cinemática de muerte glitch

    private void Start()
    {
        if (weaponManager != null) weaponManager.enabled = false;
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

        timeToWait -= Time.deltaTime;

        if (timeToWait <= 0)
        {
            Time.timeScale = 0;
            waitForCinematic = false;

            if (glitchDeathCinematic != null && glitchDeathCinematic.activeSelf)
            {
                Time.timeScale = 1;
                Debug.Log("[TutorialEvents] Terminó la espera, desactivando cinematic y avanzando el tutorial.");
                glitchDeathCinematic.SetActive(false);

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
        if (weaponManager != null) weaponManager.enabled = false;
        if (weaponIcon != null) weaponIcon.SetActive(true);
        if (gun != null) gun.SetActive(true);
    }

    public void ActiveRifleAndShotgun()
    {
        if (weaponManager != null)
        {
            weaponManager.enabled = true;
        }
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
        timeToWait = 5f;
        waitForCinematic = true;

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