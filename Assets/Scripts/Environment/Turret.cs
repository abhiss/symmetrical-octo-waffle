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

    // Throttles polling for targets in detection radius. No reason for turret to reorient itself on every single frame.
    private float _timeToNextTargetCheck;
    private float _targetCheckTime = 0.05f;

    // Prefab for explosion that will be spawned on turret death.
    [SerializeField] private GameObject _explosionPrefab;
    // Prefab for projectile that will be shot from the turret.
    [SerializeField] private GameObject _projectilePrefab;
    // Location for projectiles to be spawned from, should be a location in front of the turret.
    [SerializeField] private GameObject _projectileSpawn;
    // An audio source that provides a "detection" sound for the turret.
    private AudioSource _audio;

    void Awake()
    {
        _health = GetComponent<HealthSystem>();
        _audio = GetComponent<AudioSource>();
    }

    void Update()
    {
        // If the turret's health is 0, make it explode and destroy itself.
        if (_health.CurrentHealth <= 0)
        {
            Die();
            return;
        }
        // If turret is ready to check for targets, reset its time towards the next check and try to find a target.
        if (_timeToNextTargetCheck >= _targetCheckTime)
        {
            _timeToNextTargetCheck = 0;
            GameObject player = FindTarget();
            // If a player is found and turret is not on cooldown, shoot at the player and reset cooldown progress.
            if (player != null  && _cooldownProgress >= _attackCooldown)
            {
                Shoot(player);
                _cooldownProgress = 0;
            }
        }
        // Update the cooldown of the turret.
        if (_cooldownProgress < _attackCooldown)
        {
            _cooldownProgress += Time.deltaTime;
        }
        // Progress time towards the next target check.
        _timeToNextTargetCheck += Time.deltaTime;
    }

    private void Shoot(GameObject target)
    {
        // Create an energy projectile and initialize it with a target position.
        GameObject projectileObj = Instantiate(_projectilePrefab, _projectileSpawn.transform.position, Quaternion.identity);
        EnergyProjectile projectile = projectileObj.GetComponent<EnergyProjectile>();
        projectile.InitializeWithTargetPos(target.transform.position);
    }

    private void Die()
    {
        // Create an explosion at the turret's location and destroy itself.
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    private GameObject FindTarget()
    {
        Collider[] collidersInRange = Physics.OverlapSphere(transform.position, _detectionRadius);
        // Find a target within detection range of the turret.
        foreach (Collider collider in collidersInRange)
        {
            bool isValidTarget = collider.CompareTag("Player");
            // If a collider is a valid target, look at it and play a beeping detection sound.
            if (isValidTarget)
            {
                if (!_audio.isPlaying)
                {
                    _audio.Play();
                }
                transform.LookAt(collider.transform);
                return collider.gameObject;
            }
        }
        _audio.Stop();
        return null;
    }
}
