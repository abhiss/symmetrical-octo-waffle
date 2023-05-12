using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: TEMPORARY: Should be a scriptable object or structs
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

    [Header("Core")]
    public bool isAiming = false;
    public float aimDeadZone = 3.5f;
    public LayerMask enemyMask;
    public LayerMask playerMask;
    private Vector3 _aimDirection;
    private Ray _cameraRay;

    [Header("Aim Assist")]
    public bool enableAimAssist = true;
    public float assistRaidus = 0.5f;

    [Header("Audio and Visuals")]
    public GameObject sfxVfx;
    private Light _gunLight;
    private AudioSource _gunSfx;

    [Header("Laser")]
    public Material laserMaterial;
    public float laserWidth = 0.5f;
    private LineRenderer _laserLine;

    private void Start()
    {
        // TODO: TEMPORARY
        currentWeapon.damage = damage;
        currentWeapon.interval = interval;
        currentWeapon.maxDistance = maxDistance;

        // Laser
        _laserLine = gameObject.AddComponent<LineRenderer>();
        _laserLine.material = laserMaterial;
        _laserLine.startWidth = laserWidth;
        _laserLine.endWidth = laserWidth;
        _laserLine.enabled = false;

        // SFX / VFX
        _gunLight = sfxVfx.GetComponent<Light>();
        _gunLight.enabled = false;
        _gunSfx = sfxVfx.GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Calculation direction
        Vector3 mousePosition = Input.mousePosition;
        _cameraRay = Camera.main.ScreenPointToRay(mousePosition);
        _aimDirection = CalculateDirection();

        // Shooting
        isAiming = false;
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAiming = true;
            Shoot();
        }

        // Laser Sights
        if (Input.GetKey(KeyCode.Mouse1))
        {
            isAiming = true;
            EnableLaser();
        }
        else
        {
            _laserLine.enabled = false;
        }
    }

    private void Shoot()
    {
        StartCoroutine("GunVFX");
        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, currentWeapon.maxDistance))
        {
            // TODO: Flavor Section
            // - Create particle at hit point for debrie or sparks
            // End of flavor Section

            // Hit an enemy
            Shared.HealthSystem healthSystem = hit.collider.GetComponent<Shared.HealthSystem>();
            if (healthSystem != null)
            {
                // Deal damage to the object with HealthSystem component
                healthSystem.TakeDamage(gameObject, currentWeapon.damage);
            }
        }
    }

    private Vector3 AdjustCursorPostion()
    {
        Vector3 newPosition = transform.position + transform.forward;

        // Adjust to the floor
        LayerMask ignoreMask = enemyMask | playerMask;
        if (Physics.Raycast(_cameraRay, out RaycastHit hit, Mathf.Infinity, ~ignoreMask))
        {
            newPosition = hit.point;
            newPosition.y += 1.0f; // Player Half Height
        }

        return newPosition;
    }

    private Vector3 CalculateDirection()
    {
        Vector3 cursorPosition = AdjustCursorPostion();
        Vector3 newDirection = cursorPosition - transform.position;

        // Dead zone
        float dist = Vector3.Distance(newDirection, transform.position);
        if (dist <= aimDeadZone)
        {
            newDirection = transform.forward;
        }

        // Aim assist
        if (Physics.SphereCast(_cameraRay, assistRaidus, out RaycastHit hit, Mathf.Infinity, enemyMask) && enableAimAssist)
        {
            newDirection = hit.point - transform.position;
        }

        return newDirection.normalized;
    }

    private void EnableLaser()
    {
        Vector3 laserEndPoint = transform.position;
        laserEndPoint += _aimDirection * currentWeapon.maxDistance;

        // Shrink laser to wall hit
        _laserLine.SetPosition(0, transform.position);
        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, currentWeapon.maxDistance))
        {
            laserEndPoint = hit.point;
        }

        _laserLine.SetPosition(1, laserEndPoint);
        _laserLine.enabled = true;
    }

    private IEnumerator GunVFX()
    {
        _gunSfx.Play();
        _gunLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _gunLight.enabled = false;
    }
}