using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyProjectile : NetworkBehaviour
{
    [SerializeField] private float _speed;
    // Position for the projectile to move towards.
    private Vector3 _targetPosition;
    private const float CollisionRadius = 0.5f;
    // Prefab for explosion that will be spawned once projectile is ready to explode.
    [SerializeField] private GameObject _explosionPrefab;


    // Update is called once per frame
    void Update()
    {
        // Checks if sphere is colliding with anything. Used in lieu of SphereCollider because SphereCollider doesn't work here.
        Collider[] collidersInRadius = Physics.OverlapSphere(transform.position, CollisionRadius);
        bool collidedWithSomething = collidersInRadius.Length > 0;
        // Projectile explodes if it reaches its target destination or it collides with something.
        if (Vector3.Distance(transform.position, _targetPosition) <= 1 || collidedWithSomething)
        {
            Explode();
        }
        // Move towards the target by a step value every frame.
        float step = _speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, _targetPosition, step);
    }

    // Required initialization for projectile to have a target to move towards.
    public void InitializeWithTargetPos(Vector3 pos)
    {
        _targetPosition = pos;
    }

    // Create an explosion at the location of the projectile and destroy the projectile.
    private void Explode()
    {
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
