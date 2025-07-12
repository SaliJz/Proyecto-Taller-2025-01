using UnityEngine;
using UnityEngine.VFX;

public class PlayerPos_Test : MonoBehaviour
{
    [SerializeField] VisualEffect VFX;
    [SerializeField] Transform PlayerPos;

    private void Update()
    {
        if (VFX != null && PlayerPos != null)
        {
            VFX.SetVector3("Player Position", PlayerPos.position);
        }
    }


}
