using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Shared;

public class Grenade : MonoBehaviour
{
    [SerializeField] private GameObject _explosionPrefab;
    private const float _detonationTime = 3f;

    private void Start()
    {
        StartCoroutine(Detonate());
    }

    private IEnumerator Detonate()
    {
        yield return new WaitForSeconds(_detonationTime);

        if (_explosionPrefab != null)
        {
            // Instantiate the explosion at the grenade's position.
            Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        }

        Destroy(gameObject);  // Destroy the grenade.
    }
}