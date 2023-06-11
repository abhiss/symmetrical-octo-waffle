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
    // detect obstruction
    private bool obstruct = false;

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
            Debug.DrawRay(transform.position, Vector3.Normalize(_targetDirection) * _attackRange, Color.black);
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
            return State.Attack;
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
        _agent.enabled = true;

        // Navigate towards the player.
        _destination = _target.transform.position;
        _agent.SetDestination(_destination);

        // If we are within range to attack, enter attack state.
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _detectionRadius, TargetMask);
        if (hitColliders.Length > 0)
        {
            _target = hitColliders[0].gameObject;
        }
        if (hitColliders.Length > 0 && InSight())
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

        Collider[] hitColliders = Physics.OverlapSphere(transform.position, _attackRange, TargetMask);
        // pick a random target (changing target for multiplayer) (will refactor later)
        if (hitColliders.Length > 0)
        {
            _target = hitColliders[0].gameObject;
        }
        RotateTowardsTarget(_target);
        if (hitColliders.Length > 0 && InSight())
        {
            // Play an attack animation.
            _agent.enabled = false;
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
        direction.y = 0;
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
        CharacterMotor playerMotor = _target.GetComponent<CharacterMotor>();
        GameObject projectileObj = Instantiate(_projectile, _bulletSpawner.transform.position, Quaternion.identity);
        EnemyBullet bulletController = projectileObj.GetComponent<EnemyBullet>();
        float bulletSpeed = _projectileSpeed;
        if(!playerMotor.isGrounded)
        {
            bulletSpeed = 3 * _projectileSpeed;
        }
        bulletController.SetParameter(Vector3.Normalize(_targetDirection), bulletSpeed, AttackDamage);
        Destroy(projectileObj, 10f);
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
        // model has bugs manually adding offsets
        gunpoint.y -= 0.4f;
        _targetDirection = _target.transform.position - gunpoint;
    }

    // Check if target is insight
    public bool InSight(){
        GetTargetDirection();
        RaycastHit hit;
        bool clearShot = false;
        clearShot = Physics.SphereCast(transform.position, 0.5f, Vector3.Normalize(_targetDirection), out hit, _attackRange, ObstacleMask |TargetMask);
        // cast for both wall and sphere, because if only scan for one layer the other layer is ignored you want information of both layer
        if(clearShot)
        {
            GameObject objectHit = hit.transform.gameObject;
            if(objectHit.tag == "Player" && Vector3.Distance(_target.transform.position, this.transform.position) < _attackRange)
            {
                return true;
            }  
        }
        return false;
    }

    void OnDrawGizmos()
    {
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