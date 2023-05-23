using System.Collections;
using UnityEngine;

public class CharacterShooting : MonoBehaviour
{
    public WeaponManager CurrentWeapon;
    public bool IsReloading = false;
    private InputListener _inputListener;

    [Header("Properties")]
    public bool IsAiming = false;
    public float CursorDeadZone = 3.5f;
    public LayerMask PlayerMask;
    public LayerMask EnemyMask;
    private Vector3 _aimDirection;
    private int _newClip = 0;
    private bool _shootEnabled = true;

    [Header("Aim Assist")]
    public bool EnableAimAssist = true;
    public float AssistRaidus = 0.5f;

    [Header("Audio and Visuals")]
    public GameObject SfxVfx;
    public float VfxInterval = 0.09f;
    private Light _gunLight;
    private AudioSource _gunSfx;

    [Header("Laser")]
    public Material LaserMaterial;
    public float LaserWidth = 0.5f;
    private LineRenderer _laserLine;

    [Header("Cooldowns")]
    private float _fireRateCoolDown = 0.0f;
    private float _vfxCoolDown = 0.0f;
    private float _reloadDuration = 0.0f;

    private void Start()
    {
        // Input
        _inputListener = GetComponent<InputListener>();

        // Weapon Init
        CurrentWeapon.CurrentClipSize = CurrentWeapon.MaxClipSize;
        CurrentWeapon.CurrentAmmo = CurrentWeapon.MaxAmmo - CurrentWeapon.MaxClipSize;

        // Laser
        _laserLine = gameObject.AddComponent<LineRenderer>();
        _laserLine.material = LaserMaterial;
        _laserLine.startWidth = LaserWidth;
        _laserLine.endWidth = LaserWidth;
        _laserLine.enabled = false;

        // SFX / VFX
        _gunLight = SfxVfx.GetComponent<Light>();
        _gunLight.enabled = false;
        _gunSfx = SfxVfx.GetComponent<AudioSource>();
    }

    private void Update()
    {
        Vector3 cursorPosition = AdjustCursorPostion(Input.mousePosition);
        _aimDirection = GetAimDirection(cursorPosition);

        InputEvents();

        if (_shootEnabled)
        {
            Debug.Log($"{CurrentWeapon.CurrentClipSize} / {CurrentWeapon.CurrentAmmo}");
        }
        else if (IsReloading)
        {
            Debug.Log("RELOADING");
        }

        // Gameplay
        _shootEnabled = _fireRateCoolDown <= 0 && _reloadDuration <= 0;
        _fireRateCoolDown -= Time.deltaTime;
        _reloadDuration -= Time.deltaTime;

        if (_reloadDuration <= 0 && _newClip > 0)
        {
            CurrentWeapon.CurrentClipSize = _newClip;
            _newClip = 0;
        }

        // Misc
        _vfxCoolDown -= Time.deltaTime;

        if (_reloadDuration <= 0)
        {
            _reloadDuration = 0;
        }

        if (_fireRateCoolDown <= 0)
        {
            _fireRateCoolDown = 0;
        }

        if (_reloadDuration <= 0)
        {
            _reloadDuration = 0;
        }

        if (_vfxCoolDown <= 0)
        {
            _vfxCoolDown = 0;
            _gunLight.enabled = false;
        }
    }

    private void InputEvents()
    {
        // Reloading
        bool reloadConditions = !IsReloading && CurrentWeapon.CurrentClipSize != CurrentWeapon.MaxClipSize;
        if (_inputListener.ReloadKey && reloadConditions)
        {
            if (CurrentWeapon.CurrentAmmo <= 0)
            {
                Debug.Log("NO AMMO");
                return;
            }

            int currentBulletAmount = CurrentWeapon.CurrentAmmo + CurrentWeapon.CurrentClipSize;
            int newBulletAmount = currentBulletAmount - CurrentWeapon.MaxClipSize;
            if (newBulletAmount < 0) {
                newBulletAmount = 0;
            }

            // Update clip
            _newClip = Mathf.Min(CurrentWeapon.MaxClipSize, currentBulletAmount);
            CurrentWeapon.CurrentClipSize = 0;
            CurrentWeapon.CurrentAmmo = newBulletAmount;

            _reloadDuration = CurrentWeapon.ReloadTime;
        }

        // Automatic or single fire
        bool inputFire = _inputListener.FireKeyDown;
        if (CurrentWeapon.FireRate > 0.0f)
        {
            inputFire = _inputListener.FireKey;
        }

        // Shooting
        bool shootingConditions =  _shootEnabled && CurrentWeapon.CurrentClipSize > 0;
        if (inputFire && shootingConditions)
        {
            Shoot();
        }

        IsReloading = _reloadDuration > 0;
        IsAiming = inputFire && shootingConditions || _inputListener.AltFire;

        // Laser Sights
        DrawLaser();
        _laserLine.enabled = _inputListener.AltFire;
    }

    // Gameplay
    // -------------------------------------------------------------------------
    private void Shoot()
    {
        if (Physics.Raycast(transform.position, _aimDirection, out RaycastHit hit, CurrentWeapon.MaxDistance))
        {
            // TODO: Flavor Section
            // - Create particle at hit point for debrie or sparks
            // End of flavor Section

            // Hit an enemy
            Shared.HealthSystem healthSystem = hit.collider.GetComponent<Shared.HealthSystem>();
            if (healthSystem != null)
            {
                // Deal damage to the object with HealthSystem component
                healthSystem.TakeDamage(gameObject, CurrentWeapon.Damage);
            }
        }

        // Gameplay
        _fireRateCoolDown = CurrentWeapon.FireRate;
        --CurrentWeapon.CurrentClipSize;

        // VFX
        _gunSfx.Play();
        if (_gunLight.enabled == false)
        {
            _vfxCoolDown = VfxInterval;
            _gunLight.enabled = true;
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
}