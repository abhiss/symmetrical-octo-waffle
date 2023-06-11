using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using Unity.Netcode;
using Shared;

public class EnemyBullet : MonoBehaviour
{
    private float _speed = 1f;
    private float _damage = 1f;
    private Vector3 _direction;
    private const float COLLISION_RADIUS = 0.5f;
    [SerializeField] private GameObject _explosionEffect;

    private void Update()
    {
        // move one direction
        transform.position += _direction * _speed * Time.deltaTime;
       
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.gameObject.GetComponent<HealthSystem>();
            playerHealth.TakeDamage(gameObject,_damage);

        }
        if (!other.gameObject.CompareTag("Enemy") && !other.gameObject.CompareTag("EnemyProjectile"))
        {
            // Destroy(gameObject);
            // Initialized(transform.position,_explosionEffect)
            Explode();
        }
        
    }
    private void Explode()
    {
        Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    public void SetParameter(Vector3 direction, float speed, float damage)
    {
        _direction = direction;
        _damage = damage;
        _speed = speed;
    }
}