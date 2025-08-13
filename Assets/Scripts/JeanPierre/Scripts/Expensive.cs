using UnityEngine;

public class Expensive : MonoBehaviour
{
    [SerializeField] private float expansionTime = 2f;
    [SerializeField] private Vector3 targetScale = new Vector3(3f, 3f, 3f);
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private Color gizmoColor = Color.red;

    private Vector3 initialScale;
    private float timer = 0f;
    private bool isExpanding = true;

    void Start()
    {
        initialScale = transform.localScale;
    }

    void Update()
    {
        if (isExpanding)
        {
            timer += Time.deltaTime;
            float progress = timer / expansionTime;

            transform.localScale = Vector3.Lerp(initialScale, targetScale, progress);

            if (progress >= 1f)
            {
                isExpanding = false;
            }
        }
    }

    void OnDrawGizmos()
    {
        if (showGizmos)
        {
            Gizmos.color = gizmoColor;

            Gizmos.DrawSphere(transform.position, transform.localScale.x * 0.5f);

            Gizmos.color = new Color(gizmoColor.r, gizmoColor.g, gizmoColor.b, 0.3f);
            Gizmos.DrawSphere(transform.position, targetScale.x * 0.5f);
        }
    }
}