using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dash : MonoBehaviour
{
    public float DashForce = 10.0f;

    [Header("Conditionals")]
    public bool IsDashing = false;

    [Header("Core")]
    private CharacterMotor _characterMotor;
    private InputListener _inputListener;

    private void Start()
    {
        _characterMotor = GetComponent<CharacterMotor>();
        _inputListener = GetComponent<InputListener>();
    }

    private void Update()
    {
        if (_inputListener.ShiftKey && _characterMotor.isGrounded && !IsDashing)
        {
            Vector3 dashVector = transform.forward.normalized;
            _characterMotor.DashInputOverride = dashVector * DashForce;
        }

        _characterMotor.DashInputOverride = Vector3.MoveTowards(_characterMotor.DashInputOverride, Vector3.zero, Time.deltaTime * DashForce);
        IsDashing = _characterMotor.DashInputOverride.magnitude > 1.0f;
    }
}
