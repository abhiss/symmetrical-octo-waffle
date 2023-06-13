using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class Explosion : NetworkBehaviour
{
    [SerializeField] private float _explosionDamage;
    [SerializeField] private float _explosionRadius;
    private const float _explosionDuration = 2f;
    [SerializeField] private LayerMask _targetMask;
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
        _explosionAudio.Play();
        // Deal damage to all objects in target mask within explosion radius.
        Collider[] collidersInExplosionRadius = Physics.OverlapSphere(transform.position, _explosionRadius, _targetMask);
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
        Destroy(gameObject, _explosionDuration);
    }

    private void OnDrawGizmos()
    {
        // if (_showExplosionRadius)
        // {
        //     Gizmos.color = Color.red;
        //     Gizmos.DrawSphere(transform.position, _explosionRadius);
        // }
    }
}
