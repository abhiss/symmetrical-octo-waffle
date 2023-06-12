using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private float _explosionDamage;
    [SerializeField] private float _explosionRadius = 2f;
    private const float _explosionDuration = 2f;
    private AudioSource _explosionAudio;
    private CharacterCamera _characterCamera;

    private void Start()
    {
        _explosionAudio = GetComponent<AudioSource>();
        _characterCamera = Camera.main.transform.GetComponent<CharacterCamera>();

        // Explode as soon as the explosion is instantiated.
        Explode();
    }

    private void Explode()
    {
        // Play an explosion sound effect.
        _explosionAudio.Play();
        // Detect all colliders within a explosion radius.
        Collider[] collidersInExplosionRadius = Physics.OverlapSphere(transform.position, _explosionRadius);
        foreach (Collider collider in collidersInExplosionRadius)
        {
            // Can be succinctly converted to a layer mask if more interactions become possible.
            bool isValidTarget = collider.CompareTag("Player") || collider.CompareTag("Enemy") || collider.CompareTag("Destructible");
            // If a collider is a valid target, deal damage to its health.
            if (isValidTarget)
            {
                HealthSystem targetHealth = collider.gameObject.GetComponent<HealthSystem>();
                targetHealth.TakeDamage(gameObject, _explosionDamage);
                _characterCamera.ShakeCamera(30f, .75f);
            }
        }
        // Destroy the explosion after a duration.
        Destroy(gameObject, _explosionDuration);
    }
}
