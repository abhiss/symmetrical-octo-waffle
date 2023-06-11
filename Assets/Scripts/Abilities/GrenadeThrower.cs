using UnityEngine;
using Unity.Netcode;
using SharedMath;
using UnityEngine.VFX;

public class GrenadeThrower : NetworkBehaviour
{
    // Add references to required components and settings
    public GameObject GrenadePrefab;
    public float MaxHeight = 5.0f;

    private InputListener _inputListener;

    private void Start()
    {
        if (!IsOwner) return;
        _inputListener = GetComponent<InputListener>();
    }

    private void Update()
    {

        if (!IsOwner) return;

        if (_inputListener.UseKey)
        {
            LaunchPlayer();
        }
    }

    private void LaunchPlayer()
    {
        Vector3 cursorPosition = CursorWorldPosition();

        // Calculate trajectory
        LaunchData launchData = Trajectory.CalculateLaunchData(transform.position, cursorPosition, MaxHeight);
        GameObject grenade = Instantiate(GrenadePrefab, transform.position, Quaternion.identity);

        Rigidbody rb = grenade.GetComponent<Rigidbody>();
        rb.velocity = launchData.InitalVelocity;
    }



    private Vector3 CursorWorldPosition()
    {
        Vector3 target = Vector3.zero;
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Hit floor
        if (Physics.Raycast(cameraRay, out RaycastHit hit))
        {
            target = hit.point;
        }

        return target;
    }
}
