using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class Turret : NetworkBehaviour
{
    private HealthSystem _health;
    [SerializeField] private float _damage;
    [SerializeField] private float _detectionRadius;
    [SerializeField] private float _attackCooldown;
    private float _cooldownProgress;

    [SerializeField] private LayerMask _targetMask;
    private GameObject _target;

    [SerializeField] private GameObject _deathExplosion;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _projectileSpawn;

    private AudioSource _audio;

    [Header("Gizmos")]
    [SerializeField] private bool _showDetectionRadius;

    private void Start()
    {
        _health = GetComponent<HealthSystem>();
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (_health.CurrentHealth <= 0)
        {
            Die();
            return;
        }
        if(!IsServer) return;
        _target = FindTarget();
        // If we found a target and turret is not on cooldown, shoot at the player and reset cooldown progress.
        if (_target != null && _cooldownProgress >= _attackCooldown)
        {
            Shoot(_target);
            _cooldownProgress = 0;
        }
        // Increment the cooldown of the turret.
        if (_cooldownProgress < _attackCooldown)
        {
            _cooldownProgress += Time.deltaTime;
        }
    }
    [ClientRpc]
    private void ShootClientRpc(Vector3 targetPos){
        ShootInner(targetPos);
    }
    [ServerRpc]
    private void ShootServerRpc(Vector3 targetPos){
        ShootClientRpc(targetPos);
    }
    private void ShootInner(Vector3 targetPos){
                // Create an energy projectile and initialize it with a target position.
        GameObject projectileObj = Instantiate(_projectile, _projectileSpawn.transform.position, Quaternion.identity);
        EnergyProjectile projectile = projectileObj.GetComponent<EnergyProjectile>();
        projectile.SetTargetPosition(targetPos);
    }
    private void Shoot(GameObject target)
    {
        ShootServerRpc(target.transform.position);
        ShootInner(target.transform.position);
    }

    private void Die()
    {
        Instantiate(_deathExplosion, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private GameObject FindTarget()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, _detectionRadius, _targetMask);
        GameObject newTarget = null;
        // Find targets within detection range of the turret.
        if (collidersInRange.Length > 0)
        {
            foreach (Collider collider in collidersInRange)
            {
                // If we have no current target, make the first target we see the new target.
                if (_target == null)
                {
                    newTarget = collider.gameObject;
                }
                // If we found our current target again, make that our new target.
                else if (collider.gameObject == _target)
                {
                    newTarget = _target;
                }
                // If there is a new target, look towards it.
                if (newTarget != null)
                {
                    transform.LookAt(newTarget.transform);
                }
                if (!_audio.isPlaying)
                {
                    _audio.Play();
                }
            }
        }
        return newTarget;
    }

    private void OnDrawGizmos()
    {
        if (_showDetectionRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _detectionRadius);
        }
    }
}
