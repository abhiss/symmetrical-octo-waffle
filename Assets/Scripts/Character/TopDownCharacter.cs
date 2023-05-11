using Unity.Netcode;
using UnityEngine;

public class TopDownCharacter : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 20.0f;

    [Header("Game Objects")]
    public GameObject cameraObject;

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

	private void Start()
    {
        if (!base.IsOwner)
		{
            Destroy(cameraObject);
            return;
		}

		if (cameraObject == null)
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
    }

	private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        // Input
        Vector3 input = GetInput();
        inputVelocity = ProcessInput(input);
        if (!_topDownCamera.IsInDeadZone())
        {
            RotateCharacter(_topDownCamera.cursorWorldPosition);
        }

        // Movement
        GravityForce();
        velocity = forceVelocity + (inputVelocity * moveSpeed);
        _characterController.Move(velocity * Time.deltaTime);
    }

    public Vector3 GetInput()
    {
        return new Vector3(
            -Input.GetAxis("Horizontal"),
            0,
            -Input.GetAxis("Vertical")
        );
    }

    private Vector3 ProcessInput(Vector3 input)
    {
        // Keep the input vector normalized
        input = Vector3.ClampMagnitude(input, 1.0f);

        // Project input to surface
        float height = _characterController.height;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, height))
        {
            Vector3 projectedDir = Vector3.ProjectOnPlane(input, hit.normal);
            projectedDir = projectedDir.normalized;

            float slope = projectedDir.y;
            float slopeLimit = -0.1f * _characterController.slopeLimit;
            if (slope < 0 && slope > slopeLimit) {
                return projectedDir * input.magnitude;
            }
        }

        return new Vector3(input.x, 0, input.z);
    }

    private void RotateCharacter(Vector3 lookAtTarget)
    {
        // Rotate the player
        Vector3 dir = Vector3.Normalize(lookAtTarget - transform.position);
        Quaternion toRotation = Quaternion.LookRotation(dir,Vector3.up);
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            toRotation,
            Time.deltaTime * rotateSpeed
        );

        // Lock the axis
        Vector3 lockedAxis = transform.eulerAngles;
        lockedAxis.x = 0;
        lockedAxis.z = 0;
        transform.eulerAngles = lockedAxis;
    }

    private void GravityForce()
    {
        forceVelocity.y += Time.deltaTime * Physics.gravity.y;
        if (_characterController.isGrounded) {
            forceVelocity.y = -0.5f;
        }
        else if (_characterController.isGrounded)
        {
            forceVelocity = OrthogonalToSurfaceNormal(forceVelocity);
            forceVelocity = Vector3.MoveTowards(forceVelocity, Vector3.zero, Time.deltaTime);
        }
    }

    private Vector3 OrthogonalToSurfaceNormal(Vector3 velocity)
    {
        float extend = 1.01f;
        float length = _characterController.skinWidth * extend;
        float radius = _characterController.radius * extend;
        Vector3 p1 = transform.position + _characterController.center + Vector3.down * _characterController.height * 0.5f; // Bottom
        Vector3 p2 = p1 + Vector3.up * _characterController.height; // Top

        if (Physics.CapsuleCast(p1, p2, radius, velocity.normalized, out RaycastHit hit, length, ~0 , QueryTriggerInteraction.Ignore))
        {
            // Get orthogonal vector to surface
            Vector3 upDir = Vector3.Cross(hit.normal, velocity);
            // In the direction of velocity
            velocity = Vector3.Cross(upDir, hit.normal);

            // If the newdir still goes into the wall, player is in a corner and input can be zero
            if (Physics.CapsuleCast(p1, p2, radius, velocity.normalized, length, ~0 , QueryTriggerInteraction.Ignore))
            {
                velocity = Vector3.zero;
            }
        }

        return velocity;
    }

    public void OnDrawGizmos()
    {
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
