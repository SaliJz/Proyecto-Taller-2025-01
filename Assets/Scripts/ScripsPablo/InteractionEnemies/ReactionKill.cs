using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReactionKill : MonoBehaviour
{
    private void OnDestroy()
    {
        if (InsertKillsCount.Instance != null)
        {
            InsertKillsCount.Instance.ReportDestruction(gameObject.tag);
        }
    }
}
