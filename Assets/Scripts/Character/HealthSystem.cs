using UnityEngine;

public class HealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100.0f;
    public float currentHealth;

    [Header("Damage Settings")]
    public LayerMask enemyLayers;
    public float damageOnCollision = 10.0f;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the collided object is in the enemyLayers
        if (((1 << other.gameObject.layer) & enemyLayers) != 0)
        {
            TakeDamage(damageOnCollision);
        }
    }

    private void Die()
    {
        // Implement any logic needed when the object dies, e.g., destroying the object
        if (gameObject.CompareTag("TopDownCharacter"))
        {
            // Handle player death logic, e.g., showing Game Over screen, restarting the level, etc.
            Debug.Log("Player died!");
        }
        else
        {
            // Handle enemy death logic
            Destroy(gameObject);
        }
    }
}