using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class EnergyProjectile : NetworkBehaviour
{
    [SerializeField] private float _speed;
    private Vector3 _targetPosition;
    private const float CollisionRadius = 0.5f;
    [SerializeField] private GameObject _explosionPrefab;

    private void Update()
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
    public void SetTargetPosition(Vector3 pos)
    {
        _targetPosition = pos;
    }

    private void Explode()
    {
        Instantiate(_explosionPrefab, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }
}
