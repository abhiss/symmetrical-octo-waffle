using UnityEngine;
using Unity.Netcode;

public class CharacterAnimator : NetworkBehaviour
{
    [Header("Movement")]
    public float DampTime = 0.1f;
    private Vector3 _velocity;
    private Vector3 _previousPositon;

    [Header("Aiming")]
    public float AimDuration = 5.0f;
    public float AimSpeed = 10.0f;
    public float HolsterSpeed = 2.5f;
    private float _elaspedAimTime = 0.0f;
    private float _aimWeight = 0.0f;
    private bool _isAimed = false;
    private CharacterShooting _characterShooting;

    [Header("Core")]
    private GameObject _model;
    private Animator _animator;

    [Header("Hashes")]
    private int _horizontalHash;
    private int _verticalHash;

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

        // Aiming
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

        // Aiming
        bool shotLastFrame = _characterShooting.IsAiming;
        if (shotLastFrame == true)
        {
            _elaspedAimTime = 0;
            _isAimed = true;
        }
        AnimateAiming();
    }

    private void AnimatedMovement()
    {
        // Get move direction relative to players rotation
        Vector3 forward = transform.forward.normalized * _velocity.z;
        Vector3 right = transform.right.normalized * _velocity.x;

        // Set animation floats
        Vector3 animationDir = forward - right;
        _animator.SetFloat(_horizontalHash, animationDir.x, DampTime, Time.deltaTime);
        _animator.SetFloat(_verticalHash, animationDir.z, DampTime, Time.deltaTime);

        // Debugging
        _gizmoAnimationDir = animationDir.normalized;
    }

    private void AnimateAiming()
    {
        if (_elaspedAimTime >= AimDuration) {
            _isAimed = false;
        }

        if (_isAimed)
        {
            _elaspedAimTime += Time.deltaTime;
            _aimWeight = Mathf.MoveTowards(_aimWeight, 1, Time.deltaTime * AimSpeed);
        }
        else
        {
            _elaspedAimTime = 0;
            _aimWeight = Mathf.MoveTowards(_aimWeight, 0, Time.deltaTime * HolsterSpeed);
        }

        // ANIMATION LAYERS: Base: 0, Aiming: 1
        _animator.SetLayerWeight(1, _aimWeight);
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
