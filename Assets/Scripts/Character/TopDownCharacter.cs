using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor.UI;
using UnityEngine;

public class TopDownCharacter : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 20.0f;
    public float rotateDeadZoneRadius = 1.75f;

    [Header("Animatons")]
    public float dampTime = 0.1f;
    private int _horizontalHash;
    private int _verticalHash;
    private Animator _animator;

    [Header("Game Objects")]
    public GameObject cameraObject;
    private GameObject _model;

    [Header("Velocity")]
    public Vector3 velocity;
    public Vector3 inputVelocity;
    public Vector3 forceVelocity;

    [Header("Misc Components")]
    private CharacterController _characterController;
    private TopDownCamera _topDownCamera;

    [Header("Gizmo Variables")]
    public bool showAnimationDirection;
    public bool showVelocity;
    public bool showForceVelocity;
    public bool showInput;
    private Vector3 _gizmoAnimationDir;

	public void Start()
    {
        if (!base.IsOwner)
		{
            cameraObject.SetActive(false);
            return;
		}

		_model = transform.GetChild(0).gameObject;
		if (_model == null || cameraObject == null)
		{
			Debug.LogError(
				"Assumed player prefab Hierarchy is most likely altered. " +
				"Aborted in Start() on "
				+ gameObject.name + " gameobject");
			return;
		}

		// Controller + Camera
		_topDownCamera = cameraObject.GetComponent<TopDownCamera>();
		_characterController = GetComponent<CharacterController>();

		// Animations
		_animator = _model.GetComponent<Animator>();
		_horizontalHash = Animator.StringToHash("Horizontal");
		_verticalHash = Animator.StringToHash("Vertical");
    }

	public void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        // Input
        Vector3 input = GetInput();
        inputVelocity = ProcessInput(input);
        RotateCharacter(_topDownCamera.CursorWorldSpacePosition);

        // Character Movement
        GravityForce();
        velocity = forceVelocity + (inputVelocity * moveSpeed);
        _characterController.Move(velocity * Time.deltaTime);

        // Animatons
        AnimatedMovement(input);
    }

    public Vector3 GetInput()
    {
        return new Vector3(
            -Input.GetAxis("Horizontal"),
            0,
            -Input.GetAxis("Vertical")
        );
    }

    public void RotateCharacter(Vector3 lookAtTarget)
    {
        // Rotation dead zone
        float len = Vector3.Distance(lookAtTarget, transform.position);
        if (len <= rotateDeadZoneRadius)
        {
            return;
        }

        // Rotate the player
        Vector3 dir = Vector3.Normalize(lookAtTarget - transform.position);
        Quaternion toRotation = Quaternion.LookRotation(dir,Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            toRotation,
            Time.deltaTime * rotateSpeed);

        // Lock the axis
        Vector3 lockedAxis = transform.eulerAngles;
        lockedAxis.x = 0;
        lockedAxis.z = 0;
        transform.eulerAngles = lockedAxis;
    }

    public Vector3 ProcessInput(Vector3 input)
    {
        // Keep the input vector normalized
        input = Vector3.ClampMagnitude(input, 1.0f);

        // Input parrallel to surface
        float height = _characterController.height;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, height))
        {
            Vector3 adjustedDir = Vector3.ProjectOnPlane(input,hit.normal);
            adjustedDir = adjustedDir.normalized;

            float slope = adjustedDir.y;
            float slopeLimit = -0.1f * _characterController.slopeLimit;
            if (slope < 0 && slope > slopeLimit) {
                return adjustedDir * input.magnitude;
            }
        }

        return new Vector3(input.x, 0, input.z);
    }

    private void GravityForce()
    {
        forceVelocity.y += Time.deltaTime * Physics.gravity.y;
        if (_characterController.isGrounded) {
            forceVelocity.y = -0.5f;
        }
        else if (_characterController.isGrounded)
        {
            forceVelocity = VelocityParrellelToSurface(forceVelocity);
            forceVelocity = Vector3.MoveTowards(forceVelocity, Vector3.zero, Time.deltaTime);
        }
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

    private Vector3 VelocityParrellelToSurface(Vector3 velocity)
    {
        if (!_characterController.isGrounded)
        {
            return velocity;
        }

        float extend = 1.01f;
        float length = _characterController.skinWidth * extend;
        float radius = _characterController.radius * extend;
        Vector3 newVelocity = velocity;
        Vector3 p1 = transform.position + _characterController.center + Vector3.up * -_characterController.height * 0.5f; // Bottom
        Vector3 p2 = p1 + Vector3.up * _characterController.height; // Top

        RaycastHit hit;
        if(Physics.CapsuleCast(p1, p2, radius, newVelocity.normalized, out hit, length, ~0 , QueryTriggerInteraction.Ignore)) {
            // Make input parrellel to surface normal
            Vector3 temp = Vector3.Cross(hit.normal, newVelocity);
            newVelocity = Vector3.Cross(temp, hit.normal);

            // If the newdir still goes into the wall, player is in a corner and input can be zero
            if(Physics.CapsuleCast(p1, p2, radius, newVelocity.normalized, out hit, length, ~0 , QueryTriggerInteraction.Ignore)){
                newVelocity = Vector3.zero;
            }
        }

        return newVelocity;
    }

    public void OnDrawGizmos()
    {
        if (showAnimationDirection)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position - _gizmoAnimationDir, 0.1f);
        }

        if (showForceVelocity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + forceVelocity);
        }

        if (showVelocity)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, transform.position + velocity);
        }
    }
}
