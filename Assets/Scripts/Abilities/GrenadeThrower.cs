using UnityEngine;
using Unity.Netcode;
using SharedMath;

public class GrenadeThrower : NetworkBehaviour
{
    public GameObject GrenadePrefab;
    public Animator PlayerAnimator;
    public float MaxGrenadeHeight = 5.0f;
    public bool ThrewGrenade = false;
    private InputListener _inputListener;

    private void Start()
    {
        if (!IsOwner) return;

        _inputListener = GetComponent<InputListener>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        if (_inputListener.GrenadeKey)
        {
            ThrowGrenade();
            ThrewGrenade = true;

        }

        PlayerAnimator.SetBool("IsThrowingGrenade", ThrewGrenade);

        if (ThrewGrenade)
        {
            ThrewGrenade = false;
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
