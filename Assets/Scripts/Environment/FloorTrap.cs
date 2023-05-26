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
        if (other.CompareTag("Player"))
        {
            GameObject player = other.gameObject;
            TriggerTrapOnPlayer(player);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        _audio.Play();
    }

    private void OnTriggerExit(Collider other)
    {
        _audio.Stop();
    }

    private void TriggerTrapOnPlayer(GameObject player)
    {
        HealthSystem health = player.GetComponent<HealthSystem>();
        health.TakeDamage(gameObject, _damage);
    }
}