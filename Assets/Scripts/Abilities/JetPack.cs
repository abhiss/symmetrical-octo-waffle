using UnityEngine;
using Unity.Netcode;
using SharedMath;
using UnityEngine.VFX;
using System.Linq;

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
    public AudioSource LoopAudioSrc;
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
        JetpackLight.intensity = Mathf.MoveTowards(JetpackLight.intensity, _targetIntensity, Time.deltaTime * 2 * JetPackLightIntensity);

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

    }

    private void LaunchPlayer()
    {
        Vector3 cursorPosition = _inputListener.CursorWorldPosition();
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

        OnLaunch();
        // Place projection at target position
    }

    [ClientRpc]
    private void OnJpStateClientRpc(JetpackState jpstate)
    {
        switch (jpstate)
        {
            case JetpackState.launch:
                OnLaunchInner();
                break;
            case JetpackState.land:
                OnLandInner();
                break;
            default:
                break;
        }
    }

    private void OnLaunchInner()
    {
        // Visuals & Sounds
        _targetIntensity = JetPackLightIntensity;
        JetpackVFX_L.SendEvent(VisualEffectAsset.PlayEventID);
        JetpackVFX_R.SendEvent(VisualEffectAsset.PlayEventID);
        if(_audioSrc != null){
            _audioSrc.PlayOneShot(LaunchClip);
        }
        LoopSound();
    }

    //wraps OnLaunchInner + syncing
    private void OnLaunch()
    {
        OnJetpackStateServerRpc(JetpackState.launch);
        OnLaunchInner();
    }

    enum JetpackState
    {
        launch,
        land
    }

    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void OnJetpackStateServerRpc(JetpackState jpstate, ServerRpcParams serverRpcParams = default)
    {
        var clientId = serverRpcParams.Receive.SenderClientId;
        var list = NetworkManager.ConnectedClients.ToList();
        foreach (var client in list)
        {
            //send rpc to everyone except player who triggered it
            if (client.Value.ClientId != clientId)
            {
                Debug.Log("Sending jp particle rpc: " + jpstate.ToString());
                OnJpStateClientRpc(jpstate);
            }
        }
    }

    // This is a edge case. If the player jumps in a corner to a point higher
    // than them, they will be stuck forever. Would fix properly given more time
    private void LandPlayer()
    {
        // Land normally
        if (_characterMotor.isGrounded)
        {
            OnLand();
        }

        // Edge case, abort jetpack
        Vector3 playerVelocity = (transform.position - _previousPosition) / Time.deltaTime;
        if (playerVelocity == Vector3.zero)
        {
            OnLand();
            _characterMotor.SetForce(Vector3.zero);
        }
    }

    private void OnLandInner()
    {
        JetpackVFX_L.SendEvent(VisualEffectAsset.StopEventID);
        JetpackVFX_R.SendEvent(VisualEffectAsset.StopEventID);
        _targetIntensity = 0.0f;

        HasLaunched = false;
        LoopAudioSrc.Pause();
        _audioSrc.PlayOneShot(LandClip);
    }
    //wraps onlandinner + syncing code
    private void OnLand()
    {
        OnJetpackStateServerRpc(JetpackState.land);
        OnLandInner();
    }

    private void LoopSound()
    {
        LoopAudioSrc.clip = LoopClip;
        LoopAudioSrc.Play();
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
