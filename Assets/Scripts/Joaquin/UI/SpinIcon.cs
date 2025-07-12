using UnityEngine;

public class SpinIcon : MonoBehaviour
{
    [SerializeField] private float speed = 180f;

    void Update()
    {
        transform.Rotate(Vector3.forward * speed * Time.unscaledDeltaTime);
    }
}
