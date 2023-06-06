using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;
using EnemyMachine;


[RequireComponent(typeof(Shared.HealthSystem))]
public class MeleeDroid : MonoBehaviour, IEnemyMachine
{
    public EnemyState currentState = EnemyState.Idle;
    public Vector3 destination;

    [Header("General Settings")]
    public LayerMask targetMask;
    public float stopThreshold = 0.5f;
    private GameObject _targetObject;
    private HealthSystem _healthSystem;

    [Header("Idle Properties")]
    public float detectionRadius = 5.0f;

    [Header("Patrol Properties")]
    public float patrolRadius = 2.5f; // if within radius switch from idle to attack
    public float patrolHoldTime = 1.0f;
    private Vector3 _patrolCenter = Vector3.zero;
    private float _patrolIdleTime = 0.0f;
    private Vector3 _patrolTargetPoint;
    private EnemyState _previousState;
    private NavMeshAgent _agent;

    [Header("Attack Properties")]
    public float attackForce = 2.0f;
    public float AttackDamage = 5.0f;
    public float attackRange = 1.0f;

    [Header("Animation")]
    private Animator _animator;

    [Header("Sound")]
    private AudioSource _attackSound;

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _attackSound = GetComponent<AudioSource>();
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => {
            if(args.newHealth <= 0)
            {
                _agent.isStopped = true;
                _animator.SetTrigger("Die");
                Destroy(gameObject, 2);
            }
            //taking damage hit feed back here
            _targetObject = args.attacker;
            if(currentState == EnemyState.Idle)
            {
                currentState = EnemyState.Chasing;
            }
        });
        destination = transform.position;
        _patrolCenter = transform.position;
        _patrolTargetPoint = transform.position;
    }

    void Update()
    {
        currentState = currentState switch
        {
            EnemyState.Idle => IdleStateHandler(),
            EnemyState.Chasing => ChasingStateHandler(),
            EnemyState.Attacking => AttackingStateHandler(),
            _ => IdleStateHandler()
        };
        _previousState = currentState;
    }


    public EnemyState IdleStateHandler()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, patrolRadius, targetMask);
        if (hitColliders.Length > 0)
        {
            _targetObject = hitColliders[0].gameObject;
            return EnemyState.Chasing;
        }
        _animator.SetTrigger("Patrol");
        Patrol();
        return EnemyState.Idle;
    }

    public EnemyState ChasingStateHandler()
    {
        // If player is dead, return to being idle.
        if(CheckPlayerDead())
        {
            return EnemyState.Idle;
        }
        // Play a chase animation since we are now in a chase state.
        _animator.SetTrigger("Chase");

        // Navigate towards the player.
        destination = _targetObject.transform.position;
        _agent.SetDestination(destination);

        // If we are within range to attack, enter attack state.
        if (_agent.remainingDistance <= attackRange)
        {
            return EnemyState.Attacking;
        }

        // Stay in chase state if we're not going to attack or idle.
        return EnemyState.Chasing;
    }

    public EnemyState AttackingStateHandler()
    {
        // If player is dead, return to being idle.
        if(CheckPlayerDead())
        {
            return EnemyState.Idle;
        }
        // Check if we leave attack range.
        if (Vector3.Distance(gameObject.transform.position, _targetObject.transform.position) >= attackRange) 
        {
            _animator.ResetTrigger("Attack");
            return EnemyState.Chasing;
        }
        // If we are in attack range and we are not attacking, attack and stay in attack state.
        _animator.SetTrigger("Attack");
        return EnemyState.Attacking;
    }

    private void Patrol()
    {
        // Start a timer at position
        // if stopped at destination (within the range) start timer
        if (_agent.remainingDistance <= _agent.stoppingDistance + stopThreshold)
        {
            _patrolIdleTime += Time.deltaTime;
        }

        // Update when timer is finished (get new destination)
        if (_patrolIdleTime >= patrolHoldTime)
        {
            _animator.SetTrigger("Idle");
            _patrolTargetPoint = _patrolCenter + UnityEngine.Random.insideUnitSphere * patrolRadius;
            _patrolIdleTime = 0.0f;
        }
        destination = _patrolTargetPoint;
        _agent.SetDestination(destination);
    }

    public void Attack()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, targetMask);
        foreach (var hitCollider in hitColliders)
        {
            GameObject player = hitCollider.gameObject;
            CharacterMotor playerMotor = player.GetComponent<CharacterMotor>();
            Shared.HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
            if (player != null)
            {
                _attackSound.Play();
                playerHealthSystem.TakeDamage(gameObject, AttackDamage);
                playerMotor.AddForce(transform.forward * attackForce);
                return;
            }
        }
    }

    private bool CheckPlayerDead()
    {
        if (_targetObject == null)
        {
            return true;
        }
        return false;
    }
}
