using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;
using Shared;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Shared.HealthSystem))]
public abstract class Enemy : NetworkBehaviour
{
    protected enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
    }
    [Header("Stats")]
    private HealthSystem _healthSystem;
    [SerializeField] private float _attackDamage;
    [SerializeField] private float _speed;
    [SerializeField] private int _lootDropRate;
    [SerializeField] private Vector3 _hitbox;
    [SerializeField] private Vector3 _hitboxOffset;

    [Header("AI")]
    public bool ChaseOnSpawn;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private GameObject _target;
    [SerializeField] private float _detectionRadius = 5.0f;
    [SerializeField] private State _currentState;
    private NavMeshAgent _agent;
    private Vector3 _destination;
    [SerializeField] private float _patrolRadius = 2.5f; 
    [SerializeField] private float _timeToRoam;
    private float _timeRoamed;

    [Header("Effects")]
    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private GameObject _deathExplosion;
    [SerializeField] private GameObject _hitSparks;
    [SerializeField] private GameObject _loot;
    private AudioSource _audio;
    private Animator _animator;

    [Header("Gizmos")]
    [SerializeField] private bool _showHitbox;
    [SerializeField] private bool _showPatrolRadius;
    [SerializeField] private bool _showDetectionRadius;

    protected float AttackDamage { get => _attackDamage; }
    protected Vector3 Hitbox { get => _hitbox; }
    protected Vector3 HitboxOffset { get => _hitboxOffset; }
    protected LayerMask TargetMask { get => _targetMask; }
    protected GameObject Target { get => _target; }
    protected Animator EnemyAnimator { get => _animator; }
    protected AudioSource Audio { get => _audio; }
    protected NavMeshAgent NavAgent { get => _agent; }

    protected virtual void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _healthSystem = GetComponent<HealthSystem>();
        InitializeOnDamageEvent();
        _destination = transform.position;

        // If enemy is set to chase on spawn, it will not idle and instead chases the first player it finds.
        GameObject player = GameObject.Find("Character");
        if (ChaseOnSpawn && player != null)
        {
            _target = player;
            _currentState = State.Chase;
        }
        else
        {
            _currentState = State.Idle;
        }
    }

    private void InitializeOnDamageEvent()
    {
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => 
        {
            // Emit sparks when getting hit.
            Instantiate(_hitSparks, transform.position+transform.forward, Quaternion.identity);
            // If we are dead, spawn an explosion, maybe drop loot, and destroy ourselves.
            if (args.newHealth <= 0)
            {
                Instantiate(_deathExplosion, transform.position, Quaternion.identity);
                DropLoot();
                Destroy(gameObject);
                return;
            }
            // Play a hurt sound when getting hit.
            _audio.clip = _hurtSound;
            _audio.Play();
            // If we get hit and we're idling or patrolling, start chasing the enemy.
            if (_target == null && (_currentState == State.Idle || _currentState == State.Patrol))
            {
                _target = args.attacker;
                _currentState = State.Chase;
            }
        });
    }

    private void DropLoot()
    {
        // Random range is max exclusive, so increment end range by 1.
        float randomNumber = UnityEngine.Random.Range(1, 101);
        // Drop loot based on loot drop rate.
        if (_lootDropRate <= randomNumber)
        {
            Instantiate(_loot, transform.position, Quaternion.identity);
        }
    }

    protected virtual void Update()
    {
        TransitionState();
    }

    protected void TransitionState()
    {
        Func<State> idle = () => 
        {
            _agent.speed = 0;
            return IdleState();

        };
        Func<State> patrol = () =>
        {
            _agent.speed = _speed;
            return PatrolState();
        };
        Func<State> chase = () =>
        {
            var chaseModifier = 1.5f;
            _agent.speed = _speed * chaseModifier;
            return ChaseState();

        };
        Func<State> attack = () =>
        {
            _agent.speed = 0;
            return AttackState();
        };
        _currentState = _currentState switch
        {
            State.Idle => idle(),
            State.Patrol => patrol(),
            State.Chase => chase(),
            State.Attack => attack(),
            _ => idle()
        };
    }

    protected State PatrolState()
    {
        _animator.SetTrigger("Patrol");
        // If we're close enough to our destination or we've roamed for long enough, patrol somewhere else.
        if (Vector3.Distance(transform.position, _destination) <= 1.5f || _timeRoamed > _timeToRoam)
        {
            RefreshPatrolPoint();
        }
        // If there are enemies in our detection radius, and enter chase state if they are.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, _targetMask);
        if (hitColliders.Length > 0)
        {
            _target = hitColliders[0].gameObject;
            return State.Chase;
        }
        // If there are no enemies in detection radius, continue patrolling.
        else
        {
            _timeRoamed += Time.deltaTime;
            return State.Patrol;
        }
    }

    private void RefreshPatrolPoint()
    {
        _destination = GenerateValidPatrolPoint();
        _agent.SetDestination(_destination);
        _timeRoamed = 0;
    }
    
    private Vector3 GenerateValidPatrolPoint()
    {
        Vector3 patrolTargetPoint = UnityEngine.Random.insideUnitSphere * _patrolRadius;
        patrolTargetPoint.y = transform.position.y;
        return patrolTargetPoint;
    }

    protected State IdleState()
    {
        _animator.SetTrigger("Idle");
        // If there are enemies in detection radius, enter a chase state. Otherwise, enter a patrol state.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, _targetMask);
        if (hitColliders.Length > 0)
        {
            _target = hitColliders[0].gameObject;
            return State.Chase;
        }
        return State.Patrol;
    }

    protected State ChaseState()
    {
        if (_target == null)
        {
            return State.Idle;
        }
        // Play a chase animation.
        _animator.SetTrigger("Chase");
        // Navigate towards the player.
        _destination = _target.transform.position;
        _agent.SetDestination(_destination);

        // If we are within range to attack, enter attack state.
        Collider[] hitColliders = Physics.OverlapBox(transform.position+_hitboxOffset, _hitbox, transform.rotation, _targetMask);
        if (hitColliders.Length > 0)
        {
            return State.Attack;
        }

        // Stay in chase state if we're not going to attack.
        return State.Chase;
    }

    protected abstract State AttackState();
    public abstract void Attack();

    protected void OnDrawGizmos()
    {
        if (_showHitbox)
        {
            var oldMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(transform.position+_hitboxOffset, transform.rotation, _hitbox);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
            Gizmos.matrix = oldMatrix;
        }
        if (_showPatrolRadius)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.position, _patrolRadius);
        }
        if (_showDetectionRadius)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, _detectionRadius);
        }
    }
}