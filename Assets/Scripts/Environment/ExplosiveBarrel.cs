using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class ExplosiveBarrel : NetworkBehaviour
{
    private HealthSystem _health;
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
            CreateExplosion();
            Destroy(gameObject);
        }
    }

    private GameObject CreateExplosion()
    {
        // Instantiate an explosion based off an explosion prefab at the location of the barrel.
        GameObject explosion = Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        return explosion;
    }
}
