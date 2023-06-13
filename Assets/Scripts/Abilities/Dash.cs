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

    NetworkVariable<bool> n_isDashing = new (false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    private void Start()
    {
        n_isDashing.OnValueChanged = (prev, next) => {
            if(next == false)
            {
                DashVFX.SendEvent(VisualEffectAsset.StopEventID);
            }
        };
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
            if(n_isDashing.Value) n_isDashing.Value = false;
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

    //tells every other client to play dash particle
    [ServerRpc(Delivery = RpcDelivery.Reliable, RequireOwnership = false)]
    private void PlayerDashServerRpc(ServerRpcParams serverRpcParams = default)
    {
        PlayerDashClientRpc();
    }

    [ClientRpc]
    private void PlayerDashClientRpc()
    {
        PlayerDashInner();
    }

    private void PlayerDashInner()
    {
        DashVFX.SendEvent(VisualEffectAsset.PlayEventID);
    }

    private void PlayerDash()
    {
        n_isDashing.Value = true;
        PlayerDashServerRpc();
        Vector3 dashVector = transform.forward.normalized;
        _characterMotor.DashInputOverride = dashVector * DashForce;
        PlayerDashInner();
    }
}
