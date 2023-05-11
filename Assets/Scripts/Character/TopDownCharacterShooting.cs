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
    private Vector3 _aimDirection;
    public int playerLayer = 6;
    public int enemyLayer = 7;
    private LayerMask _ignoreMask;

    [Header("Audio and Visuals")]
    public GameObject sfxVfx;
    public Material laserMaterial;
    public Transform gunNozzle;
    public float laserWidth = 0.5f;
    private LineRenderer _laserLine;
    private Light _gunLight;
    private AudioSource _gunSfx;

    private void Start()
    {
        // TODO: TEMPORARY
        currentWeapon.damage = damage;
        currentWeapon.interval = interval;
        currentWeapon.maxDistance = maxDistance;

        // Core
        _ignoreMask = ~(1 << playerLayer | 1 << enemyLayer);

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
        Vector3 cursorPosition = AdjustCursorPostion(Input.mousePosition);
        float dist = Vector3.Distance(_aimDirection, transform.position);
        _aimDirection = cursorPosition - transform.position;
        if (dist <= 3.5f)
        {
            _aimDirection = transform.forward;
        }

        _aimDirection = _aimDirection.normalized;
        isAiming = false;

        // Shooting
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            isAiming = true;
            Shoot();
            _gunSfx.Play();
            StartCoroutine("GunVFX");
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
        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, currentWeapon.maxDistance))
        {
            // TODO: Flavor Section
            // - Create particle at hit point for debrie or sparks
            // End of flavor Section
            if (hit.collider.gameObject.layer != playerLayer) {
                return;
            }

            // Hit an enemy
            Shared.HealthSystem healthSystem = hit.collider.GetComponent<Shared.HealthSystem>();
            if (healthSystem != null)
            {
                // Deal damage to the object with HealthSystem component
                healthSystem.TakeDamage(gameObject, currentWeapon.damage);
            }
        }
    }

    private Vector3 AdjustCursorPostion(Vector3 cursorPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(cursorPosition);
        Vector3 newPosition = transform.position + transform.forward;

        // Adjust to the floor
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, _ignoreMask))
        {
            newPosition = hit.point;
            newPosition.y += 1.0f; // Player Half Height
        }

        return newPosition;
    }

    private void EnableLaser()
    {
        Vector3 laserEndPoint = transform.position + _aimDirection * currentWeapon.maxDistance;

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
        _gunLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _gunLight.enabled = false;
    }
}