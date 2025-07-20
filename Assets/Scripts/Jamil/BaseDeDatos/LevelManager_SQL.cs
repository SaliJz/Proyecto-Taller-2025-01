using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 


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
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this; 

        SceneManager.sceneLoaded += OnSceneLoaded; 

        DontDestroyOnLoad(this.gameObject);

        enemyKillDataGlitch = new EnemyKillData { enemyType = EnemyEnumType.Glitch };
        enemyKillDataShard = new EnemyKillData { enemyType = EnemyEnumType.Shard };
        enemyKillDataTracker = new EnemyKillData { enemyType = EnemyEnumType.Tracker};
        enemyKillDataSpill = new EnemyKillData { enemyType = EnemyEnumType.Spill };
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Get_Top_Weapon_Per_Enemy fetcher = FindObjectOfType<Get_Top_Weapon_Per_Enemy>();
        if (fetcher != null)
        {
            fetcher.Execute();
        }
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.L)) // Solo para prueba
        {
            DebugEnemyKillData();
        }
    }
    public void DebugEnemyKillData()
    {
        Debug.Log("--- Enemy Kill Data Debug ---");

        void PrintData(EnemyKillData data)
        {
            Debug.Log($"Enemy: {data.enemyType}, Gun: {data.countGun}, Rifle: {data.countRifle}, Shotgun: {data.countShotgun}");
        }

        PrintData(enemyKillDataGlitch);
        PrintData(enemyKillDataShard);
        PrintData(enemyKillDataTracker);
        PrintData(enemyKillDataSpill);

        Debug.Log("-----------------------------");
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
                        targetData.countGun = enemyWeaponStat.kill_count;
                        break;
                    case "Rifle":
                        targetData.countRifle = enemyWeaponStat.kill_count;
                        break;
                    case "Shotgun":
                        targetData.countShotgun = enemyWeaponStat.kill_count;
                        break;
                }
            }
        }
    }

    string GetWeaponHighestNumberKills(EnemyKillData enemyKillData) 
    {
        int maxValue = Mathf.Max(enemyKillData.countGun, enemyKillData.countRifle, enemyKillData.countShotgun);

        List<string> candidates = new List<string>();

        if (enemyKillData.countGun == maxValue)
            candidates.Add("Gun");
        if (enemyKillData.countRifle == maxValue)
            candidates.Add("Rifle");
        if (enemyKillData.countShotgun == maxValue)
            candidates.Add("Shotgun");

        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }


    public Color GetColorByEnemyType(EnemyType.CurrentEnemyType tipo)
    {
        string arma = tipo switch
        {
            EnemyType.CurrentEnemyType.Glitch => GetWeaponHighestNumberKills(enemyKillDataGlitch),
            EnemyType.CurrentEnemyType.Shard => GetWeaponHighestNumberKills(enemyKillDataShard),
            EnemyType.CurrentEnemyType.Tracker => GetWeaponHighestNumberKills(enemyKillDataTracker),
            EnemyType.CurrentEnemyType.Spill => GetWeaponHighestNumberKills(enemyKillDataSpill),
            _ => ""
        };

        return arma switch
        {
            "Gun" => Color.green,
            "Rifle" => Color.blue,
            "Shotgun" => Color.red,
            _ => Color.white
        };
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
