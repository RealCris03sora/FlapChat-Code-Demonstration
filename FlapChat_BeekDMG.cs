using UnityEngine;

public class BeekDamage : MonoBehaviour
{
    public PlayerController playerController;
    public EnemyProjectile projectile;
    float speedDamage;

    private void Update()
    {
        speedDamage = playerController.currentPlayerSpeed / 4.0f;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        DamageHandler enemyHealth = other.GetComponent<DamageHandler>();
        if (enemyHealth != null)
        {
            // bounce calculus
            Vector2 bounceVelocity = new Vector2(speedDamage * 0.3f, speedDamage * 1.6f);

            Rigidbody2D enemyRigidbody = other.GetComponent<Rigidbody2D>();
            if (enemyRigidbody != null)
            {
                enemyRigidbody.velocity = bounceVelocity;
            }

            // damage based on speed
            enemyHealth.TakeDamage(speedDamage);
        }

        // destroy projectile if beak collides with them
        if (other.CompareTag("EnemyProjectile"))
        {
            Destroy(other.gameObject);
        }
    }
}
