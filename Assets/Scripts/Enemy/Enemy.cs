using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Enemy : MonoBehaviour
{
    [Header("Stats")]
    private HealthSystem _health;
    [SerializeField] private float _damage;
    //  This member will later influence the speed at which the enemy moves using the NavMeshAgent.
    [SerializeField] private float _speed;
    private bool _isAlive;

    [Header("Movement")]
    private StateMachine _stateHandler;

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
    
    /*
    Placeholder function for dealing damage to a GameObject with a player script that contains a TakeDamage function.

    private void DealDamage(Player player)
    {
        player.TakeDamage(_damage);
    }
    */

    // Start is called before the first frame update
    void Awake()
    {
        _animationHandler = this.GetComponent<EnemyAnimationHandler>();
        _stateHandler = this.GetComponent<StateMachine>();
        _health = this.GetComponent<HealthSystem>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        _isAlive = Health > 0;

        _animationHandler.UpdateAnimationTrigger();
        // Enemy is dead, so only proceed with die function
        if (!_isAlive)
        {
            Die();
        }
    }

    void Die()
    {
        _navMeshAgent.isStopped = true;
        Destroy(gameObject, 2);
    }
}
