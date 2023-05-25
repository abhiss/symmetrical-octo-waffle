using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Enemy _enemy;
    private StateMachine _stateMachine;
    private Animator _animator;
    private State _currentState;
    private UnityEngine.AI.NavMeshAgent _navMeshAgent;

    void Awake()
    {
        _enemy = GetComponent<Enemy>();
        _animator = GetComponent<Animator>();
        _navMeshAgent = GetComponent<UnityEngine.AI.NavMeshAgent>();
    }

    public void AnimateAttack()
    {
        // Stop the nav mesh agent so the enemy stays in place while playing attack animation.
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger("Attack");
    }

    public void AnimateIdle()
    {
        // Enable the nav mesh agent and make it move at regular speed while playing idle animation.
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _enemy.Speed;
        _animator.SetTrigger("Idle");
    }

    public void AnimateChase()
    {
        // Enable the nav mesh agent and make it move at a faster speed while playing chase animation.
        _navMeshAgent.isStopped = false;
        _navMeshAgent.speed = _enemy.Speed * 1.5f;
        _animator.SetTrigger("Chase");
    }

    public void AnimateDeath()
    {
        // Disable the nav mesh agent so the enemy stays in place while playing death animation.
        _navMeshAgent.isStopped = true;
        _animator.SetTrigger("Die");
    }
}
