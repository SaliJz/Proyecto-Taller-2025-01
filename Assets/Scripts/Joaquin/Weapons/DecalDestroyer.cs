using UnityEngine;

public class DecalDestroyer : MonoBehaviour
{
    [SerializeField] private float lifeTime = 10f;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
