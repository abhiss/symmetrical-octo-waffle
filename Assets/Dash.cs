using UnityEngine;
using Unity.Netcode;

public class Dash : NetworkBehaviour
{
    public float DashForce = 10.0f;

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

        // Input
        if (_inputListener.ShiftKey && _characterMotor.isGrounded && !IsDashing)
        {
            Vector3 dashVector = transform.forward.normalized;
            _characterMotor.DashInputOverride = dashVector * DashForce;
        }

        // Decelerate the input override
        _characterMotor.DashInputOverride = Vector3.MoveTowards(_characterMotor.DashInputOverride, Vector3.zero, Time.deltaTime * DashForce);
        IsDashing = _characterMotor.DashInputOverride.magnitude > 1.0f;
    }
}
