using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;

public class Dash : NetworkBehaviour
{
    public GameObject VFXObject;
    public float DashForce = 10.0f;
    private float _minPlaySpeed = 2.1f;
    private VisualEffect _vfx;

    [Header("Conditionals")]
    public bool IsDashing = false;

    [Header("Core")]
    private CharacterMotor _characterMotor;
    private InputListener _inputListener;

    private void Start()
    {
        if (!IsOwner) return;

        _characterMotor = GetComponent<CharacterMotor>();
        _inputListener = GetComponent<InputListener>();
        _vfx = VFXObject.GetComponent<VisualEffect>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        _vfx.playRate = Mathf.Max(_characterMotor.DashInputOverride.magnitude, _minPlaySpeed);

        // Input
        if (_inputListener.ShiftKey && _characterMotor.isGrounded)
        {
            PlayerDash();
        }

        // Stop the event once we reach the min play speed
        if (_characterMotor.DashInputOverride.magnitude <= _minPlaySpeed)
        {
            _vfx.SendEvent(VisualEffectAsset.StopEventID);
        }

        IsDashing = _characterMotor.DashInputOverride.magnitude > 1.0f;
        _inputListener.DisableInput = IsDashing;

        // Decelerate the input override
        _characterMotor.DashInputOverride = Vector3.MoveTowards(
            _characterMotor.DashInputOverride,
            Vector3.zero,
            Time.deltaTime * DashForce
        );
    }

    private void PlayerDash()
    {
        Vector3 dashVector = transform.forward.normalized;
        _characterMotor.DashInputOverride = dashVector * DashForce;
        _vfx.SendEvent(VisualEffectAsset.PlayEventID);
    }
}
