using Unity.Netcode;
using UnityEngine;

public class CharacterMotor : NetworkBehaviour
{
    [Header("Movement Settings")]
    public float MoveSpeed = 3.0f;
    public float RotateSpeed = 20.0f;
    public float DecelerateInputInfluence = 5.0f;
    private InputListener _inputListener;
    private Vector3 _input;

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

        // Input
        _inputListener = GetComponent<InputListener>();

        // Controller + Camera
        _characterCamera = CameraObject.GetComponent<CharacterCamera>();
        _characterController = GetComponent<CharacterController>();
    }

    public void AddForce(Vector3 force)
    {
        _forceVelocity += force;
    }

    private void Update()
    {
        if (!base.IsOwner)
        {
            return;
        }

        // Input
        _input = _inputListener.GetAxisInput();
        _inputVelocity = ProcessInput(_input);
        if (!_characterCamera.IsInDeadZone())
        {
            RotateCharacter(_characterCamera.cursorWorldPosition);
        }

        // Movement
        GravityForce();
        _velocity = _forceVelocity + _inputVelocity * MoveSpeed;
        _characterController.Move(_velocity * Time.deltaTime);
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
            Time.deltaTime * RotateSpeed
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
            decelerationRate *= DecelerateInputInfluence;
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
