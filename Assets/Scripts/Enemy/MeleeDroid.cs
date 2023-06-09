using Shared;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SubsystemsImplementation;

public class MeleeDroid : Enemy
{
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _missSound;

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    // Note: Due to NavMesh agent, the droid may slide as it is attacking. This needs to be fixed.
    protected override State AttackState()
    {
        // If we are in attack range, attack and stay in attack state.
        Collider[] hitColliders = Physics.OverlapBox(transform.position+HitboxOffset, Hitbox, transform.rotation, TargetMask);
        RotateTowardsTarget(Target);
        if (hitColliders.Length > 0)
        {
            // Play an attack animation.
            EnemyAnimator.SetTrigger("Attack");
            return State.Attack;
        }
        // If we are no longer in attack range, enter a chase state.
        else
        {
            EnemyAnimator.ResetTrigger("Attack");
            return State.Chase;
        }
    }

    public override void Attack()
    {
        // Attack and deal damage to any valid players within our hitbox.
        Collider[] hitColliders = Physics.OverlapBox(transform.position+HitboxOffset, Hitbox, transform.rotation, TargetMask);
        RotateTowardsTarget(Target);
        foreach (var hitCollider in hitColliders)
        {
            GameObject player = hitCollider.gameObject;
            Shared.HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
            if (player != null)
            {
                Audio.clip = _hitSound;
                Audio.Play();
                playerHealthSystem.TakeDamage(gameObject, AttackDamage);
                return;
            }
        }
        Audio.clip = _missSound;
        Audio.Play();
    }

    private void RotateTowardsTarget(GameObject target)
    {
        Vector3 direction = target.transform.position - transform.position;
        float rotateSpeed = 5f;
        float step = rotateSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}