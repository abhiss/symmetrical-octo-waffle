using UnityEngine;
using Unity.Netcode;

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
        _horizontalHash = Animator.StringToHash("Horizontal");
        _verticalHash = Animator.StringToHash("Vertical");

        // Shooting
        _reloadHash = Animator.StringToHash("Reloading");
        _shootHash = Animator.StringToHash("Shoot");

        // Misc
        _jetpackHash = Animator.StringToHash("UseJetpack");
        _dashHash = Animator.StringToHash("IsDashing");

        _jetPack = GetComponent<JetPack>();
        _dash = GetComponent<Dash>();
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

        // Debugging
        _gizmoAnimationDir = animationDir.normalized;
    }

    private void AnimatedGun()
    {
        _animator.SetBool(_reloadHash, _characterShooting.IsReloading);
        _animator.SetBool(_shootHash, _characterShooting.IsShooting);
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
