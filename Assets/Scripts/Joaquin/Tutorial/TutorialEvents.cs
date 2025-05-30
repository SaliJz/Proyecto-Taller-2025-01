using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEvents : MonoBehaviour
{
    [SerializeField] private GameObject gun;
    [SerializeField] private GameObject rifle;
    [SerializeField] private GameObject shotgun;
    [SerializeField] private GameObject MissionManager;
    [SerializeField] private GameObject SpawnerManager;
    [SerializeField] private WeaponManager weaponManager;
    [SerializeField] private GameObject[] normalEnemies;
    [SerializeField] private int normalEnemiesCount = 10;
    [SerializeField] private GameObject[] specialEnemies;
    [SerializeField] private int specialEnemiesCount = 5;
    [SerializeField] private Transform spawnPos;

    public void ActiveGun()
    {
        if (weaponManager != null) weaponManager.enabled = false;
        gun.SetActive(true);
    }

    public void ActiveRifleAndShotgun()
    {
        if (weaponManager != null)
        {
            weaponManager.enabled = true;
        }

        rifle.SetActive(true);
        shotgun.SetActive(true);
    }

    public void SpawnNormalEnemies()
    {
        for (int i = 0; i < normalEnemiesCount; i++)
        {
            Vector3 vector3 = spawnPos.position + Vector3.right * i * 2f; // Espaciado de 2 unidades entre enemigos
            Instantiate(normalEnemies[Random.Range(0, normalEnemies.Length)], vector3, Quaternion.identity);
        }
    }

    public void SpawnSpecialEnemies()
    {
        for (int i = 0; i < specialEnemiesCount; i++)
        {
            Vector3 vector3 = spawnPos.position + Vector3.right * i * 3f; // Espaciado de 3 unidades entre enemigos especiales
            Instantiate(specialEnemies[Random.Range(0, specialEnemies.Length)], vector3, Quaternion.identity);
        }
    }

    public void ActivateFirstMission()
    {
        if (MissionManager != null) MissionManager.SetActive(true);
        if (SpawnerManager != null) SpawnerManager.SetActive(true);
    }
}
