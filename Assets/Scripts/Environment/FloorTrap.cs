using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class FloorTrap : MonoBehaviour
{
    [SerializeField] private float _damage = 2f;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Triggered floor trap collider");
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            GameObject player = other.gameObject;
            TriggerTrapOnPlayer(player);
        }
    }

    private void TriggerTrapOnPlayer(GameObject player)
    {
        HealthSystem health = player.GetComponent<HealthSystem>();
        health.TakeDamage(gameObject, _damage);
    }
}
