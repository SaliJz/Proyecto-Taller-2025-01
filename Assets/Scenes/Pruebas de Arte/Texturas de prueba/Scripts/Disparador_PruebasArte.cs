using UnityEngine;

public class Disparador_PruebasArte : MonoBehaviour
{
    [SerializeField] GameObject Effect;
    [SerializeField] GameObject MuzzleFlash;
    [SerializeField] float vel;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            GameObject proyectil = Instantiate(Effect);
            proyectil.transform.position = transform.position;
            proyectil.transform.rotation = transform.rotation;

            Rigidbody rb = proyectil.GetComponent<Rigidbody>();

            rb.velocity = new Vector3(vel, 0, 0);

            if (MuzzleFlash != null)
            {
                GameObject muzzle = Instantiate(MuzzleFlash);
                muzzle.transform.position = transform.position;
            }
        }
    }
}
