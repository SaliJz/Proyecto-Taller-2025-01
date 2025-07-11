using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponType : MonoBehaviour
{
    private GameObject dataBaseManager;
    public enum CurrentWeaponType
    {
        Gun,Rifle,Shotgun
    }
    public CurrentWeaponType currentWeaponType;
    public int weapon_id; //Sera obtenido luego de insertarse

    private void Start()
    {
        dataBaseManager = GameObject.Find("DataBaseManager");
        Insert_Weapon insert_Weapon= dataBaseManager.GetComponent<Insert_Weapon>();
        insert_Weapon.Execute(this);
    }
}
