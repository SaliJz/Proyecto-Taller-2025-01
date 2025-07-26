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
        if (UpAnim != null) UpAnim.gameObject.SetActive(false);
        if (DownAnim != null) DownAnim.gameObject.SetActive(false);
        if (RightAnim != null) RightAnim.gameObject.SetActive(false);
        if (LeftAnim != null) LeftAnim.gameObject.SetActive(false);

        if (PlayerTransform == null)
        {
            // 1. Encuentra el GameObject primero.
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

            // 2. Comprueba si se encontró antes de obtener el transform.
            if (playerObj != null)
            {
                PlayerTransform = playerObj.transform;
            }
            else
            {
                // 3. Muestra un error si no se encuentra para facilitar la depuración.
                Debug.LogError("Active_ShieldHUD: No se pudo encontrar un objeto con la etiqueta 'Player'.");
            }
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
        if (anim == null) return;

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

        yield return new WaitForSeconds(0.5f);

        obj.SetActive(false);
    }
}
