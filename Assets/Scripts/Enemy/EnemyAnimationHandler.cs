using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAnimationHandler : MonoBehaviour
{
    private Enemy _enemy;
    private StateMachine _stateMachine;
    private Animator _animator;
    private string _currentState;

    void Start()
    {
        _enemy = this.GetComponent<Enemy>();
        _stateMachine = this.GetComponent<StateMachine>();
        _animator = this.GetComponent<Animator>();
        _currentState = _stateMachine.GetCurrentStateAsString();
    }

    public void UpdateAnimationTrigger()
    {
        _currentState = _stateMachine.GetCurrentStateAsString();
        if (!_enemy.IsAlive)
        {
            _animator.SetTrigger("Die");
        }
        //  Handle enemy movement animations based on current state.
        switch (_currentState)
        {
            case "Idle":
                _animator.SetTrigger("Idle");
                break;
            case "Chase":
                _animator.SetTrigger("Chase");
                break;
            case "Attack":
                _animator.SetTrigger("Attack");
                break;
            default:
                //  If current state is somehow invalid, just play an idle animation.
                _animator.SetTrigger("Idle");
                break;
        }
    }
    
}
