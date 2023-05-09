using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _health;
    [SerializeField] private float _damage;
    //  This member will later influence the speed at which the enemy moves using the NavMeshAgent.
    [SerializeField] private float _speed;
    private bool _isAlive;

    [Header("Movement")]
    private StateMachine _stateHandler;

    /*
    [Header("Animation")]
    Uncomment after integrating animation for enemy.
    private EnemyAnimationHandler _animationHandler;
    */

    public float Health
    {
        get { return _health; }
        set { _health = value; }
    }

    public float Damage 
    {
        get { return _damage; }
        set { _damage = value; }
    }

    public bool IsAlive
    {
        get { return _isAlive; }
    }

    public void TakeDamage(float damage) 
    {
        _health -= damage;
    }
    
    /*
    Placeholder function for dealing damage to a GameObject with a player script that contains a TakeDamage function.

    private void DealDamage(Player player)
    {
        player.TakeDamage(_damage);
    }
    */

    // Start is called before the first frame update
    void Start()
    {
        /*
        Uncomment after integrating animation for enemy.
        _animationHandler = this.GetComponent<EnemyAnimationHandler>();
        */
        _stateHandler = this.GetComponent<StateMachine>();
    }

    // Update is called once per frame
    void Update()
    {
        _isAlive = _health > 0;

        /*
        Uncomment after integrating animation for enemy.
        _animationHandler.UpdateAnimationTrigger();
        */
        // Enemy is dead, so only proceed with die function
        if (!_isAlive)
        {
            Die();
        }
    }

    void Die()
    {
        Destroy(gameObject, 2);
    }
}
