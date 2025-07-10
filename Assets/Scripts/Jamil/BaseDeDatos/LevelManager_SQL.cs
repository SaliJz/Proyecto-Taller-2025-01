using System.Collections.Generic;
using UnityEngine;

public class LevelManager_SQL : MonoBehaviour
{
    public static LevelManager_SQL Instance { get; private set; }
    [SerializeField] string currentLevel;
    public enum EnemyType { Glitch, Shard, Tracker, Spill }
    public class EnemyKillDataGeneral
    {  
        public EnemyType enemyType;
        public int countGun;
        public int countRifle;
        public int countShotgun;
    }
    public EnemyKillDataGeneral enemyKillDataGlitch;
    public EnemyKillDataGeneral enemyKillDataShard;
    public EnemyKillDataGeneral enemyKillDataTracker;
    public EnemyKillDataGeneral enemyKillDataSpill;
    public class EnemyKillDataForLevel
    {
        Level_SQL level_SQL;
        public EnemyType enemyType;
        public int countGun;
        public int countRifle;
        public int countShotgun;
    }

    private EnemySpawner enemySpawner;
    public class Level_SQL //Para asignar los enemigos a un level SQL
    {
        public string level_name;
        List<int> enemy_weapon_id=new List<int>();
        //EnemyKillDataGeneral enemyKillData;
    }
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        Level_SQL level1 = new Level_SQL();
        Level_SQL level2 = new Level_SQL();
        Level_SQL level3 = new Level_SQL();
        Level_SQL level4 = new Level_SQL();
        Level_SQL level5 = new Level_SQL();


        enemyKillDataGlitch = new EnemyKillDataGeneral { enemyType = EnemyType.Glitch };
        enemyKillDataShard = new EnemyKillDataGeneral { enemyType = EnemyType.Shard };
        enemyKillDataTracker = new EnemyKillDataGeneral { enemyType = EnemyType.Tracker };
        enemyKillDataSpill = new EnemyKillDataGeneral { enemyType =EnemyType.Spill };

        enemySpawner = GameObject.FindAnyObjectByType<EnemySpawner>();
    }


    public void AssignValuesToEnemyKillData(SelectEnemyWeaponStatModel model)
    {
        foreach (EnemyWeaponStatModel enemyWeaponStat in model.dataStats)
        {
            EnemyKillDataGeneral targetData = null;

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
    }

    void UpdateInvulnerableEnemiesInCurrentEnemySpawner()
    {

    }


}
