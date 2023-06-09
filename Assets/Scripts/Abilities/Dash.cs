using UnityEngine;
using Unity.Netcode;
using UnityEngine.VFX;

public class Dash : NetworkBehaviour
{
    public float DashForce = 10.0f;
    private float _minPlaySpeed = 1.0f;
    public VisualEffect DashVFX;

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
    }

    private void Update()
    {
        if (!IsOwner) return;

        DashVFX.playRate = Mathf.Max(_characterMotor.DashInputOverride.magnitude, _minPlaySpeed);

        // Input
        if (_inputListener.ShiftKey && _characterMotor.isGrounded)
        {
            PlayerDash();
        }

        // Stop the event once we reach the min play speed
        if (_characterMotor.DashInputOverride.magnitude <= _minPlaySpeed)
        {
            DashVFX.SendEvent(VisualEffectAsset.StopEventID);
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
        DashVFX.SendEvent(VisualEffectAsset.PlayEventID);
    }
}
