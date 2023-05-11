using UnityEngine;
using Unity.Netcode;

public class TopDownCharacterAnimator : NetworkBehaviour
{
    [Header("Movement")]
    public float dampTime = 0.1f;
    public bool showAnimationDirection;

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
    private int _horizontalHash;
    private int _verticalHash;

    [Header("Debugging")]
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

    public Vector3 GetInput()
    {
        return new Vector3(
            -Input.GetAxis("Horizontal"),
            0,
            -Input.GetAxis("Vertical")
        );
    }

    private void Update()
    {
        if (!base.IsOwner)
		{
            return;
        }

        // Movement
        Vector3 input = GetInput();
        AnimatedMovement(input);

        // Aiming
        bool shotLastFrame = _shootingComponent.isAiming;
        if (shotLastFrame == true)
        {
            _elaspedAimTime = 0;
            _isAimed = true;
        }
        AnimateAiming();
    }

    private void AnimatedMovement(Vector3 input)
    {
        // Get move direction relative to players rotation
        Vector3 forward = transform.forward.normalized;
        Vector3 right = transform.right.normalized;

        // Apply input
        forward *= input.z;
        right *= input.x;

        // Set animation floats
        Vector3 animationDir = forward - right;
        _animator.SetFloat(_horizontalHash, animationDir.x, dampTime, Time.deltaTime);
        _animator.SetFloat(_verticalHash, animationDir.z, dampTime, Time.deltaTime);

        // Debugging
        _gizmoAnimationDir = animationDir;
    }

    private void AnimateAiming()
    {
        if (_elaspedAimTime >= aimDuration) {
            _isAimed = false;
        }

        if (_isAimed)
        {
            _elaspedAimTime += Time.deltaTime;
            _aimWeight = Mathf.Lerp(_aimWeight, 1, Time.deltaTime * aimSpeed);
        }
        else
        {
            _elaspedAimTime = 0;
            _aimWeight = Mathf.Lerp(_aimWeight, 0, Time.deltaTime * holsterSpeed);
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
