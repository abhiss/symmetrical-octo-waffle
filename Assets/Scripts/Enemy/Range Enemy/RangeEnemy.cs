using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;
using EnemyMachine;

[RequireComponent(typeof(Shared.HealthSystem))]
public class RangeEnemy : MonoBehaviour, IEnemyMachine
{
    public EnemyState currentState = EnemyState.Idle;
    public Vector3 destination;

    [Header("General Settings")]
    public LayerMask targetMask;
    private float _stopThreshold = 0.5f;
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
    public float attackCD = 1.0f;
    public float attackRange = 1.0f;
    public float panicRange = 2.0f;
    private float _attackTimer;
    //private bool _isAttacking = false;

    [Header("Debugging")]
    public bool showPatrolInfo;
    public bool showAttackInfo;
    public bool showChaseInfo;

    void Start()
    {
        _stopThreshold = attackRange;
        _attackTimer = attackCD;
        _agent = GetComponent<NavMeshAgent>();
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => {
            if(args.newHealth <= 0)
            {
                //dead, death animation/code here. 
                Debug.Log("Enemy dead.");
                Destroy(gameObject);
            }
            //taking damage
            Debug.Log($"Enemy taking damage. Remaining health: {args.newHealth}");
        });
        destination = transform.position;

        // Init Idle State
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
        // Detect player
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, patrolRadius, targetMask);
        foreach (var hitCollider in hitColliders)
        {
            _targetObject = hitCollider.gameObject;
            return EnemyState.Chasing;
        }

        Patrol();
        return EnemyState.Idle;
    }

    private void Patrol()
    {
        // Start a timer at position
        // if stopped at destination (within the range) start timer
        if (_agent.remainingDistance <= _agent.stoppingDistance + _stopThreshold)
        {
            _patrolIdleTime += Time.deltaTime;
        }

        // Update when timer is finished (get new destination)
        if (_patrolIdleTime >= patrolHoldTime)
        {
            _patrolTargetPoint = _patrolCenter + UnityEngine.Random.insideUnitSphere * patrolRadius;
            _patrolIdleTime = 0.0f;
        }
        destination = _patrolTargetPoint;
        _agent.SetDestination(destination);
    }

    //reposition
    public EnemyState ChasingStateHandler()
    {
        if(CheckPlayerDead())
        {
            return EnemyState.Idle;
        }

        destination = _targetObject.transform.position;
        _agent.SetDestination(destination);
        // 1.0f = Player Radius + Enemy Radius
        // currentDistance = attack range
        if (_agent.remainingDistance <= attackRange)
        {
            Rotate();
            return EnemyState.Attacking;
        }

        return EnemyState.Chasing;
    }

    public EnemyState AttackingStateHandler()
    {
        if(CheckPlayerDead())
        {
            return EnemyState.Idle;
        }

        // repositioning
        if (Vector3.Distance(gameObject.transform.position, _targetObject.transform.position) >= attackRange) 
        {
            return EnemyState.Chasing;
        }

        if (Vector3.Distance(gameObject.transform.position, _targetObject.transform.position) <= panicRange)
        {
            return EnemyState.Attacking;
        }

        // todo add attack CD
        // if(_attackTimer < attackCD){
        //     _attackTimer += Time.deltaTime;
        // }
        else
        {
            Attack();
        }

        return EnemyState.Attacking;
    }

    private void Attack()
    {
        // TODO: fix when player right on top of monster/ doesn't get hit by raytracing.
        // TODO: Play animation
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attackRange, targetMask))
        {

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

    public void Rotate()
    {

        Vector3 direction = new Vector3(_targetObject.transform.position.x, transform.position.y, _targetObject.transform.position.z);
        transform.LookAt(direction);
    }

    private void OnDrawGizmos()
    {
        if (!_agent)
        {
            return;
        }

        Debug.DrawLine(transform.position, transform.position + transform.forward);
        if (showPatrolInfo && currentState == EnemyState.Idle)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(_patrolCenter, 0.1f);
            Gizmos.DrawWireSphere(_patrolCenter, patrolRadius);
        }

        if (showChaseInfo && currentState == EnemyState.Chasing)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, patrolRadius);
        }

        if (showAttackInfo && currentState == EnemyState.Attacking)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * attackRange);
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}
