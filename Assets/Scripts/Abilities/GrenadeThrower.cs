using UnityEngine;
using Unity.Netcode;
using SharedMath;

public class GrenadeThrower : NetworkBehaviour
{
    public GameObject GrenadePrefab;
    public Animator PlayerAnimator;
    public float MaxGrenadeHeight = 5.0f;
    public int GrenadeCount = 5;
    public int MaxGrenades = 10;
    public float GrenadeThrowCooldown = 3f; // Cooldown time in seconds
    private float lastGrenadeThrowTime;
    private InputListener _inputListener;

    private void Start()
    {
        if (!IsOwner) return;

        _inputListener = GetComponent<InputListener>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_inputListener.GrenadeKey && Time.time >= lastGrenadeThrowTime + GrenadeThrowCooldown && GrenadeCount > 0)
        {
            ThrowGrenade();
            lastGrenadeThrowTime = Time.time;
            GrenadeCount--;
        }
    }

    private void ThrowGrenade()
    {
        Vector3 cursorPosition = _inputListener.CursorWorldPosition();

        // Calculate trajectory
        LaunchData launchData = Trajectory.CalculateLaunchData(transform.position, cursorPosition, MaxGrenadeHeight);
        GameObject grenade = Instantiate(GrenadePrefab, transform.position, Quaternion.identity);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = launchData.InitalVelocity;
    }
}