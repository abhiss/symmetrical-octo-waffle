using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCharacter : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 20.0f;
    public float rotateDeadZone = 1.75f;

    [Header("Animatons")]
    public float dampTime = 0.1f;
    private Vector3 animationDir;
    private Animator animator;
    private int horizontalHash;
    private int verticalHash;

    [Header("GameObjects")]
    public GameObject cameraObject;
    public GameObject modelObject;

    [Header("Velocities")]
    public Vector3 inputVelocity;
    public Vector3 externalVelocity;
    public Vector3 velocity;
    private CharacterController characterController;
    private ControllerColliderHit controllerHit;

    [Header("Misc")]
    private TopDownCamera cam;

    public void Start()
    {
        if (modelObject == null || cameraObject == null)
        {
            Debug.LogError(
                "Empty fields in TopDownCharacter script. " +
                "aborted Start() on "
                + gameObject.name + " gameobject");
            return;
        }

        // Controller + Camera
        cam = cameraObject.GetComponent<TopDownCamera>();
        characterController = GetComponent<CharacterController>();

        // Animations
        animator = modelObject.GetComponent<Animator>();
        horizontalHash = Animator.StringToHash("Horizontal");
        verticalHash = Animator.StringToHash("Vertical");
    }

    public void Update()
    {
        // Input
        Vector3 input = GetInput();
        inputVelocity = ProcessInput(input);
        RotateCharacter(cam.CursorWorldSpacePosition);

        // Character Movement
        GravityForce();
        velocity = externalVelocity + (inputVelocity * moveSpeed);
        characterController.Move(velocity * Time.deltaTime);

        // Animatons
        AnimatedMovement(input);
        AnimatedWeapon();
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
        // Deadzone
        float len = Vector3.Distance(lookAtTarget, transform.position);
        if (len <= rotateDeadZone)
        {
            return;
        }

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
        input = Vector3.ClampMagnitude(input, 1.0f);

        // Input parrallel to surface
        if (controllerHit != null) {
            Vector3 adjustedDir = Vector3.ProjectOnPlane(
                input,
                controllerHit.normal
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
        externalVelocity.y += Time.deltaTime * Physics.gravity.y;
        if (characterController.isGrounded) {
            externalVelocity.y = -0.5f;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(!hit.collider.isTrigger) {
            controllerHit = hit;
        }
    }

    private void AnimatedMovement(Vector3 input)
    {
        Vector3 forward = transform.forward.normalized;
        Vector3 right = transform.right.normalized;
        forward *= input.z;
        right *= input.x;

        animationDir = forward - right;
        animator.SetFloat(horizontalHash, animationDir.x, dampTime, Time.deltaTime);
        animator.SetFloat(verticalHash, animationDir.z, dampTime, Time.deltaTime);
    }

    private void AnimatedWeapon()
    {
        // TODO: Set layer weight
    }

    void OnDrawGizmosSelected()
    {
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + animationDir, 0.1f);
    }
}
