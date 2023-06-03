using UnityEngine;
using Unity.Netcode;

public class CharacterAnimator : NetworkBehaviour
{
    [Header("Movement")]
    public float DampTime = 0.1f;
    private Vector3 _velocity;
    private Vector3 _previousPositon;
    private CharacterMotor _characterMotor;

    [Header("Aiming")]
    // public float AimDuration = 5.0f;
    // public float AimSpeed = 10.0f;
    // public float HolsterSpeed = 2.5f;
    // private float _elaspedAimTime = 0.0f;
    private float _aimWeight = 0.0f;
    private bool _isAimed = false;
    private CharacterShooting _characterShooting;

    [Header("Core")]
    private GameObject _model;
    private Animator _animator;

    [Header("Hashes")]
    private int _horizontalHash;
    private int _verticalHash;
    private int _speedHash;
    private int _groundedHash;
    private int _shootHash;
    private int _reloadHash;

    [Header("Debugging")]
    public bool ShowAnimationDirection;
    private Vector3 _gizmoAnimationDir;

    private void Start()
    {
        if (!base.IsOwner)
        {
            return;
        }

        _model = transform.GetChild(0).gameObject;
        _animator = _model.GetComponent<Animator>();

        // Movement
        _horizontalHash = Animator.StringToHash("X");
        _verticalHash = Animator.StringToHash("Y");
        _speedHash = Animator.StringToHash("Speed");
        _groundedHash = Animator.StringToHash("OnGround");

        // Shooting
        _reloadHash = Animator.StringToHash("Reloading");
        _shootHash = Animator.StringToHash("Shoot");

        _characterMotor = GetComponent<CharacterMotor>();
        _characterShooting = GetComponent<CharacterShooting>();
    }

    private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        // Movement
        _velocity = (transform.position - _previousPositon) / Time.deltaTime;
        _previousPositon = transform.position;

        AnimatedMovement();
        AnimatedGun();
    }

    private void AnimatedMovement()
    {
        // Get move direction relative to players rotation
        Vector3 forward = transform.forward.normalized * _velocity.z;
        Vector3 right = transform.right.normalized * _velocity.x;

        // Movement floats
        Vector3 animationDir = forward - right;
        _animator.SetFloat(_horizontalHash, animationDir.x, DampTime, Time.deltaTime);
        _animator.SetFloat(_verticalHash, animationDir.z, DampTime, Time.deltaTime);
        _animator.SetFloat(_speedHash, _velocity.magnitude, DampTime, Time.deltaTime);

        // Movement events
        _animator.SetBool(_groundedHash, _characterMotor.isGrounded);

        // Debugging
        _gizmoAnimationDir = animationDir.normalized;
    }

    private void AnimatedGun()
    {
        _animator.SetBool(_reloadHash, _characterShooting.IsReloading);
        _animator.SetBool(_shootHash, _characterShooting.IsShooting);
    }

    public void OnDrawGizmos()
    {
        if (ShowAnimationDirection)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position - _gizmoAnimationDir, 0.1f);
        }
    }
}
