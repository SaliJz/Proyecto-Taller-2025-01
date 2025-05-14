using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float bulletSpeed = 10f;
    public Camera mainCamera;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Vector3 targetPoint = hit.point;

               
                Vector3 direction = targetPoint - firePoint.position;

                
                direction.y = 0;

              
                Vector3 directionNormalized = direction.normalized;

             
                firePoint.rotation = Quaternion.LookRotation(directionNormalized);

               
                GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                   
                    rb.velocity = directionNormalized * bulletSpeed;
                }
            }
        }
    }
}



