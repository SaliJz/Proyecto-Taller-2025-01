using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    private float bulletDamage;
    [SerializeField] private GameObject bulletImpactPrefab; // Prefab del efecto de impacto

    public void Initialize(float damage)
    {
        bulletDamage = damage;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Wall") || other.CompareTag("Ground"))
        {
            CreateBulletEffect(other);
        }

        Destroy(gameObject);
    }

    private void CreateBulletEffect(Collider other)
    {
        Log("Golpe en " + other.gameObject.name);

        // Intenta hacer raycast desde la bala hacia adelante
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 2f))
        {
            GameObject hole = Instantiate(bulletImpactPrefab, hit.point, Quaternion.LookRotation(hit.normal));
            hole.transform.SetParent(hit.collider.transform);
        }
        else
        {
            // Si falla el raycast, instancia en el centro del objeto golpeado
            Vector3 fallbackPoint = other.bounds.center;
            GameObject hole = Instantiate(bulletImpactPrefab, fallbackPoint, Quaternion.identity);
            hole.transform.SetParent(other.transform);
            Log("Raycast no detectó contacto, usando fallback point");
        }
    }

#if UNITY_EDITOR
    private void Log(string message)
    {
        Debug.Log(message);
    }
#endif
}



//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class Bullet : MonoBehaviour
//{
//    private float bulletDamage;

//    public void Initialize(float damage)
//    {
//        bulletDamage = damage;
//    }

//    private void OnCollisionEnter(Collision collision)
//    {

//        //VidaEnemigoGeneral enemigo = collision.gameObject.GetComponent<VidaEnemigoGeneral>();

//        // Verifica si la bala ha colisionado con un objeto
//        //if (collision.gameObject.CompareTag("Enemy"))
//        //{
//        //    // Daño al enemigo
//        //    if (enemigo != null)
//        //    {
//        //        enemigo.RecibirDanio(bulletDamage);
//        //    }

//        //    Debug.Log("Hit enemy!");
//        //    Destroy(gameObject); // Destruye la bala al impactar
//        //}
//        /* else*/
//        if (collision.gameObject.CompareTag("Wall"))
//        {
//            Debug.Log("Hit wall!");
//            Destroy(gameObject); // Destruye la bala al impactar
//        }
//        else if (collision.gameObject.CompareTag("Ground"))
//        {
//            Destroy(gameObject); // Destruye la bala al impactar
//        }
//    }
//}
