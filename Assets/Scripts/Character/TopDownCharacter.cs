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
    private ControllerColliderHit _charcaterControllerHit;
    private TopDownCamera _topDownCamera;

    [Header("Gizmo Variables")]
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
        if (_charcaterControllerHit != null) {
            Vector3 adjustedDir = Vector3.ProjectOnPlane(
                input,
                _charcaterControllerHit.normal
            ).normalized;

            float slope = adjustedDir.y;
            if (slope < 0) {
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
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        // Only consider non trigger colliders as collisions
        if (hit.collider.isTrigger) {
            return;
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

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - _gizmoAnimationDir, 0.1f);
    }
}
