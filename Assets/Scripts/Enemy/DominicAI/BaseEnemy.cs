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
        public float patrolRadius = 2.5f;
        public float patrolHoldTime = 1.0f;
        private Vector3 _patrolCenter = Vector3.zero;
        private float _patrolIdleTime = 0.0f;
        private Vector3 _patrolTargetPoint;
        private EnemyState _previousState;
        private NavMeshAgent _agent;

        [Header("Attack Properties")]
        public float attackForce = 2.0f;
        public float attackTime = 1.0f;
        public float attackRange = 1.0f;
        private bool _isAttacking = false;

        [Header("Debugging")]
        public bool showPatrolInfo;
        public bool showAttackInfo;
        public bool showChaseInfo;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();
            _healthSystem = GetComponent<HealthSystem>();
            _healthSystem.OnDamageEvent += new EventHandler<HealthSystem.OnDamageArgs>((_, args) => {
                if(args.newHealth < 0)
                {
                    //dead
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

            // TODO: Doesnt work
            if (Physics.SphereCast(transform.position, 1.0f, Vector3.up, out RaycastHit hit, 0.1f, targetMask))
            {
                CharacterMotor player = hit.transform.gameObject.GetComponent<CharacterMotor>();
                if (player != null)
                {
                    Debug.Log("pushing");
                    Vector3 dir = hit.transform.position - transform.position;
                    player.AddForce(dir);
                }
            }

            _agent.SetDestination(destination);
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
            if (_agent.remainingDistance <= _agent.stoppingDistance + stopThreshold)
            {
                _patrolIdleTime += Time.deltaTime;
            }

            // Update when timer is finished
            if (_patrolIdleTime >= patrolHoldTime)
            {
                 _patrolTargetPoint = _patrolCenter + UnityEngine.Random.insideUnitSphere * patrolRadius;
                 _patrolIdleTime = 0.0f;
            }
            destination = _patrolTargetPoint;
        }

        public EnemyState ChasingStateHandler()
        {
            destination = _targetObject.transform.position;
            // 1.0f = Player Radius + Enemy Radius
            float currentDistance = _agent.stoppingDistance + stopThreshold + 1.0f;
            if (_agent.remainingDistance <= currentDistance)
            {
                return EnemyState.Attacking;
            }

            return EnemyState.Chasing;
        }

        public EnemyState AttackingStateHandler()
        {
            if (!_isAttacking) {
                StartCoroutine(AttackCoroutine());
            }
            else
            {
                return EnemyState.Attacking;
            }

            return EnemyState.Chasing;
        }

        private IEnumerator AttackCoroutine()
        {
            _isAttacking = true;

            // TODO: Play animation
            if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, attackRange, targetMask))
            {
                CharacterMotor player = hit.transform.gameObject.GetComponent<CharacterMotor>();
                if (player != null)
                {
                    // Debug.Log("Attacking");
                    player.AddForce(transform.forward * attackForce);
                }
            }

            yield return new WaitForSeconds(attackTime);
            _isAttacking = false;
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
