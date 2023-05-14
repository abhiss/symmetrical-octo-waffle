using System.Collections;
using UnityEngine;

public class TopDownCharacterShooting : MonoBehaviour
{
    public WeaponManager CurrentWeapon;

    [Header("Properties")]
    public bool IsAiming = false;
    public float AimDeadZone = 3.5f;
    public LayerMask PlayerMask;
    public LayerMask EnemyMask;
    private Vector3 _aimDirection;
    private Ray _cameraRay;
    private bool _canShoot = true;

    [Header("Aim Assist")]
    public bool EnableAimAssist = true;
    public float AssistRaidus = 0.5f;

    [Header("Audio and Visuals")]
    public GameObject SfxVfx;
    private Light _gunLight;
    private AudioSource _gunSfx;

    [Header("Laser")]
    public Material LaserMaterial;
    public float LaserWidth = 0.5f;
    private LineRenderer _laserLine;

    private void Start()
    {
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
        // Calculation direction
        Vector3 mousePosition = Input.mousePosition;
        _cameraRay = Camera.main.ScreenPointToRay(mousePosition);
        _aimDirection = CalculateDirection();

        // Shooting
        IsAiming = false;

        // Automatic or single fire
        bool inputFire = Input.GetKeyDown(KeyCode.Mouse0);
        if (CurrentWeapon.Interval > 0.0f) {
            inputFire = Input.GetKey(KeyCode.Mouse0);
        }

        if (inputFire && _canShoot)
        {
            StartCoroutine(AutomaticFire());
            IsAiming = true;
            Shoot();
        }

        // Laser Sights
        if (Input.GetKey(KeyCode.Mouse1))
        {
            IsAiming = true;
            EnableLaser();
        }
        else
        {
            _laserLine.enabled = false;
        }
    }

    private void Shoot()
    {
        StartCoroutine(GunVFX());
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
    }

    private Vector3 AdjustCursorPostion()
    {
        Vector3 newPosition = transform.position + transform.forward;

        // Adjust to the floor
        LayerMask ignoreMask = EnemyMask | PlayerMask;
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
        float deadZoneDist = Vector3.Distance(newDirection, transform.position);
        if (deadZoneDist <= AimDeadZone)
        {
            newDirection = transform.forward;
        }
        else if (EnableAimAssist)
        {
            // Aim assist (target closest enemy to cursor)
            float closestDistance = Mathf.Infinity;
            Vector3 closestPosition = cursorPosition;
            Collider[] hitColliders = Physics.OverlapSphere(cursorPosition, AssistRaidus, EnemyMask);
            foreach (var hitCollider in hitColliders)
            {
                Vector3 hitPosiiton = hitCollider.transform.position;
                float dist = Vector3.Distance(cursorPosition, hitPosiiton);
                if (dist <= closestDistance)
                {
                    closestPosition = hitPosiiton;
                }
            }

            newDirection = closestPosition - transform.position;
        }

        return newDirection.normalized;
    }

    private void EnableLaser()
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
        _laserLine.enabled = true;
    }

    private IEnumerator GunVFX()
    {
        _gunSfx.Play();
        _gunLight.enabled = true;
        yield return new WaitForSeconds(0.05f);
        _gunLight.enabled = false;
    }

    private IEnumerator AutomaticFire()
    {
        _canShoot = false;
        yield return new WaitForSeconds(CurrentWeapon.Interval);
        _canShoot = true;
    }
}