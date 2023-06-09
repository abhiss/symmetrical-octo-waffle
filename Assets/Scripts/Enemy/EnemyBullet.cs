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
    //[SerializeField] private GameObject _explosionEffect;

    private void Update()
    {
        // move one direction
        transform.position += _direction * _speed * Time.deltaTime;
       
    }

    void OnTriggerStay(Collider other)
    {
        Debug.Log("here");
        if (other.gameObject.CompareTag("Player"))
        {
            HealthSystem playerHealth = other.gameObject.GetComponent<HealthSystem>();
            playerHealth.TakeDamage(gameObject,_damage);

        }
        Destroy(gameObject);
    }

    public void SetParameter(Vector3 direction, float speed, float damage)
    {
        _direction = direction;
        _damage = damage;
        _speed = speed;
    }
    // Create an explosion at the location of the projectile and destroy the projectile.
    // private void Explode()
    // {
    //     Instantiate(_explosionEffect, transform.position, Quaternion.identity);
    //     Destroy(gameObject);
    // }
}
 // Checks if sphere is colliding with anything. Used in lieu of SphereCollider because SphereCollider doesn't work here.
        // Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, COLLISION_RADIUS);
        // bool collided = (collidersInRadius.Length > 0);
        // // Projectile explodes if it reaches its target destination or it collides with something.
        // if(collided )
        // {
            
        //     Collider objCollider = collidersInRadius[0];
        //     GameObject target = objCollider.gameObject;
            
        //     if(!(objCollider.CompareTag("Enemy")))
        //     {
        //         Debug.Log("collide");
        //         Destroy(gameObject);
        //     }
                     
        // }