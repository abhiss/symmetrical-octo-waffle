using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class FloorTrap : NetworkBehaviour
{
    [SerializeField] private float _damage = 1f;
    private AudioSource _audio;

    private void Awake()
    {
        _audio = GetComponent<AudioSource>();
    }

    private void OnTriggerStay(Collider other)
    {
        // If the collider belongs to the player, deal damage to them.
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            HealthSystem health = player.GetComponent<HealthSystem>();
            health.TakeDamage(gameObject, _damage);
        }
    }

    // Play a zapping sound once the player enters the trap.
    private void OnTriggerEnter(Collider other)
    {
        _audio.Play();
    }

    // Stop playing a zapping sound once the player exits the trap.
    private void OnTriggerExit(Collider other)
    {
        _audio.Stop();
    }
}