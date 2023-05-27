using UnityEngine;
using System;

public class CharacterShooting : MonoBehaviour
{
    public WeaponCreator CurrentWeapon;
    public Transform WeaponDrawer;
    private InputListener _inputListener;

    [Header("Laser")]
    public Material LaserMaterial;
    public float LaserWidth = 0.5f;
    private LineRenderer _laserLine;

    [Header("Conditions")]
    [NonSerialized] public bool IsReloading = false;
    [NonSerialized] public bool IsAiming = false;

    [Header("Properties")]
    public float CursorDeadZone = 3.5f;
    public LayerMask PlayerMask;
    public LayerMask EnemyMask;
    private Vector3 _aimDirection;
    private int _newClip = 0;
    private bool _fireEnabled = true;

    [Header("Aim Assist")]
    public bool EnableAimAssist = true;
    public float AssistRaidus = 0.5f;

    [Header("Audio and Visuals")]
    public float VfxInterval = 0.09f;
    private Transform weaponTransform;
    private AudioSource _playerAudioSrc;
    private Light _muzzleFlash;

    [Header("Cooldowns")]
    private float _fireRateCoolDown = 0.0f;
    private float _vfxCoolDown = 0.0f;
    private float _reloadDuration = 0.0f;

    [Header("Debugging")]
    public bool EnableDebugging = true;

    private void Start()
    {
        // Weapon Init
        CurrentWeapon.CurrentClipSize = CurrentWeapon.MaxClipSize;
        CurrentWeapon.CurrentAmmo = CurrentWeapon.MaxAmmo - CurrentWeapon.MaxClipSize;
        SetActiveWeapon(CurrentWeapon.WeaponName);

        // Input
        _inputListener = GetComponent<InputListener>();

        // Laser
        _laserLine = gameObject.AddComponent<LineRenderer>();
        _laserLine.material = LaserMaterial;
        _laserLine.startWidth = LaserWidth;
        _laserLine.endWidth = LaserWidth;
        _laserLine.enabled = false;

        _playerAudioSrc = GetComponent<AudioSource>();
    }

    private void Update()
    {
        Vector3 cursorPosition = AdjustCursorPostion(Input.mousePosition);
        _aimDirection = GetAimDirection(cursorPosition);
        InputEvents();

        if (EnableDebugging)
        {
            DebugMode();
        }

        _muzzleFlash.enabled = _vfxCoolDown > 0;
        DrawLaser();
        GameplayTimers();
    }

    private void InputEvents()
    {
        _fireEnabled = _fireRateCoolDown <= 0 && _reloadDuration <= 0 && CurrentWeapon.CurrentClipSize > 0;

        // Reloading
        bool reloadConditions = !IsReloading && CurrentWeapon.CurrentClipSize != CurrentWeapon.MaxClipSize;
        if (_inputListener.ReloadKey && reloadConditions)
        {
            Reload();
        }

        // Automatic or single fire
        bool inputFire = _inputListener.FireKeyDown;
        if (CurrentWeapon.FireRate > 0.0f)
        {
            inputFire = _inputListener.FireKey;
        }

        // Shooting
        if (inputFire && _fireEnabled)
        {
            Fire();
        }

        _laserLine.enabled = _inputListener.AltFire;

        // Conditions
        IsReloading = _reloadDuration > 0;
        IsAiming = inputFire || _inputListener.AltFire;
    }

    // Gameplay
    // -------------------------------------------------------------------------
    private void Fire()
    {
        // VFX
        _playerAudioSrc.PlayOneShot(CurrentWeapon.FireSound);
        _vfxCoolDown = VfxInterval;

        // Gameplay
        _fireRateCoolDown = CurrentWeapon.FireRate;
        --CurrentWeapon.CurrentClipSize;

        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, CurrentWeapon.MaxDistance))
        {
            // TODO: Flavor Section
            // - Create particle at hit point for debrie or sparks
            // End of flavor Section

            // Hit an enemy
            Shared.HealthSystem healthSystem = hit.collider.GetComponent<Shared.HealthSystem>();
            if (healthSystem == null)
            {
                return;
            }

            // Deal damage to the object with HealthSystem component
            healthSystem.TakeDamage(gameObject, CurrentWeapon.Damage);
        }
    }

    private void Reload()
    {
        if (CurrentWeapon.CurrentAmmo <= 0)
        {
            _playerAudioSrc.PlayOneShot(CurrentWeapon.NoAmmoSound);
            return;
        }

        _playerAudioSrc.PlayOneShot(CurrentWeapon.ReloadSound);

        // Remove ammo from ammo pool
        int currentBulletAmount = CurrentWeapon.CurrentAmmo + CurrentWeapon.CurrentClipSize;
        int newBulletAmount = currentBulletAmount - CurrentWeapon.MaxClipSize;
        if (newBulletAmount < 0)
        {
            newBulletAmount = 0;
        }

        CurrentWeapon.CurrentClipSize = 0;
        CurrentWeapon.CurrentAmmo = newBulletAmount;

        _newClip = Mathf.Min(CurrentWeapon.MaxClipSize, currentBulletAmount);
        _reloadDuration = CurrentWeapon.ReloadTime;
    }

    private void GameplayTimers()
    {
        // Gameplay
        if (_fireRateCoolDown > 0)
        {
            _fireRateCoolDown -= Time.deltaTime;
        }

        if (_reloadDuration > 0)
        {
            _reloadDuration -= Time.deltaTime;
        }

        // Apply new clip
        if (_reloadDuration <= 0 && _newClip > 0)
        {
            CurrentWeapon.CurrentClipSize = _newClip;
            _newClip = 0;
        }

        // Misc
        if (_vfxCoolDown > 0)
        {
            _vfxCoolDown -= Time.deltaTime;
        }
    }

    // Aim directional Logic
    // -------------------------------------------------------------------------
    private Vector3 AdjustCursorPostion(Vector3 mousePosition)
    {
        Vector3 newPosition = transform.position + transform.forward;
        LayerMask ignoreMask = EnemyMask | PlayerMask;
        Ray cameraRay = Camera.main.ScreenPointToRay(mousePosition);

        // Adjust to the floor
        if (Physics.Raycast(cameraRay, out RaycastHit hit, Mathf.Infinity, ~ignoreMask))
        {
            newPosition = hit.point;
            newPosition.y += 1.0f; // Player Half Height
        }

        return newPosition;
    }

    private Vector3 GetAimDirection(Vector3 aimPosition)
    {
        Vector3 newDirection = aimPosition - transform.position;

        // Dead zone
        float deadZoneDist = Vector3.Distance(aimPosition, transform.position);
        if (deadZoneDist <= CursorDeadZone)
        {
            newDirection = transform.forward;
        }
        else if (EnableAimAssist)
        {
            // Aim assist (target closest enemy to cursor)
            float closestDistance = Mathf.Infinity;
            Vector3 closestPosition = aimPosition;
            Collider[] hitColliders = Physics.OverlapSphere(aimPosition, AssistRaidus, EnemyMask);
            foreach (var hitCollider in hitColliders)
            {
                Vector3 hitPosiiton = hitCollider.transform.position;
                float dist = Vector3.Distance(aimPosition, hitPosiiton);
                if (dist <= closestDistance)
                {
                    closestPosition = hitPosiiton;
                }
            }

            newDirection = closestPosition - transform.position;
        }

        return newDirection.normalized;
    }

    // Misc
    // -------------------------------------------------------------------------
    private void DrawLaser()
    {
        Vector3 laserEndPoint = transform.position;
        laserEndPoint += _aimDirection * CurrentWeapon.MaxDistance;

        // Shrink laser to wall hit
        _laserLine.SetPosition(0, transform.position);
        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, CurrentWeapon.MaxDistance))
        {
            laserEndPoint = hit.point;
        }

        _laserLine.SetPosition(1, laserEndPoint);
    }

    private void SetActiveWeapon(string weaponName)
    {
        if (weaponTransform != null)
        {
            weaponTransform.gameObject.SetActive(false);
        }

        // Needs to be like this with current setup
        Transform newWeapon = WeaponDrawer.Find(weaponName);
        newWeapon.gameObject.SetActive(true);
        weaponTransform = newWeapon;

        _muzzleFlash = newWeapon.Find("MuzzleLocation").GetComponent<Light>();
    }

    // Debugging
    // -------------------------------------------------------------------------
    private void DebugMode()
    {
        Debug.Log($"{CurrentWeapon.CurrentClipSize} / {CurrentWeapon.CurrentAmmo}");
    }
}