using Unity.Netcode;
using UnityEngine;

public class CharacterMotor : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float MoveSpeed = 3.0f;
    public float RotateSpeed = 20.0f;
    public float DecelerateInputInfluence = 5.0f;
    [System.NonSerialized] public Vector3 DashInputOverride;
    private InputListener _inputListener;
    private Vector3 _input;

    [Header("Gravity")]
    public bool isGrounded;
    public float MaxGroundedVelocity = 5.0f;
    private bool _disableGrounding = false;

    [Header("Game Objects")]
    public GameObject CameraObject;

    [Header("Velocity")]
    [SerializeField] private Vector3 _velocity;
    [SerializeField] private Vector3 _inputVelocity;
    [SerializeField] private Vector3 _forceVelocity;

    [Header("Misc Components")]
    private CharacterController _characterController;
    private CharacterCamera _characterCamera;

    [Header("Debugging")]
    public bool ShowVelocity;
    public bool ShowForceVelocity;
    public bool ShowInput;
    public bool ShowInputVelocity;

    private void Start()
    {
        // Multiplayer clone
        if (!base.IsOwner)
        {
            Destroy(CameraObject);
            return;
        }

        // Assert we have a camera
        if (CameraObject == null)
        {
            Debug.LogError("Missing camera object on player, exiting.");
            return;
        }

        // Get core components
        _inputListener = GetComponent<InputListener>();
        _characterCamera = CameraObject.GetComponent<CharacterCamera>();
        _characterController = GetComponent<CharacterController>();
    }

    private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        // Input
        _input = _inputListener.GetAxisInput();
        // Keep the input vector normalized but allow input acceleartion
        _input = Vector3.ClampMagnitude(_input, 1.0f);
        if (DashInputOverride != Vector3.zero)
        {
            _input = DashInputOverride;
        }

        _inputVelocity = ProcessInput(_input) * MoveSpeed;
        if (!_characterCamera.CursorWithinDeadzone())
        {
            RotateCharacter(_characterCamera.CursorWorldPosition);
        }

        // Movement
        isGrounded = CustomIsGrounded();
        Gravity(ref _forceVelocity);
        _velocity = _forceVelocity + _inputVelocity;
        _characterController.Move(_velocity * Time.deltaTime);
    }

    // Input Dependent
    // -------------------------------------------------------------------------
    private Vector3 ProcessInput(Vector3 input)
    {
        // Project input to surface
        float height = _characterController.height;
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, height))
        {
            Vector3 projectedDir = Vector3.ProjectOnPlane(input, hit.normal);
            projectedDir = projectedDir.normalized;

            float slope = projectedDir.y;
            float slopeLimit = -0.1f * _characterController.slopeLimit;
            if (slope < 0 && slope > slopeLimit)
            {
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
            Time.deltaTime * RotateSpeed
        );

        // Lock the axis
        Vector3 lockedAxis = transform.eulerAngles;
        lockedAxis.x = 0;
        lockedAxis.z = 0;
        transform.eulerAngles = lockedAxis;
    }

    // Gravity
    // -------------------------------------------------------------------------
    public void AddForce(Vector3 force)
    {
        _disableGrounding = true;
        _forceVelocity += force;
    }

    public void SetForce(Vector3 force)
    {
        _disableGrounding = true;
        _forceVelocity = force;
    }

    private bool CustomIsGrounded()
    {
        if (_velocity.y > 0.0f)
        {
            return false;
        }

        return _characterController.isGrounded;
    }

    private void Gravity(ref Vector3 velocity)
    {
        velocity.y += Time.deltaTime * Physics.gravity.y;

        // Give player control over their deceleration rate
        if (_inputVelocity.magnitude > 0.0f)
        {
            // Don't let player influence their Y Axis
            float savedHeight = velocity.y;

            velocity = Vector3.MoveTowards(
                velocity,
                Vector3.zero,
                DecelerateInputInfluence * Time.deltaTime
            );

            velocity.y = savedHeight;
        }

        if (_disableGrounding)
        {
            _disableGrounding = false;
            return;
        }

        // Make velocity orthogonal to surface
        velocity = OrthogonalToSurfaceNormal(velocity);

        // Exceeded max grounded velocity
        if (velocity.magnitude > MaxGroundedVelocity && isGrounded)
        {
            velocity = Vector3.zero;
        }

        // Keep controller grounded
        if (isGrounded)
        {
            velocity.y = -0.5f;
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

    // Debugging
    // -------------------------------------------------------------------------
    public void OnDrawGizmos()
    {
        if (ShowInput)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + _input);
        }

        if (ShowInputVelocity)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, transform.position + _inputVelocity);
        }

        if (ShowForceVelocity)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + _forceVelocity);
        }

        if (ShowVelocity)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(transform.position, transform.position + _velocity);
        }
    }
}