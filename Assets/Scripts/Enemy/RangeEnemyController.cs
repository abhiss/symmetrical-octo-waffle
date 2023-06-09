using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;

[RequireComponent(typeof(Shared.HealthSystem))]
public class RangeEnemyController : MonoBehaviour
{
    private enum State
    {
        Idle,
        Patrol,
        Chase,
        Attack,
    }

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
    [SerializeField] private State currentState;
    [SerializeField] private GameObject _target;
    [SerializeField] private float _attackRange = 15f;
    private NavMeshAgent _agent;
    private Vector3 _destination;
    private bool _isDead = false;
    //Patrolling
    [SerializeField] public float _patrolRadius = 2.5f; 
    [SerializeField] private float _timeToRoam;
    private float _timeRoamed;

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
        _agent = GetComponent<NavMeshAgent>();
        _animator = GetComponent<Animator>();
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
            if (_target == null && (currentState == State.Idle || currentState == State.Patrol))
            {
                _target = args.attacker;
                currentState = State.Chase;
            }
        });
        currentState = State.Idle;
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
            State.Idle => IdleState(),
            State.Patrol => PatrolState(),
            State.Chase => ChaseState(),
            State.Attack => AttackState(),
            _ => IdleState()
        };
    }

    private State PatrolState()
    {
        // Play a patrol animation.
        _animator.SetTrigger("Patrol");
        // If we're close enough to our destination or we've roamed for long enough, patrol somewhere else.
        if (Vector3.Distance(transform.position, _destination) <= 1 || _timeRoamed > _timeToRoam)
        {
            RefreshPatrolPoint();
        }
        // If there are enemies in our detection radius, and enter chase state if they are.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, TargetMask);
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
        Vector3 _patrolTargetPoint = UnityEngine.Random.insideUnitSphere * _patrolRadius;
        _patrolTargetPoint.y = transform.position.y;
        _destination = _patrolTargetPoint;
        _agent.SetDestination(_destination);
        _timeRoamed = 0;
    }

    private State IdleState()
    {
        // If there are enemies in detection radius, enter a chase state. Otherwise, enter a patrol state.
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, TargetMask);
        if (hitColliders.Length > 0)
        {
            _target = hitColliders[0].gameObject;
            return State.Chase;
        }
        return State.Patrol;
    }

    private State ChaseState()
    {
        // Play a chase animation.
        _animator.SetTrigger("Chase");
        //_agent.Resume();

        // Navigate towards the player.
        _destination = _target.transform.position;
        _agent.SetDestination(_destination);

        // If we are within range to attack, enter attack state.
        
        Collider[] hitColliders = Physics.OverlapBox(transform.position+_hitboxOffset, _hitbox, transform.rotation, TargetMask);
        if (hitColliders.Length > 0)
        {
            return State.Attack;
        }
        

        // Stay in chase state if we're not going to attack.
        return State.Chase;
    }

    // Note: Due to NavMesh agent, the droid may slide as it is attacking. This needs to be fixed.
    private State AttackState()
    {
        // If we are in attack range, attack and stay in attack state.
        //_agent.Stop();

        Collider[] hitColliders = Physics.OverlapBox(transform.position+_hitboxOffset, _hitbox, transform.rotation, TargetMask);
        RotateTowardsTarget(_target);
        if (hitColliders.Length > 0)
        {
            // Play an attack animation.
            _animator.SetTrigger("Attack");
            return State.Attack;
        }
        // If we are no longer in attack range, enter a chase state.
        else
        {
            _animator.ResetTrigger("Attack");
            return State.Chase;
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
    }

    public void Death()
    {
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
            Gizmos.DrawSphere(transform.position, _patrolRadius);
        }
        if (_showDetectionRadius)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(transform.position, _detectionRadius);
        }
    }
}