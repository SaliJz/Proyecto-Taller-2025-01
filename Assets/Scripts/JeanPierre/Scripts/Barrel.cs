using UnityEngine;

public class Barrel : MonoBehaviour
{
    [Header("Daño ALTO")]
    [SerializeField] private int danioAltoAmetralladora = 20;
    [SerializeField] private int danioAltoPistola = 20;
    [SerializeField] private int danioAltoEscopeta = 20;

    [SerializeField] private int health = 100;

    [SerializeField] private GameObject explosionPrefab;

    private bool isDead = false;

    public void RecibirDanioPorBala(BalaPlayer.TipoBala tb)
    {
        int d = tb switch
        {
            BalaPlayer.TipoBala.Ametralladora => danioAltoAmetralladora,
            BalaPlayer.TipoBala.Pistola => danioAltoPistola,
            BalaPlayer.TipoBala.Escopeta => danioAltoEscopeta,
            _ => 0
        };

        TakeDamage(d);
    }

    public void TakeDamage(int damageAmount)
    {
        health -= damageAmount;

        if (health <= 0 && !isDead)
        {
            isDead = true;
            Die();
        }
    }

    private void Die()
    {
        if (explosionPrefab != null)
        {
            Instantiate(explosionPrefab, transform.position, transform.rotation);
        }

        Destroy(gameObject);
    }
}