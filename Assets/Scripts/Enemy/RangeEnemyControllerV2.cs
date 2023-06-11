using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;
using EnemyMachine;

[RequireComponent(typeof(Shared.HealthSystem))]
public class RangeEnemyControllerV2 : MonoBehaviour
{
    // private enum State
    // {
    //     Idle,
    //     Patrol,
    //     Chase,
    //     Attack,
    // }

    [Header("General")]
    public float AttackDamage = 5.0f;
    private HealthSystem _healthSystem;
    private Vector3 _targetDirection;
    [SerializeField] private Vector3 _hitboxOffset;
    [SerializeField] private Vector3 _hitbox;
    private LayerMask ObstacleMask = 8;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _bulletSpawner;
    [SerializeField] private float _projectileSpeed;

    [Header("AI")]
    public LayerMask TargetMask;
    [SerializeField] private float _detectionRadius = 5.0f;
    [SerializeField] private EnemyState currentState;
    [SerializeField] private GameObject _target;
    [SerializeField] private float _attackRange = 15f;
    private NavMeshAgent _agent;
    // _previousLocation is to prevent enemy getting stuck with each other
    private Vector3 _previousLocation;
    private Vector3 _destination;
    private bool _isDead = false;
    //Patrolling
    // [SerializeField] public float patrolRadius = 2.5f; 
    // [SerializeField] private float _timeToRoam;
    // private float _timeRoamed;
    public float patrolRadius = 2.5f; // if within radius switch from idle to attack
    public float patrolHoldTime = 1.0f;
    private Vector3 _patrolCenter = Vector3.zero;
    private float _patrolIdleTime = 0.0f;
    private Vector3 _patrolTargetPoint;

    [Header("Animation/Sound/VFX")]
    private Animator _animator;
    private AudioSource _audio;
    [SerializeField] private AudioClip _hurtSound;
    [SerializeField] private AudioClip _missSound;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private GameObject _explosionPrefab;
    [SerializeField] private GameObject _sparksPrefab;

    [Header("Gizmos")]
    [SerializeField] private bool _showHitbox;
    [SerializeField] private bool _showPatrolRadius;
    [SerializeField] private bool _showDetectionRadius;

    void Start()
    {
        _patrolCenter = transform.position;
        _agent = GetComponentInChildren<NavMeshAgent>();
        // get animator
        GameObject child = transform.GetChild(0).gameObject;
        _animator = child.GetComponent<Animator>();
        _audio = GetComponent<AudioSource>();
        _healthSystem = GetComponent<HealthSystem>();
        _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => 
        {
            // Emit sparks when getting hit.
            Destroy(Instantiate(_sparksPrefab, transform.position, Quaternion.identity), 0.25f);
            // If we are dead, spawn an explosion and destroy ourselves.
            if (args.newHealth <= 0)
            {
                _agent.isStopped = true;
                _isDead = true;
                _animator.SetTrigger("Die");
                // Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
                // Destroy(gameObject);
                return;
            }
            // Play a hurt sound when getting hit.
            _audio.clip = _hurtSound;
            _audio.Play();
            // If we get hit, start chasing the enemy.
            if (_target == null && (currentState == EnemyState.Idle))
            {
                _target = args.attacker;
                currentState = EnemyState.Chasing;
            }
        });
        currentState = EnemyState.Idle;
    }

    void Update()
    {
        // if enemy is dead
        if(_isDead)
        {
            return;
        }
        // debug ----
        if(_target != null)
        {
            GetTargetDirection();
            Debug.DrawRay(transform.position, transform.TransformDirection(_targetDirection * _attackRange),Color.green);
        }
        currentState = currentState switch
        {
            EnemyState.Idle => IdleStateHandler(),
            EnemyState.Chasing => ChasingStateHandler(),
            EnemyState.Attacking => AttackingStateHandler(),
            _ => IdleStateHandler()
        };
    }

    // private State PatrolState()
    // {
    //     // Play a patrol animation.
    //     _animator.SetTrigger("Patrol");
    //     // If we're close enough to our destination or we've roamed for long enough, patrol somewhere else.
    //     if (Vector3.Distance(transform.position, _destination) <= 1 || _timeRoamed > _timeToRoam)
    //     {
    //         RefreshPatrolPoint();
    //     }
    //     // If there are enemies in our detection radius, and enter chase state if they are.
    //     Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, TargetMask);
    //     if (hitColliders.Length > 0)
    //     {
    //         _target = hitColliders[0].gameObject;
    //         return State.Chase;
    //     }
    //     // If there are no enemies in detection radius, continue patrolling.
    //     else
    //     {
    //         _timeRoamed += Time.deltaTime;
    //         return State.Patrol;
    //     }
    // }

    // private void RefreshPatrolPoint()
    // {
    //     Vector3 _patrolTargetPoint = UnityEngine.Random.insideUnitSphere * _patrolRadius;
    //     _patrolTargetPoint.y = transform.position.y;
    //     _destination = _patrolTargetPoint;
    //     _agent.SetDestination(_destination);
    //     _timeRoamed = 0;
    // }

    // private State IdleState()
    // {
    //     // If there are enemies in detection radius, enter a chase state. Otherwise, enter a patrol state.
    //     Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, TargetMask);
    //     if (hitColliders.Length > 0)
    //     {
    //         _target = hitColliders[0].gameObject;
    //         return State.Chase;
    //     }
    //     return State.Patrol;
    // }

     public EnemyState IdleStateHandler()
    {
        // Detect player
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, patrolRadius, TargetMask);
        foreach (var hitCollider in hitColliders)
        {
            _target = hitCollider.gameObject;
            return EnemyState.Chasing;
        }
        _animator.SetTrigger("Patrol");
        Patrol();
        return EnemyState.Idle;
    }

    private void Patrol()
    {
        // Start a timer at position
        // if stopped at destination (within the range) start timer
        if (_agent.remainingDistance <= _agent.stoppingDistance)
        {
            _patrolIdleTime += Time.deltaTime;
        }

        // Update when timer is finished (get new destination)
        if (_patrolIdleTime >= patrolHoldTime || isStuck())
        {
            _animator.ResetTrigger("Patrol");
            _animator.SetTrigger("Idle");
            _patrolTargetPoint = _patrolCenter + UnityEngine.Random.insideUnitSphere * patrolRadius;
            _patrolIdleTime = 0.0f;
        }
        _destination = _patrolTargetPoint;
        _agent.SetDestination(_destination);
        _previousLocation = transform.position;
    }

    private EnemyState ChasingStateHandler()
    {
        // Play a chase animation.
        _animator.SetTrigger("Chase");
        _agent.Resume();

        // Navigate towards the player.
        _destination = _target.transform.position;
        _agent.SetDestination(_destination);

        // If we are within range to attack, enter attack state.
        
        Collider[] hitColliders = Physics.OverlapBox(transform.position+_hitboxOffset, _hitbox, transform.rotation, TargetMask);
        if (hitColliders.Length > 0)
        {
            return EnemyState.Attacking;
        }
        

        // Stay in chase state if we're not going to attack.
        return EnemyState.Chasing;
    }

    // Note: Due to NavMesh agent, the droid may slide as it is attacking. This needs to be fixed.
    private EnemyState AttackingStateHandler()
    {
        // If we are in attack range, attack and stay in attack state.
        _agent.Stop();

        Collider[] hitColliders = Physics.OverlapBox(transform.position+_hitboxOffset, _hitbox, transform.rotation, TargetMask);
        RotateTowardsTarget(_target);
        if (hitColliders.Length > 0)
        {
            // Play an attack animation.
            _animator.SetTrigger("Attack");
            return EnemyState.Attacking;
        }
        // If we are no longer in attack range, enter a chase state.
        else
        {
            _animator.ResetTrigger("Attack");
            return EnemyState.Chasing;
        }
    }

    void RotateTowardsTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float rotateSpeed = 5f;
        float step = rotateSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }

    public void Attack()
    {
        // Attack and deal damage to any valid players within our hitbox.
        RotateTowardsTarget(_target);
        GetTargetDirection();
        GameObject projectileObj = Instantiate(_projectile, _bulletSpawner.transform.position, Quaternion.identity);
        EnemyBullet bulletController = projectileObj.GetComponent<EnemyBullet>();
        bulletController.SetParameter(Vector3.Normalize(_targetDirection),  _projectileSpeed, AttackDamage);
        Destroy(projectileObj, 10f);
    }

    public void Death()
    {
        Debug.Log("die");
        Explosion explosionScript =_explosionPrefab.GetComponent<Explosion>();
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void GetTargetDirection()
    {
        Vector3 gunpoint = _bulletSpawner.transform.position;
        _targetDirection = _target.transform.position - gunpoint;
    }

    public bool InSight(){
        //GetTargetDirection();
        // RaycastHit hit;
        // bool obstruct = false;
        // obstruct = Physics.Raycast(transform.position+_hitboxOffset,Vector3.Normalize(_targetDirection), out hit, _attackRange, TargetMask);
        if ( Vector3.Distance(_target.transform.position, this.transform.position) < _attackRange)
        {
            return true;
        }
        return false;
    }

    private bool isStuck(){
        if (_previousLocation == this.transform.position && _agent.remainingDistance > _agent.stoppingDistance)
        {
            return true;
        }
        return false;
    }

    void OnDrawGizmos()
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
            Gizmos.DrawSphere(transform.position, patrolRadius);
        }
        if (_showDetectionRadius)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, _detectionRadius);
        }
    }
}