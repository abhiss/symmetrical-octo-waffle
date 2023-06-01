using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;

namespace EnemyMachine
{
    [RequireComponent(typeof(Shared.HealthSystem))]
    public class BaseEnemy : MonoBehaviour, IEnemyMachine
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
        public float attackCD = 1.0f;
        public float attackRange = 1.0f;
        private float _attackTimer;
        //private bool _isAttacking = false;

        [Header("Debugging")]
        public bool showPatrolInfo;
        public bool showAttackInfo;
        public bool showChaseInfo;

        [Header("Animation")]
        private Animator _animator;
        [SerializeField] private GameObject _animatorModel;

        void Start()
        {
            _attackTimer = attackCD;
            _agent = GetComponent<NavMeshAgent>();
            _animator = _animatorModel.GetComponent<Animator>();
            _healthSystem = GetComponent<HealthSystem>();
            _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => {
                if(args.newHealth <= 0)
                {
                    _agent.isStopped = true;
                    _animator.SetTrigger("Die");
                    Debug.Log("Enemy dead.");
                    Destroy(gameObject, 2);
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

            // TODO: Doesnt work
            // if (Physics.SphereCast(transform.position, 1.0f, Vector3.up, out RaycastHit hit, 0.1f, targetMask))
            // {
            //     CharacterMotor player = hit.transform.gameObject.GetComponent<CharacterMotor>();
            //     if (player != null)
            //     {
            //         Debug.Log("pushing");
            //         Vector3 dir = hit.transform.position - transform.position;
            //         player.AddForce(dir);
            //     }
            // }
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
            _animator.SetTrigger("Patrol");
            Patrol();
            return EnemyState.Idle;
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

        public EnemyState ChasingStateHandler()
        {
            if(CheckPlayerDead())
            {
                return EnemyState.Idle;
            }
            _animator.SetTrigger("Chase");
            destination = _targetObject.transform.position;
            _agent.SetDestination(destination);
            // 1.0f = Player Radius + Enemy Radius
            // currentDistance = attack range
            float currentDistance = _agent.stoppingDistance + stopThreshold + 1.0f;
            if (_agent.remainingDistance <= currentDistance)
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

            if(_attackTimer < attackCD){
                _attackTimer += Time.deltaTime;
                return EnemyState.Chasing;
            }

            if (Vector3.Distance(gameObject.transform.position, _targetObject.transform.position) <= attackRange) 
            {
                Attack();
                return EnemyState.Attacking;
            }
            return EnemyState.Chasing;
        }

        private void Attack()
        {
            // TODO: fix when player right on top of monster/ doesn't get hit by raytracing.
            _animator.SetTrigger("Attack");
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attackRange, targetMask))
            {
                CharacterMotor player = hit.transform.gameObject.GetComponent<CharacterMotor>();
                Shared.HealthSystem playerHealthSystem = hit.collider.GetComponent<Shared.HealthSystem>();
                if (player != null)
                {
                    // Debug.Log("Attacking");
                    playerHealthSystem.TakeDamage(gameObject, AttackDamage);
                    _attackTimer = 0f;
                    player.AddForce(transform.forward * attackForce);
                    return;
                }
            }
            //if player is inside the enemy, (within radius but not hitable by raycast)
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f, targetMask);
            foreach (var hitCollider in hitColliders)
            {
                GameObject player = hitCollider.gameObject;
                CharacterMotor playerMotor = player.GetComponent<CharacterMotor>();
                Shared.HealthSystem playerHealthSystem = player.GetComponent<Shared.HealthSystem>();
                if (player != null)
                {
                    // Debug.Log("Attacking");
                    playerHealthSystem.TakeDamage(gameObject, AttackDamage);
                    _attackTimer = 0f;
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
}
