using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IHackable
{
    void ApplyHack(float duration, Vector3 impactPoint, Vector3 impactNormal);
}
