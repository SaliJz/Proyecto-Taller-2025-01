using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EnemyType;

public class LevelManager_SQL : MonoBehaviour
{
    public static LevelManager_SQL Instance { get; private set; }

    public enum EnemyEnumType { Glitch, Shard, Tracker, Spill }

    public class EnemyKillData
    {
        public EnemyEnumType enemyType;
        public int countGun;
        public int countRifle;
        public int countShotgun;
    }

    public EnemyKillData enemyKillDataGlitch;
    public EnemyKillData enemyKillDataShard;
    public EnemyKillData enemyKillDataTracker;
    public EnemyKillData enemyKillDataSpill;

    private EnemySpawner enemySpawner;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject); 

        enemyKillDataGlitch = new EnemyKillData { enemyType = EnemyEnumType.Glitch };
        enemyKillDataShard = new EnemyKillData { enemyType = EnemyEnumType.Shard };
        enemyKillDataTracker = new EnemyKillData { enemyType = EnemyEnumType.Tracker };
        enemyKillDataSpill = new EnemyKillData { enemyType = EnemyEnumType.Spill };

        enemySpawner = GameObject.FindAnyObjectByType<EnemySpawner>();
    }

    public void AssignValuesToEnemyKillData(SelectEnemyWeaponStatModel model)
    {
        foreach (EnemyWeaponStatModel enemyWeaponStat in model.dataStats)
        {
            EnemyKillData targetData = null;

            switch (enemyWeaponStat.enemy_type)
            {
                case "Glitch":
                    targetData = enemyKillDataGlitch;
                    break;
                case "Shard":
                    targetData = enemyKillDataShard;
                    break;
                case "Tracker":
                    targetData = enemyKillDataTracker;
                    break;
                case "Spill":
                    targetData = enemyKillDataSpill;
                    break;
            }

            if (targetData != null)
            {
                switch (enemyWeaponStat.weapon_name)
                {
                    case "Gun":
                        targetData.countGun += 1;
                        break;
                    case "Rifle":
                        targetData.countRifle += 1;
                        break;
                    case "Shotgun":
                        targetData.countShotgun += 1;
                        break;
                }
            }
        }

        UpdateInvulnerableEnemiesInCurrentEnemySpawner();
    }

    void UpdateInvulnerableEnemiesInCurrentEnemySpawner()
    {
        string weaponGlitch = GetWeaponHighestNumberKills(enemyKillDataGlitch);
        string weaponShard = GetWeaponHighestNumberKills(enemyKillDataShard);
        string weaponTracker = GetWeaponHighestNumberKills(enemyKillDataTracker);
        string weaponSpill = GetWeaponHighestNumberKills(enemyKillDataSpill);

        Debug.Log($"weaponGlitch es: {weaponGlitch}");
        Debug.Log($"weaponShard es: {weaponShard}");
        Debug.Log($"weaponTracker es: {weaponTracker}");
        Debug.Log($"weaponSpill es: {weaponSpill}");

        Color GetColorEnemy(string weaponHighestEnemy)
        {
            switch (weaponHighestEnemy)
            {
                case "Gun": return  Color.green;
                case "Rifle": return Color.blue;
                case "Shotgun": return Color.red;

                default: return Color.white;
            }
        }
        
        GameObject[] enemiesSpawn = enemySpawner.enemyPrefabs;
        foreach(GameObject enemy in enemiesSpawn)
        {
            VidaEnemigoGeneral vidaEnemigoGeneral=enemy.GetComponent<VidaEnemigoGeneral>();

            if (enemy.GetComponent<EnemyType>().currentEnemyType == EnemyType.CurrentEnemyType.Glitch)
            {
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(GetColorEnemy(weaponGlitch));
                Color c = GetColorEnemy(weaponGlitch);
                Debug.Log($"Color usado para Glitch: {c}");
                vidaEnemigoGeneral.SetTipoDesdeColor(c);
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(c);


            }
            else if (enemy.GetComponent<EnemyType>().currentEnemyType == EnemyType.CurrentEnemyType.Shard)
            {
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(GetColorEnemy(weaponShard));
                Color c = GetColorEnemy(weaponShard);
                vidaEnemigoGeneral.SetTipoDesdeColor(c);
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(c);
            }
            else if (enemy.GetComponent<EnemyType>().currentEnemyType == EnemyType.CurrentEnemyType.Spill)
            {
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(GetColorEnemy(weaponSpill));
                Color c = GetColorEnemy(weaponSpill);
                vidaEnemigoGeneral.SetTipoDesdeColor(c);
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(c);
            }
            else if (enemy.GetComponent<EnemyType>().currentEnemyType == EnemyType.CurrentEnemyType.Tracker)
            {
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(GetColorEnemy(weaponTracker));
                Color c = GetColorEnemy(weaponTracker);
                vidaEnemigoGeneral.SetTipoDesdeColor(c);
                vidaEnemigoGeneral.AsignarColorYEmissionAMateriales(c);
            }

          
        }
      
    }

    string GetWeaponHighestNumberKills(EnemyKillData enemyKillData)
    {
        int valueMax = Mathf.Max(enemyKillData.countGun, enemyKillData.countRifle, enemyKillData.countShotgun);

        if (valueMax == enemyKillData.countGun)
        {
            return "Gun";
        }
        else if (valueMax == enemyKillData.countRifle)
        {
            return "Rifle";
        }
        else if (valueMax == enemyKillData.countShotgun)
        {
            return "Shotgun";
        }
        else
        {
            if (enemyKillData.countGun == enemyKillData.countRifle)
            {
                List<string> list = new List<string> { "Gun", "Rifle" };
                int random = Random.Range(0, list.Count);
                return list[random];
            }
            else if (enemyKillData.countGun == enemyKillData.countShotgun)
            {
                List<string> list = new List<string> { "Gun", "Shotgun" };
                int random = Random.Range(0, list.Count);
                return list[random];
            }
            else if (enemyKillData.countRifle == enemyKillData.countShotgun)
            {
                List<string> list = new List<string> { "Rifle", "Shotgun" };
                int random = Random.Range(0, list.Count);
                return list[random];
            }
            else
            {
                List<string> list = new List<string> { "Gun", "Rifle", "Shotgun" };
                int random = Random.Range(0, list.Count);
                return list[random];
            }
        }
    }
}
