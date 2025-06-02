using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GlitctTime_Effect : MonoBehaviour
{
    [SerializeField] Light[] luces;
    [SerializeField] float Max_Intensity = 50;
    [SerializeField] float Min_Intensity = 25;
    [SerializeField] float Change_Vel;

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < luces.Length; i++)
        {
            if (luces[i].intensity < Min_Intensity)
            {
                luces[i].intensity = luces[i].intensity + Change_Vel;

            }else if (luces[i].intensity > Max_Intensity)
            {
                luces[i].intensity = luces[i].intensity - Change_Vel;
            }
        }
    }
}
