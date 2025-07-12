using UnityEngine;

public class TargetIndicator : MonoBehaviour
{
    [SerializeField] private GameObject prefabWorldIndicator;
    private GameObject instanceWorldIndicator;

    public void ActivateIndicator()
    {
        if (instanceWorldIndicator == null)
        {
            instanceWorldIndicator = Instantiate(prefabWorldIndicator, transform.position, transform.rotation, transform);
        }

        indicatorManagerUI.Instance.AddTarget(this);
    }

    public void DisableIndicator()
    {
        if (instanceWorldIndicator != null)
        {
            Destroy(instanceWorldIndicator);
        }

        indicatorManagerUI.Instance.RemoveTarget(this);
    }
}
