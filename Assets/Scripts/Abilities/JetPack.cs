using UnityEngine;
using Unity.Netcode;
using SharedMath;

public class JetPack : NetworkBehaviour
{
    [Header("Settings")]
    public float MaxJumpDistance = 20.0f;
    public float MaxHeight = 5.0f;

    [Header("Conditionals")]
    public bool HasLaunched = false;
    private bool _launchQueued = false;

    [Header("Core")]
    private CharacterMotor _character;
    private InputListener _inputListener;
    private AudioSource _loopSrc;

    [Header("Sound")]
    public AudioClip LaunchClip;
    public AudioClip LoopClip;
    public AudioClip LandClip;
    private AudioSource _audioSrc;

    [Header("Edge Case")]
    private Vector3 _previousPosition;

    [Header("Debugging")]
    public bool EnableDebugging = false;
    private Vector3 _targetPosition;
    private Vector3 _originPosition;

    private void Start()
    {
        if (!IsOwner) return;

        _character = GetComponent<CharacterMotor>();
        _inputListener = GetComponent<InputListener>();
        _audioSrc = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Skip a frame so isGrounded updates
        if (_launchQueued)
        {
            HasLaunched = true;
            _launchQueued = false;
            _previousPosition = transform.position;
            return;
        }

        // Edge case
        if (HasLaunched)
        {
            LandPlayer();
        }

        if (_inputListener.SpaceDown && _character.isGrounded)
        {
            LaunchPlayer();
        }
    }

    private void LaunchPlayer()
    {
        Vector3 cursorPosition = CursorWorldPosition();
        float jumpDistance = GetJumpDistanceXZ(cursorPosition, transform.position);
        bool invalidJump = cursorPosition == Vector3.zero || jumpDistance > MaxJumpDistance || HeadClearance();
        if (invalidJump)
        {
            // Invalid jump
            // Place a projection at cursorPosition
            return;
        }

        // Calculate trajectory
        LaunchData launchData = Trajectory.CalculateLaunchData(transform.position, cursorPosition, MaxHeight);

        // Launch the player
        _character.SetForce(launchData.InitalVelocity);
        _launchQueued = true;

        _audioSrc.PlayOneShot(LaunchClip);
        LoopSound();

        _originPosition = transform.position;
        _targetPosition = cursorPosition;

        // Place projection at target position
    }

    // This is a edge case. If the player jumps in a corner to a point higher
    // than them, they will be stuck forever. Would fix properly given more time
    private void LandPlayer()
    {
        // Land normally
        if (_character.isGrounded)
        {
            Land();
        }

        // Edge case, abort jetpack
        Vector3 playerVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        if (playerVelocity == Vector3.zero)
        {
            Land();
            _character.SetForce(Vector3.zero);
        }
    }

    private void Land()
    {
        HasLaunched = false;
        Destroy(_loopSrc);
        _audioSrc.PlayOneShot(LandClip);
    }

    private void LoopSound()
    {
        _loopSrc = gameObject.AddComponent<AudioSource>();
        _loopSrc.loop = true;
        _loopSrc.clip = LoopClip;
        _loopSrc.pitch = 0.25f;
        _loopSrc.Play();
    }

    private bool HeadClearance()
    {
        return Physics.SphereCast(transform.position, 0.5f, Vector3.up, out RaycastHit hit, MaxHeight);
    }

    private float GetJumpDistanceXZ(Vector3 p1, Vector3 p2)
    {
        p1.y = 0;
        p2.y = 0;
        return Vector3.Distance(p1, p2);
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

    public void OnDrawGizmos()
    {
        if (EnableDebugging)
        {
            // Points
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(_targetPosition, 0.1f);
            Gizmos.DrawSphere(_originPosition, 0.1f);
        }
    }
}
