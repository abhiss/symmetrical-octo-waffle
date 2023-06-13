using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class ExplosiveBarrel : NetworkBehaviour
{
    private HealthSystem _health;
    [SerializeField] private GameObject _explosionPrefab;

    private void Start()
    {
        _health = GetComponent<HealthSystem>();
    }

    [ClientRpc]
    void ExplodeClientRpc(){
        ExplodeInner();
    }
    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    void ExplodeServerRpc(){
        ExplodeClientRpc();
    }
    void ExplodeInner(){
        if(gameObject is null) {
            Debug.Log("Explosive barrel was null before exploding.");
        }
        // Create an explosion at the position of the barrel and destroy the barrel.
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
    private void Update()
    {
        // If the barrel is dead, cause it to explode.
        if (_health.CurrentHealth <= 0)
        {
            ExplodeServerRpc();
            ExplodeInner();
        }
    }
}
