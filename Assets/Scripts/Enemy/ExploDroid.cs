using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;

public class ExploDroid : Enemy
{
    [SerializeField] private GameObject _detonationExplosion;
    [SerializeField] private AudioClip _warningSound;

    // Note: Due to NavMesh agent, the droid may slide as it is attacking. This needs to be fixed.
    protected override State AttackState()
    {
        if (Audio.clip != _warningSound && !Audio.isPlaying)
        {
            Audio.clip = _warningSound;
            Audio.Play();
        }
        EnemyAnimator.SetTrigger("Attack");
        return State.Attack;
    }

    public override void Attack()
    {
        Instantiate(_detonationExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}