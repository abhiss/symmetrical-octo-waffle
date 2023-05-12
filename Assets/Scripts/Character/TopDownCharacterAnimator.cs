using UnityEngine;
using Unity.Netcode;

public class TopDownCharacterAnimator : NetworkBehaviour
{
    [Header("Movement")]
    public float dampTime = 0.1f;
    private Vector3 _velocity;
    private Vector3 _previousPositon;

    [Header("Aiming")]
    public float aimDuration = 5.0f;
    public float aimSpeed = 10.0f;
    public float holsterSpeed = 2.5f;
    private float _elaspedAimTime = 0.0f;
    private float _aimWeight = 0.0f;
    private bool _isAimed = false;
    private TopDownCharacterShooting _shootingComponent;

    [Header("Core")]
    private GameObject _model;
    private Animator _animator;

    [Header("Hashes")]
    private int _horizontalHash;
    private int _verticalHash;

    [Header("Debugging")]
    public bool showAnimationDirection;
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
        _shootingComponent = GetComponent<TopDownCharacterShooting>();
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
        bool shotLastFrame = _shootingComponent.isAiming;
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
        _animator.SetFloat(_horizontalHash, animationDir.x, dampTime, Time.deltaTime);
        _animator.SetFloat(_verticalHash, animationDir.z, dampTime, Time.deltaTime);

        // Debugging
        _gizmoAnimationDir = animationDir.normalized;
    }

    private void AnimateAiming()
    {
        if (_elaspedAimTime >= aimDuration) {
            _isAimed = false;
        }

        if (_isAimed)
        {
            _elaspedAimTime += Time.deltaTime;
            _aimWeight = Mathf.MoveTowards(_aimWeight, 1, Time.deltaTime * aimSpeed);
        }
        else
        {
            _elaspedAimTime = 0;
            _aimWeight = Mathf.MoveTowards(_aimWeight, 0, Time.deltaTime * holsterSpeed);
        }

        // ANIMATION LAYERS: Base: 0, Aiming: 1
        _animator.SetLayerWeight(1, _aimWeight);
    }

    public void OnDrawGizmos()
    {
        if (showAnimationDirection)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position - _gizmoAnimationDir, 0.1f);
        }
    }
}
