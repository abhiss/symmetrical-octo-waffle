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
    private float _timeToNextTargetCheck;
    // Throttles polling for targets in detection radius.
    private float _targetCheckTime = 0.05f;
    [SerializeField] private GameObject _explosionPrefab;
    private AudioSource _audioSource;

    void Awake()
    {
        _health = GetComponent<HealthSystem>();
        _audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (_health.currentHealth <= 0)
        {
            Die();
            return;
        }
        // If turret is ready to check for targets, reset its time towards the next check.
        if (_timeToNextTargetCheck >= _targetCheckTime)
        {
            _timeToNextTargetCheck = 0;
            GameObject player = FindTarget();
            // If a valid player target exists, and turret is not on cooldown, look at them.
            if (player != null)
            {
                // If turret is not on cooldown, shoot at the player.
                if (_cooldownProgress >= _attackCooldown)
                {
                    Shoot(player);
                }
            }
        }
        // Update the cooldown of the turret.
        if (_cooldownProgress < _attackCooldown)
        {
            _cooldownProgress += Time.deltaTime;
        }
        // Increment time towards the next target check.
        _timeToNextTargetCheck += Time.deltaTime;
    }

    private void Shoot(GameObject target)
    {
        // Generate an explosion at the target's location. Placeholder implementation, later will shoot a projecitle.
        Instantiate(_explosionPrefab, target.transform.position, Quaternion.identity);
        _cooldownProgress = 0;
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
        // Find a target within range of the turret.
        foreach (Collider collider in collidersInRange)
        {
            bool isValidTarget = collider.CompareTag("Player");
            // If a collider is a valid target, look at it and play a beeping detection sound.
            if (isValidTarget)
            {
                if (!_audioSource.isPlaying)
                {
                    _audioSource.Play();
                }
                transform.LookAt(collider.transform);
                return collider.gameObject;
            }
        }
        _audioSource.Stop();
        return null;
    }
}
