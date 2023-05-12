using Unity.Netcode;
using UnityEngine;

public class TopDownCharacter : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 20.0f;
    public float decelerationInputInfluence = 5.0f;

    [Header("Game Objects")]
    public GameObject cameraObject;

    [Header("Velocity")]
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _inputVelocity;
    [SerializeField] private Vector3 _forceVelocity;

    [Header("Misc Components")]
    private CharacterController _characterController;
    private TopDownCamera _topDownCamera;

    [Header("Gizmo Variables")]
    public bool showVelocity;
    public bool showForceVelocity;
    public bool showInputRaw;
    public bool showInputVelocity;

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
        _inputVelocity = ProcessInput(input);
        if (!_topDownCamera.IsInDeadZone())
        {
            RotateCharacter(_topDownCamera.cursorWorldPosition);
        }

        // Movement
        GravityForce();
        _velocity = _forceVelocity + _inputVelocity * moveSpeed;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    public void AddForce(Vector3 force)
    {
        _forceVelocity += force;
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
        _forceVelocity = OrthogonalToSurfaceNormal(_forceVelocity);
        _forceVelocity.y += Time.deltaTime * Physics.gravity.y;

        // Give player control over their deceleration rate
        float decelerationRate = _forceVelocity.magnitude;
        if (_inputVelocity.magnitude > 0.0f)
        {
            decelerationRate *= decelerationInputInfluence;
        }

        if (_characterController.isGrounded) {
            _forceVelocity = Vector3.MoveTowards(_forceVelocity, Vector3.zero, decelerationRate * Time.deltaTime);
            _forceVelocity.y = -0.5f;
        }
    }

    private Vector3 OrthogonalToSurfaceNormal(Vector3 inVector)
    {
        float extend = 1.01f;
        float length = _characterController.skinWidth * extend;
        float radius = _characterController.radius * extend;
        // Bottom Position
        Vector3 p1 = transform.position + _characterController.center + Vector3.down * _characterController.height * 0.5f;
        // Top Position
        Vector3 p2 = p1 + Vector3.up * _characterController.height;

        // Want to retain original Y value for gravity
        float storedGravity = inVector.y;
        if (Physics.CapsuleCast(p1, p2, radius, inVector.normalized, out RaycastHit hit, length, ~0 , QueryTriggerInteraction.Ignore))
        {
            // Project onto surface
            inVector = Vector3.ProjectOnPlane(inVector, hit.normal);

            // If the player is in a corner
            if (Physics.CapsuleCast(p1, p2, radius, inVector.normalized, length, ~0 , QueryTriggerInteraction.Ignore))
            {
                inVector = Vector3.zero;
            }
        }

        inVector.y = storedGravity;
        return inVector;
    }

    public void OnDrawGizmos()
    {
        if (showInputRaw)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + GetInput());
        }

        if (showInputVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _inputVelocity);
        }

        if (showForceVelocity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + _forceVelocity);
        }

        if (showVelocity)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + _velocity);
        }
    }
}
