using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shared;

class Enemy : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private float _damage;
    [SerializeField] private float _speed;
    [SerializeField] private float _attackSpeed;
    private HealthSystem _health;

    private bool _isAlive;
    private const float DeathTime = 5;
    [SerializeField] private GameObject _target;

    private EnemyAnimationHandler _animationHandler;
    private StateMachine _stateMachine;
    private State _currentState;
    private Dictionary<Type, Action> _stateActions;

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

    void Awake()
    {
        _animationHandler = GetComponent<EnemyAnimationHandler>();
        _health = GetComponent<HealthSystem>();
        _stateMachine = GetComponent<StateMachine>();
        _currentState = _stateMachine.GetCurrentState();
        _target = GameObject.Find("TopDownCharacter");
        _stateActions = new();

        // Map states to their correct functions.
        _stateActions[typeof(AttackState)] = Attack;
        _stateActions[typeof(ChaseState)] = Chase;
        _stateActions[typeof(IdleState)] = Idle;
    }

    void Update()
    {
        // Update alive status.
        _isAlive = Health > 0;
        _target = GameObject.Find("TopDownCharacter");

        // Take an action based on current state.
        StartCoroutine(TakeAction());
    }

    IEnumerator TakeAction()
    {
        // Update the current enemy state.
        _currentState = _stateMachine.GetCurrentState();
        // If the enemy is dead, begin a Death action and wait for it to finish.
        if (!_isAlive)
        {
            Die();
            yield return new WaitForSeconds(DeathTime);
        }
        //  If the enemy is alive, invoke a function based on the current enemy state.
        _stateActions[_currentState.GetType()].Invoke();
    }

    void Die()
    {
        _animationHandler.AnimateDeath();
        Destroy(gameObject, DeathTime);
    }

    void Attack()
    {
        _animationHandler.AnimateAttack();
    }

    void Chase()
    {
        _animationHandler.AnimateChase();
    }

    void Idle()
    {
        _animationHandler.AnimateIdle();
    }

    bool IsAttacking()
    {
        return _currentState.GetType() == typeof(AttackState);
    }
}
