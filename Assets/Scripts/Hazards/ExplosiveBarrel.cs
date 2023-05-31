using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class ExplosiveBarrel : NetworkBehaviour
{
    private HealthSystem _health;
    // Prefab for explosion that will be spawned on turret death.
    [SerializeField] private GameObject _explosionPrefab;

    void Awake()
    {
        _health = GetComponent<HealthSystem>();
    }

    void Update()
    {
        // If the barrel is dead, cause it to explode.
        if (_health.CurrentHealth <= 0)
        {
            // Create an explosion at the position of the barrel and destroy the barrel.
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}
