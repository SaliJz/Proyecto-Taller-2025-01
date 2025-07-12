using UnityEngine;

public class MovimientoJugador : MonoBehaviour
{
    public float velocidadMovimiento = 5f;
    private Rigidbody rb;
    private Vector3 movimiento;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {

        float movimientoX = Input.GetAxisRaw("Horizontal");
        float movimientoZ = Input.GetAxisRaw("Vertical");


        movimiento = new Vector3(movimientoX, 0f, movimientoZ).normalized;
    }

    void FixedUpdate()
    {

        rb.velocity = movimiento * velocidadMovimiento + new Vector3(0, rb.velocity.y, 0);
    }

}
