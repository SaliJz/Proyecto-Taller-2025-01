using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IEquippableAbility
{
    string GetAbilityName();
    void ApplyUpgrades();
}