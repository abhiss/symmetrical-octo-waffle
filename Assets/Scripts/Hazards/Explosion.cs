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
<<<<<<< HEAD

    [Header("Gizmos")]
    [SerializeField] private bool _showExplosionRadius;
=======
>>>>>>> 1365cd3988dcca969b69ce6ed043bc5361f381c7

    private void Start()
    {
        _explosionAudio = GetComponent<AudioSource>();
<<<<<<< HEAD
=======
        _characterCamera = Camera.main.transform.GetComponent<CharacterCamera>();

        // Explode as soon as the explosion is instantiated.
>>>>>>> 1365cd3988dcca969b69ce6ed043bc5361f381c7
        Explode();
    }

    private void Explode()
    {
        _explosionAudio.Play();
        // Deal damage to all objects in target mask within explosion radius.
        Collider[] collidersInExplosionRadius = Physics.OverlapSphere(transform.position, _explosionRadius, _targetMask);
        foreach (Collider collider in collidersInExplosionRadius)
        {
<<<<<<< HEAD
            HealthSystem targetHealth = collider.gameObject.GetComponent<HealthSystem>();
            targetHealth.TakeDamage(gameObject, _explosionDamage);
=======
            // Can be succinctly converted to a layer mask if more interactions become possible.
            bool isValidTarget = collider.CompareTag("Player") || collider.CompareTag("Enemy") || collider.CompareTag("Destructible");
            // If a collider is a valid target, deal damage to its health.
            if (isValidTarget)
            {
                HealthSystem targetHealth = collider.gameObject.GetComponent<HealthSystem>();
                targetHealth.TakeDamage(gameObject, _explosionDamage);
                _characterCamera.ShakeCamera(30f, .75f);
            }
>>>>>>> 1365cd3988dcca969b69ce6ed043bc5361f381c7
        }
        Destroy(gameObject, _explosionDuration);
    }

    private void OnDrawGizmos()
    {
        if (_showExplosionRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, _explosionRadius);
        }
    }
}
