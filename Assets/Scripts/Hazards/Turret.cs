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
    [SerializeField] private GameObject _deathExplosion;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _projectileSpawn;
    [SerializeField] private LayerMask _targetMask;
    private GameObject _target;
    private AudioSource _audio;

    private void Start()
    {
        _health = GetComponent<HealthSystem>();
        _audio = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // If the turret's health is 0, make it explode and destroy itself.
        if (_health.CurrentHealth <= 0)
        {
            Die();
            return;
        }
        _target = FindTarget();
        // If a player is found and turret is not on cooldown, shoot at the player and reset cooldown progress.
        if (_target != null && _cooldownProgress >= _attackCooldown)
        {
            Shoot(_target);
            _cooldownProgress = 0;
        }
        // Update the cooldown of the turret.
        if (_cooldownProgress < _attackCooldown)
        {
            _cooldownProgress += Time.deltaTime;
        }
        // Progress time towards the next target check.
    }

    private void Shoot(GameObject target)
    {
        // Create an energy projectile and initialize it with a target position.
        GameObject projectileObj = Instantiate(_projectile, _projectileSpawn.transform.position, Quaternion.identity);
        EnergyProjectile projectile = projectileObj.GetComponent<EnergyProjectile>();
        projectile.SetTargetPosition(target.transform.position);
    }

    private void Die()
    {
        // Create an explosion at the turret's location and destroy the turret.
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
                if (_target == null)
                {
                    newTarget = collider.gameObject;
                }
                else if (collider.gameObject == _target)
                {
                    newTarget = _target;
                }
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
}
