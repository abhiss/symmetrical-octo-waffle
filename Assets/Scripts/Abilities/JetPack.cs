using UnityEngine;
using Unity.Netcode;
using SharedMath;
using UnityEngine.VFX;

public class JetPack : NetworkBehaviour
{
    [Header("Settings")]
    public float MaxJumpDistance = 20.0f;
    public float MaxHeight = 5.0f;

    [Header("Conditionals")]
    public bool HasLaunched = false;
    private bool _launchQueued = false;

    [Header("Core")]
    private CharacterMotor _characterMotor;
    private InputListener _inputListener;
    private AudioSource _loopSrc;
    private Vector3 _previousPosition;

    [Header("Sound")]
    public AudioClip LaunchClip;
    public AudioClip LoopClip;
    public AudioClip LandClip;
    private AudioSource _audioSrc;

    [Header("Visuals")]
    public float VFXPlaySpeed = 4.0f;
    public VisualEffect JetpackVFX_L;
    public VisualEffect JetpackVFX_R;
    public Light JetpackLight;
    public float JetPackLightIntensity = 2.0f;
    private float _targetIntensity = 0.0f;

    [Header("Debugging")]
    public bool EnableDebugging = false;
    private Vector3 _targetPosition;
    private Vector3 _originPosition;

    private void Start()
    {
        if (!IsOwner) return;

        JetpackVFX_L.playRate = VFXPlaySpeed;
        JetpackVFX_R.playRate = VFXPlaySpeed;

        _characterMotor = GetComponent<CharacterMotor>();
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

        if (_inputListener.SpaceDown && _characterMotor.isGrounded)
        {
            LaunchPlayer();
        }

        JetpackLight.intensity = Mathf.MoveTowards(JetpackLight.intensity, _targetIntensity, Time.deltaTime *  2 * JetPackLightIntensity);
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
        _characterMotor.SetForce(launchData.InitalVelocity);
        _launchQueued = true;

        _originPosition = transform.position;
        _targetPosition = cursorPosition;

        // Visuals & Sounds
        _targetIntensity = JetPackLightIntensity;
        JetpackVFX_L.SendEvent(VisualEffectAsset.PlayEventID);
        JetpackVFX_R.SendEvent(VisualEffectAsset.PlayEventID);
        _audioSrc.PlayOneShot(LaunchClip);
        LoopSound();
        // Place projection at target position
    }

    // This is a edge case. If the player jumps in a corner to a point higher
    // than them, they will be stuck forever. Would fix properly given more time
    private void LandPlayer()
    {
        // Land normally
        if (_characterMotor.isGrounded)
        {
            Land();
        }

        // Edge case, abort jetpack
        Vector3 playerVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        if (playerVelocity == Vector3.zero)
        {
            Land();
            _characterMotor.SetForce(Vector3.zero);
        }
    }

    private void Land()
    {
        JetpackVFX_L.SendEvent(VisualEffectAsset.StopEventID);
        JetpackVFX_R.SendEvent(VisualEffectAsset.StopEventID);
        _targetIntensity = 0.0f;

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
