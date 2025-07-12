using System.Collections;
using UnityEngine;

public class Active_ShieldHUD : MonoBehaviour
{
    [SerializeField] Animator UpAnim;
    [SerializeField] Animator DownAnim;
    [SerializeField] Animator RightAnim;
    [SerializeField] Animator LeftAnim;
    public Transform PlayerTransform;

    private void Start()
    {
        if (UpAnim != false) UpAnim.gameObject.SetActive(false);
        if (DownAnim != false) DownAnim.gameObject.SetActive(false);
        if (RightAnim != false) RightAnim.gameObject.SetActive(false);
        if (LeftAnim != false) LeftAnim.gameObject.SetActive(false);

        if (PlayerTransform == null)
        {
            PlayerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        }

        ResetAllAlphas();
    }

    private void ResetAllAlphas()
    {
        SetAlpha(UpAnim, 0f);
        SetAlpha(DownAnim, 0f);
        SetAlpha(RightAnim, 0f);
        SetAlpha(LeftAnim, 0f);
    }

    private void SetAlpha(Animator anim, float alpha)
    {
        var image = anim.GetComponent<UnityEngine.UI.Image>();
        if (image != null)
        {
            var color = image.color;
            color.a = alpha;
            image.color = color;
        }
    }

    public void ActivateIndicator(Vector3 attackDir)
    {
        if (PlayerTransform != null)
        {
            attackDir.Normalize();

            Vector3 forward = PlayerTransform.forward;
            Vector3 right = PlayerTransform.right;

            float forwardDot = Vector3.Dot(forward, attackDir);
            float rightDot = Vector3.Dot(right, attackDir);

            const float threshold = 0.4f;

            if (forwardDot > threshold)
            {
                Debug.Log("ARRIBA");
                ShowIndicator(UpAnim);
            }

            if (forwardDot < -threshold)
            {
                Debug.Log("ATRAS");
                ShowIndicator(DownAnim);
            }

            if (rightDot > threshold)
            {
                Debug.Log("DERECHA");
                ShowIndicator(RightAnim);
            }

            if (rightDot < -threshold)
            {
                Debug.Log("IZQUIERDA");
                ShowIndicator(LeftAnim);
            }
        }

    }

    private void ShowIndicator(Animator animGO)
    {
        StartCoroutine(PlayAndDisable(animGO));
    }

    private IEnumerator PlayAndDisable(Animator animGO)
    {
        GameObject obj = animGO.gameObject;

        if (!obj.activeSelf)
            obj.SetActive(true);

        animGO.Play("ShieldFade", 0, 0f);

        yield return new WaitForSeconds(0.2f);

        obj.SetActive(false);
    }
}
