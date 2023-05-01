using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: TEMPORARY: Should be a scriptable object
public struct PlayerWeapon {
    public float damage;
    public float interval;
    public float maxDistance;
}

public class TopDownCharacterShooting : MonoBehaviour
{
    [Header("TEMP: Weapon Settings")]
    public float damage = 10.0f;
    public float interval = 0.5f;
    public float maxDistance = 100.0f;
    PlayerWeapon currentWeapon;

    [Header("Laser Sights Settings")]
    public Material laserMaterial;
    public Transform gunNozzle;
    private LineRenderer _laserLine;

    [Header("Animation Settings")]
    public float aimSpeed = 10.0f;
    public float holsterSpeed = 2.5f;
    private bool _isAimed = false;
    private float _aimWeight = 0.0f;
    private GameObject _modelObject;
    private Animator _animator;

    private void Start()
    {
        // TODO: TEMPORARY
        currentWeapon.damage = damage;
        currentWeapon.interval = interval;
        currentWeapon.maxDistance = maxDistance;

        // Animations
        _modelObject = transform.GetChild(0).gameObject;
        _animator = _modelObject.GetComponent<Animator>();

        // Laser
        _laserLine = gameObject.AddComponent<LineRenderer>();
        _laserLine.material = laserMaterial;
        _laserLine.startWidth = 0.05f;
        _laserLine.endWidth = 0.05f;
        _laserLine.enabled = false;
    }

    private void Update()
    {
        // Shooting
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _isAimed = true;
            // TODO: Play shoot animation and sound
            Shoot();
        }

        // Aiming
        if (Input.GetKey(KeyCode.Mouse1))
        {
            EnableLaser();
            _isAimed = true;
        }
        else
        {
            _laserLine.enabled = false;
        }

        AnimateAiming();
    }

    private void Shoot()
    {
        Vector3 direction = transform.forward;
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, currentWeapon.maxDistance))
        {
            // Raycast hit something
            Debug.Log("Hit: " + hit.collider.gameObject.name);

            HealthSystem healthSystem = hit.collider.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                // Deal damage to the object with HealthSystem component
                healthSystem.TakeDamage(currentWeapon.damage);
            }
        }
    }

    private void EnableLaser()
    {
        Vector3 origin = gunNozzle.position;
        Vector3 direction = transform.forward;
        Vector3 laserEndPoint = origin + direction * currentWeapon.maxDistance;

        _laserLine.SetPosition(0, origin);
        if (Physics.Raycast(origin, direction, out RaycastHit hit, currentWeapon.maxDistance))
        {
            laserEndPoint = hit.point;
        }

        _laserLine.SetPosition(1, laserEndPoint);
        _laserLine.enabled = true;
    }

    private void AnimateAiming()
    {
        if (_isAimed)
        {
            _aimWeight = Mathf.Lerp(_aimWeight, 1, Time.deltaTime * aimSpeed);
        }
        else
        {
            _aimWeight = Mathf.Lerp(_aimWeight, 0, Time.deltaTime * holsterSpeed);
        }

        // ANIMATION LAYERS: Base: 0, Aiming: 1
        _animator.SetLayerWeight(1, _aimWeight);
    }
}