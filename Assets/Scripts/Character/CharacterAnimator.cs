using UnityEngine;
using Unity.Netcode;
using Unity.Netcode.Components;


public class CharacterAnimator : NetworkBehaviour
{
    [Header("Movement")]
    public float DampTime = 0.1f;
    private Vector3 _velocity;
    private Vector3 _previousPositon;

    [Header("Core")]
    private GameObject _model;
    private Animator _animator;
    private CharacterMotor _characterMotor;
    private CharacterShooting _characterShooting;
    private JetPack _jetPack;
    private Dash _dash;

    [Header("Hashes")]
    private int _horizontalHash;
    private int _verticalHash;
    private int _shootHash;
    private int _reloadHash;
    private int _jetpackHash;
    private int _dashHash;
    private int _groundedHash;
    private int _verticalAimHash;
    private int _horizontalAimHash;
    private int _aimUpDownHash;

    [Header("Debugging")]
    public bool ShowAnimationDirection;
    private Vector3 _gizmoAnimationDir;

    private void Start()
    {
        if (!IsOwner) return;

        _model = transform.GetChild(0).gameObject;
        _animator = _model.GetComponent<Animator>();

        // Movement
        _horizontalHash = Animator.StringToHash("Horizontal");
        _verticalHash = Animator.StringToHash("Vertical");
        _groundedHash = Animator.StringToHash("IsGrounded");

        // Shooting
        _reloadHash = Animator.StringToHash("Reloading");
        _shootHash = Animator.StringToHash("Shoot");
        _verticalAimHash = Animator.StringToHash("VerticalAim");
        _horizontalAimHash = Animator.StringToHash("HorizontalAim");
        _aimUpDownHash = Animator.StringToHash("AimUpDown");

        // Misc
        _jetpackHash = Animator.StringToHash("UseJetpack");
        _dashHash = Animator.StringToHash("IsDashing");

        // Core components
        _characterMotor = GetComponent<CharacterMotor>();
        _characterShooting = GetComponent<CharacterShooting>();

        // Misc components
        _jetPack = GetComponent<JetPack>();
        _dash = GetComponent<Dash>();
    }

    private void Update()
    {
        if (!IsOwner) return;

        // Movement
        _velocity = (transform.position - _previousPositon) / Time.deltaTime;
        _previousPositon = transform.position;

        AnimatedMovement();
        AnimatedGun();
        AnimatedMisc();
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
        _animator.SetBool(_groundedHash, _characterMotor.isGrounded);

        // Debugging
        _gizmoAnimationDir = animationDir.normalized;
    }

    private void AnimatedGun()
    {
        // Get move direction relative to players rotation
        Vector3 forward = transform.forward.normalized * _characterShooting.AimDirection.z;
        Vector3 right = transform.right.normalized * _characterShooting.AimDirection.x;

        // Movement floats
        Vector3 aimDir = forward - right;

        _animator.SetBool(_reloadHash, _characterShooting.IsReloading);
        _animator.SetBool(_shootHash, _characterShooting.IsShooting);

        _animator.SetFloat(_horizontalAimHash, aimDir.z, DampTime, Time.deltaTime);
        _animator.SetFloat(_verticalAimHash, aimDir.x, DampTime, Time.deltaTime);
        _animator.SetFloat(_aimUpDownHash, _characterShooting.AimDirection.y, DampTime, Time.deltaTime);
    }

    private void AnimatedMisc()
    {
        _animator.SetBool(_jetpackHash, _jetPack.HasLaunched);
        _animator.SetBool(_dashHash, _dash.IsDashing);
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
