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
        //  If the enemy is no longer alive, play a death animation.
        if (!_enemy.IsAlive)
        {
            _animator.SetTrigger("Die");
            return;
        }
        //  Handle enemy movement animations based on current state.
        switch (_currentState)
        {
            //  Enemy is idle, so it walks at regular speed and plays an idle animation.
            case IdleState:
                _navMeshAgent.speed = _enemy.Speed;
                _animator.SetTrigger("Idle");
                break;
            //  Enemy is chasing the player, so it runs at 1.5x speed and plays a chase animation.
            case ChaseState:
                _navMeshAgent.speed = _enemy.Speed * 1.5f;
                _animator.SetTrigger("Chase");
                break;
            //  Enemy is attacking the player, so it halts movement and plays an attack animation.
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
