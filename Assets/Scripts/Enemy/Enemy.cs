using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Enemy : MonoBehaviour
{
    [Header("Stats")]
    private HealthSystem _health;
    [SerializeField] private float _damage;
    [SerializeField] private float _speed;
    private bool _isAlive;

    [Header("Animation")]
    private EnemyAnimationHandler _animationHandler;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;

    public float Health
    {
        get { return _health.currentHealth; }
        set { _health.currentHealth = value; }
    }

    public float Damage 
    {
        get { return _damage; }
    }

    public float Speed
    {
        get { return _speed; }
    }

    public bool IsAlive
    {
        get { return _isAlive; }
    }

    public void TakeDamage(float damage) 
    {
        _health.TakeDamage(damage);
    }
    
    void Awake()
    {
        _animationHandler = this.GetComponent<EnemyAnimationHandler>();
        _health = this.GetComponent<HealthSystem>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    void Update()
    {
        //  Update alive status.
        _isAlive = Health > 0;

        // Check for state on every frame and update animation accordingly.
        _animationHandler.UpdateAnimationTrigger();
        // Enemy is dead, so proceed with die function.
        if (!_isAlive)
        {
            Die();
        }
    }

    void Die()
    {
        //  Stop nav mesh agent now that enemy is dead.
        _navMeshAgent.isStopped = true;
        Destroy(gameObject, 2);
    }
}
