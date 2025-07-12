using System.Collections;
using UnityEngine;

public class Loot_Absortion : MonoBehaviour
{
    [Header("Launch Shoot")]
    [SerializeField] Vector3 Origin;
    [SerializeField] float force;

    [Header("Drag to Player")]
    [SerializeField] float delayToBeAttract;
    [SerializeField] float attractSpeed;
    [SerializeField] Transform PlayerTransform;

    bool attracting;
    Rigidbody rb;


    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(StartAttraction());

        LaunchShoot(Origin, force);
    }
    private void Update()
    {
        if (attracting && PlayerTransform != null)
        {
            transform.position = Vector3.Lerp(transform.position, PlayerTransform.position, attractSpeed);
        }
    }

    void LaunchShoot(Vector3 explosionOrigin, float force)
    {
        Vector3 randomDir = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(0.5f, 1f),
            Random.Range(-1f, 1f)
            ).normalized;

        GetComponent<Rigidbody>().AddForce(randomDir * force, ForceMode.Impulse);

    }

    IEnumerator StartAttraction()
    {
        yield return new WaitForSeconds(delayToBeAttract);
        attracting = true;
        rb.isKinematic = true;
    }

    private void LateUpdate()
    {
        transform.forward = Camera.main.transform.forward;
    }

}
