using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IgnitionAreaEffect : BaseAreaEffect
{
    protected override void ApplyEffectToTargets()
    {
        ApplyDamageToEnemies();
        ApplyDamageToPlayers();
    }

    protected override Color GetGizmoColor()
    {
        return new Color(1f, 0.4f, 0f, 0.3f);
    }
}