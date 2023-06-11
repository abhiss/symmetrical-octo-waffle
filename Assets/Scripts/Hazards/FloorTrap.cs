using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class FloorTrap : NetworkBehaviour
{
    [SerializeField] private float _damage = 1f;
    [SerializeField] private LayerMask _targetMask;
    private AudioSource _audio;

    private void Start()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider other)
    {
        bool colliderInLayerMask =  ((1 << other.gameObject.layer) & _targetMask) != 0;
        // If the collider belongs to the player, deal damage to them.
        if (colliderInLayerMask)
        {
            GameObject player = other.gameObject;
            HealthSystem health = player.GetComponent<HealthSystem>();
            health.TakeDamage(gameObject, _damage);
            if (!_audio.isPlaying)
            {
                _audio.Play();
            }
        }
    }

    private void OnTriggerExit()
    {
        _audio.Stop();
    }
}