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
    public float rotateDeadZone = 1.75f;

    [Header("Animatons")]
    public float dampTime = 0.1f;
    private int horizontalHash;
    private int verticalHash;
    private Animator animator;

    [Header("Game Objects")]
    public GameObject cameraObject;
    private GameObject modelObject;

    [Header("Velocity")]
    public Vector3 velocity;
    public Vector3 inputVelocity;
    public Vector3 forceVelocity;

    [Header("Misc Components")]
    private CharacterController characterController;
    private ControllerColliderHit characterControllerHit;
    private TopDownCamera cam;

    [Header("Gizmo Variables")]
    private Vector3 gizmoAnimatorDir;

   
	public void Start()
    {
		modelObject = transform.GetChild(0).gameObject;
		if (modelObject == null || cameraObject == null)
		{
			Debug.LogError(
				"Assumed player prefab Hierarchy is most likely altered. " +
				"Aborted in Start() on "
				+ gameObject.name + " gameobject");
			return;
		}

		// Controller + Camera
		cam = cameraObject.GetComponent<TopDownCamera>();
		if (!base.IsOwner)
		{
            cameraObject.SetActive(false);
		}
		characterController = GetComponent<CharacterController>();

		// Animations
		animator = modelObject.GetComponent<Animator>();
		horizontalHash = Animator.StringToHash("Horizontal");
		verticalHash = Animator.StringToHash("Vertical");
	
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
        RotateCharacter(cam.CursorWorldSpacePosition);

        // Character Movement
        GravityForce();
        velocity = forceVelocity + (inputVelocity * moveSpeed);
        characterController.Move(velocity * Time.deltaTime);

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
        if (len <= rotateDeadZone)
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
        if (characterControllerHit != null) {
            Vector3 adjustedDir = Vector3.ProjectOnPlane(
                input,
                characterControllerHit.normal
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
        if (characterController.isGrounded) {
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
        animator.SetFloat(horizontalHash, animationDir.x, dampTime, Time.deltaTime);
        animator.SetFloat(verticalHash, animationDir.z, dampTime, Time.deltaTime);

        // Debugging
        gizmoAnimatorDir = animationDir;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position - gizmoAnimatorDir, 0.1f);
    }
}
