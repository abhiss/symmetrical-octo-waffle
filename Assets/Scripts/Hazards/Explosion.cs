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

    [Header("Gizmos")]
    [SerializeField] private bool _showExplosionRadius;

    private void Start()
    {
        _explosionAudio = GetComponent<AudioSource>();
        Explode();
    }

    private void Explode()
    {
        _explosionAudio.Play();
        // Deal damage to all objects in target mask within explosion radius.
        Collider[] collidersInExplosionRadius = Physics.OverlapSphere(transform.position, _explosionRadius, _targetMask);
        foreach (Collider collider in collidersInExplosionRadius)
        {
            HealthSystem targetHealth = collider.gameObject.GetComponent<HealthSystem>();
            targetHealth.TakeDamage(gameObject, _explosionDamage);
            if (collider.gameObject.CompareTag("Player"))
            {
                Camera.main.GetComponent<CharacterCamera>().ShakeCamera(15.0f, 0.5f);
            }
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
