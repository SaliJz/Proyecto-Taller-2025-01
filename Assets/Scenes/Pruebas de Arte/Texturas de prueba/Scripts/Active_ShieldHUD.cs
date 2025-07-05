using Unity.VisualScripting;
using UnityEngine;

public class Active_ShieldHUD : MonoBehaviour
{
    [SerializeField] Animator UpAnim;
    [SerializeField] Animator DownAnim;
    [SerializeField] Animator RightAnim;
    [SerializeField] Animator LeftAnim;
    public Transform PlayerTransform;

    public void ActiveShield(Vector3 attackDir)
    {
        if(PlayerTransform != null)
        {
            attackDir.Normalize();

            Vector3 forward = PlayerTransform.forward;
            Vector3 right = PlayerTransform.right;

            float frontDot = Vector3.Dot(forward, attackDir);
            float rightDot = Vector3.Dot(right, attackDir);

            if (frontDot > 0.7f)
            {
                Debug.Log("ARRIBA");
                UpAnim.Play("ShieldFade");
            }
            else if(frontDot < -0.7f)
            {
                Debug.Log("ATRAS");
                DownAnim.Play("ShieldFade");
            }
            else if (rightDot > 0f)
            {
                Debug.Log("DERECHA");
                RightAnim.Play("ShieldFade");
            }
            else
            {
                Debug.Log("IZQUIERDA");
                LeftAnim.Play("ShieldFade");
            }
        }

    }
}
