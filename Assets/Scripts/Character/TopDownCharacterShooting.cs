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
    private Vector3 _aimDirection;

    [Header("Core")]
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

    [Header("Animation Settings")]
    public float aimDuration = 5.0f;
    public float aimSpeed = 10.0f;
    public float holsterSpeed = 2.5f;
    private float _elaspedAimTime = 0.0f;
    private bool _isAimed = false;
    private float _aimWeight = 0.0f;
    private GameObject _model;
    private Animator _animator;

    private void Start()
    {
        // TODO: TEMPORARY
        currentWeapon.damage = damage;
        currentWeapon.interval = interval;
        currentWeapon.maxDistance = maxDistance;

        // Core
        _ignoreMask = ~(1 << playerLayer | 1 << enemyLayer);

        // Animations
        _model = transform.GetChild(0).gameObject;
        _animator = _model.GetComponent<Animator>();

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

        // Shooting
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            _elaspedAimTime = 0;
            _isAimed = true;

            Shoot();
            _gunSfx.Play();
            StartCoroutine("GunVFX");
        }

        // Laser Sights
        if (Input.GetKey(KeyCode.Mouse1))
        {
            _elaspedAimTime = 0;
            _isAimed = true;
            EnableLaser();
        }
        else
        {
            _laserLine.enabled = false;
        }

        AnimateAiming();
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

    private void AnimateAiming()
    {
        if (_elaspedAimTime >= aimDuration) {
            _isAimed = false;
        }

        if (_isAimed)
        {
            _elaspedAimTime += Time.deltaTime;
            _aimWeight = Mathf.Lerp(_aimWeight, 1, Time.deltaTime * aimSpeed);
        }
        else
        {
            _elaspedAimTime = 0;
            _aimWeight = Mathf.Lerp(_aimWeight, 0, Time.deltaTime * holsterSpeed);
        }

        // ANIMATION LAYERS: Base: 0, Aiming: 1
        _animator.SetLayerWeight(1, _aimWeight);
    }

    private IEnumerator GunVFX()
    {
        _gunLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _gunLight.enabled = false;
    }
}