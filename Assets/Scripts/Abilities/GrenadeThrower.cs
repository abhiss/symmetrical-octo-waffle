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
    private bool _threwGrenade = true;
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
            // Calculate trajectory
            Vector3 cursorPosition = _inputListener.CursorWorldPosition();
            LaunchData launchData = Trajectory.CalculateLaunchData(transform.position, cursorPosition, MaxGrenadeHeight);
            throwGrenadeServerRpc(launchData.InitalVelocity);

            // Functional
            lastGrenadeThrowTime = Time.time;
            GrenadeCount--;

            // Animation
            _threwGrenade = true;
        }

        // Throw animation
        PlayerAnimator.SetBool("IsThrowingGrenade", _threwGrenade);

        if (_threwGrenade)
        {
            _threwGrenade = false;
        }
    }

    [ServerRpc]
    void throwGrenadeServerRpc(Vector3 initalVelocity)
    {
        GameObject grenade = Instantiate(GrenadePrefab, transform.position, Quaternion.identity);
        grenade.GetComponent<NetworkObject>().Spawn();
        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = initalVelocity;
    }
}