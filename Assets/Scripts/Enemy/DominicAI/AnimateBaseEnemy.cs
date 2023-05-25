using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace EnemyMachine
{
    public class AnimateBaseEnemy : MonoBehaviour
    {
        public GameObject model;
        private BaseEnemy _stateMachine;
        private NavMeshAgent _agent;

        [Header("Animations")]
        private Animator _animator;
        private int _velocityHash;
        private int _attackingHash;

        void Start()
        {
            _stateMachine = GetComponent<BaseEnemy>();
            _agent = GetComponent<NavMeshAgent>();

            _animator = model.GetComponent<Animator>();
            _velocityHash = Animator.StringToHash("VelocityMagnitude");
            _attackingHash = Animator.StringToHash("IsAttacking");
        }

        void Update()
        {
            bool IsAttacking = _stateMachine.currentState == EnemyState.Attacking;
            _animator.SetBool(_attackingHash, IsAttacking);
            _animator.SetFloat(_velocityHash, _agent.velocity.magnitude);
        }
    }
}