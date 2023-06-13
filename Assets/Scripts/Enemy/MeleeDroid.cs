using UnityEngine;
using Shared;

public class MeleeDroid : Enemy
{
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _missSound;

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
        // Play a miss sound if no players in hitbox, and a hit sound if any players are in hitbox.
        if (hitColliders.Length == 0)
        {
            Audio.clip = _missSound;
        }
        else
        {
            Audio.clip = _hitSound;
        }
        Audio.Play();
        // For each player in hitbox, deal damage to them.
        foreach (var hitCollider in hitColliders)
        {
            GameObject player = hitCollider.gameObject;
            Shared.HealthSystem playerHealthSystem = player.GetComponent<HealthSystem>();
            if (player != null)
            {
                playerHealthSystem.TakeDamage(gameObject, AttackDamage);
            }
        }
    }

    private void RotateTowardsTarget(GameObject target)
    {
        if (target == null)
        {
            return;
        }
        Vector3 direction = target.transform.position - transform.position;
        float rotateSpeed = 5f;
        float step = rotateSpeed * Time.deltaTime;
        Vector3 newDirection = Vector3.RotateTowards(transform.forward, direction, step, 0.0f);
        transform.rotation = Quaternion.LookRotation(newDirection);
    }
}