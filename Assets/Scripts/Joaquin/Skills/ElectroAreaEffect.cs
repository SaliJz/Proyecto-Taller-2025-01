using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElectroAreaEffect : BaseAreaEffect
{
    protected override void ApplyEffectToTargets()
    {
        ApplyDamageToEnemies();
        ApplyDamageToPlayers();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(0.2f, 0.8f, 1f, 0.3f);
    }
}