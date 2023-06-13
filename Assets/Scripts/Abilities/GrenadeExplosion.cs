using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class Grenade : NetworkBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;
    private const float _detonationTime = 3f;

    private void Start()
    {
        StartCoroutine(Detonate());
    }
    [ClientRpc]
    private void ExplodeClientRpc(){
        ExplodeInner();
    }
    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void ExplodeServerRpc(){
        ExplodeClientRpc();
    }
    void ExplodeInner(){
        if (_explosionPrefab != null && gameObject != null)
        {
            // Instantiate the explosion at the grenade's position.
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }
    }

    private IEnumerator Detonate()
    {
        yield return new WaitForSeconds(_detonationTime);
        ExplodeServerRpc();
        // Destroy(gameObject);  // Destroy the grenade.
        GlobalNetworkManager.Instance.DespawnGameObjectServerRpc(gameObject.GetComponent<NetworkObject>().NetworkObjectId);
    }
}