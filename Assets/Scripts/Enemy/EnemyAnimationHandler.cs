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
        _enemy = this.GetComponent<Enemy>();
        _stateMachine = this.GetComponent<StateMachine>();
        _animator = this.GetComponent<Animator>();
        _navMeshAgent = this.GetComponent<UnityEngine.AI.NavMeshAgent>();
        _currentState = _stateMachine.GetCurrentState();
    }

    public void UpdateAnimationTrigger()
    {
        _currentState = _stateMachine.GetCurrentState();
        if (!_enemy.IsAlive)
        {
            _animator.SetTrigger("Die");
        }
        //  Handle enemy movement animations based on current state.
        switch (_currentState)
        {
            case IdleState:
                _navMeshAgent.speed = _enemy.Speed;
                _animator.SetTrigger("Idle");
                break;
            case ChaseState:
                _navMeshAgent.speed = _enemy.Speed * 1.5f;
                _animator.SetTrigger("Chase");
                break;
            case AttackState:
                _navMeshAgent.speed = 0;
                _animator.SetTrigger("Attack");
                break;
            default:
                //  If current state is somehow invalid, just play an idle animation.
                _animator.SetTrigger("Idle");
                break;
        }
    }
    
}
