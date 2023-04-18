using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCharacterMotor : MonoBehaviour
{
    // TODO: Get instance of input manager
    public GameObject cameraObject;
    public GameObject playerModel;
    private TemporaryCameraScript cam;
    private TopDownCharacterLogic controller;

    [Header("Animatons")]
    public float dampTime = 0.1f;
    private Animator animator;
    private int horizontalHash;
    private int verticalHash;
    private Vector3 animationDir;

    void Start()
    {
        if (playerModel == null || cameraObject == null)
        {
            Debug.LogError(
                "Empty fields in TopDownCharaterMotor script. " +
                "aborted Start() on "
                + transform.name + " gameobject");
            return;
        }

        // Movement + Camera
        cam = cameraObject.GetComponent<TemporaryCameraScript>();
        controller = GetComponent<TopDownCharacterLogic>();
        controller.Init();

        // Animations
        animator = playerModel.GetComponent<Animator>();
        horizontalHash = Animator.StringToHash("Horizontal");
        verticalHash = Animator.StringToHash("Vertical");
    }

    void Update()
    {
        Vector3 input = GetInput();
        controller.inputVelocity = controller.ProcessRawInput(input);
        controller.RotateCharacter(cam.CursorWorldSpacePosition);
        controller.Tick();

        // Animatons
        Vector3 forward = transform.forward.normalized;
        Vector3 right = transform.right.normalized;
        forward *= input.z;
        right *= input.x;

        animationDir = forward + right;
        animator.SetFloat(horizontalHash, animationDir.x, dampTime, Time.deltaTime);
        animator.SetFloat(verticalHash, animationDir.z, dampTime, Time.deltaTime);
    }

    private Vector3 GetInput()
    {
        // Setup a input manager later
        return new Vector3(
            -Input.GetAxis("Horizontal"),
            0,
            -Input.GetAxis("Vertical")
        );
    }

    void OnDrawGizmosSelected()
    {
        // Draws a blue line from this transform to the target
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position + animationDir, 0.1f);
    }
}
