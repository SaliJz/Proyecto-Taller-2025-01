using System.Collections;
using UnityEngine;

public class HaloMoveController : MonoBehaviour
{
    [SerializeField] private Vector3 currentPosition;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float arriveSpeed;
    [SerializeField] private float roundTripSpeed;


    public IEnumerator MoveHaloAparition()
    {
        bool isHaloArrive = false;
        while (!isHaloArrive)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPosition, arriveSpeed * Time.deltaTime);
            yield return null;

            if (transform.position.y >= targetPosition.y)
            {
                isHaloArrive = true;
            }
        }

        StartCoroutine(RoundTripHaloAparition());
    }

    IEnumerator RoundTripHaloAparition()
    {
        bool isHaloRoundTrip = true;
        float minY = transform.position.y;
        float maxY = -2.34f;
        float range = maxY - minY;

        while (isHaloRoundTrip)
        {
            float y = Mathf.PingPong(Time.time * roundTripSpeed, range) + minY;
            transform.position = new Vector3(transform.position.x, y, transform.position.z);
            yield return null;
        }
    }

}
