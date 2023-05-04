using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyMachine
{
    public class BaseEnemy : MonoBehaviour, IEnemyMachine
    {
        public float stopThreshold = 0.5f;
        public LayerMask targetMask;
        public Vector3 destination;
        public EnemyState currentState = EnemyState.Idle;

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
        public float attackRange = 0.0f;

        [Header("Debugging")]
        public bool showPatrolInfo;
        public bool showAttackInfo;
        public bool showChaseInfo;

        void Start()
        {
            _agent = GetComponent<NavMeshAgent>();

            // Init Idle State
            _patrolCenter = transform.position;
            _patrolTargetPoint = transform.position;

            destination = transform.position;
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

            _agent.SetDestination(destination);
            _previousState = currentState;
        }

        public EnemyState IdleStateHandler()
        {
            // Detect player
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, patrolRadius);
            foreach (var hitCollider in hitColliders)
            {
                if (hitCollider.gameObject.layer == targetMask)
                {
                    destination = hitCollider.transform.position;
                    return EnemyState.Chasing;
                }
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
                 _patrolTargetPoint = _patrolCenter + Random.insideUnitSphere * patrolRadius;
                 _patrolIdleTime = 0.0f;
            }
            destination = _patrolTargetPoint;
        }

        public EnemyState ChasingStateHandler()
        {
            if (_agent.remainingDistance <= _agent.stoppingDistance + stopThreshold)
            {
                return EnemyState.Attacking;
            }

            if (_agent.remainingDistance >= patrolRadius)
            {
                destination = _patrolCenter;
                return EnemyState.Idle;
            }

            return EnemyState.Chasing;
        }

        public EnemyState AttackingStateHandler()
        {
            if (_agent.remainingDistance > _agent.stoppingDistance + attackRange)
            {
                return EnemyState.Chasing;
            }

            // TODO: Raycast a attack
            return EnemyState.Attacking;
        }

        void OnDrawGizmos()
        {
            if (!_agent)
            {
                return;
            }

            if (showPatrolInfo && currentState == EnemyState.Idle)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(_patrolCenter, 0.1f);
                Gizmos.DrawWireSphere(_patrolCenter, patrolRadius);
            }

            if (showChaseInfo && currentState == EnemyState.Chasing)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(_patrolCenter, patrolRadius);
            }

            if (showAttackInfo && currentState == EnemyState.Attacking)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(_patrolCenter, attackRange);
            }
        }
    }
}
