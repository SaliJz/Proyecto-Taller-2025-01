using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyType : MonoBehaviour
{
    public enum CurrentEnemyType
    {
        Glitch,Shard,Tracker,Spill
    }
    public CurrentEnemyType currentEnemyType;
    public int enemy_id; //Sera obtenido luego de insertarse
}
