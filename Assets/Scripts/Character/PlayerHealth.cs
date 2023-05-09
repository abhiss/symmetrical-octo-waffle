using UnityEngine;

public class PlayerHealthSystem : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100.0f;
    public float currentHealth;
    public float damagePerSecond = 10f;

    private float _timeSinceLastDamage;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        _timeSinceLastDamage = 0f;
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Implement any logic needed when the object dies, e.g., destroying the object
        Destroy(gameObject);
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("Collision detected!");
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("detected!!!!!!!!!!!!!!!!!!!!!!!!!!");
            _timeSinceLastDamage += Time.deltaTime;
            if (_timeSinceLastDamage >= 1f)
            {
                TakeDamage(damagePerSecond);
                _timeSinceLastDamage = 0f;
            }
        }
    }
}