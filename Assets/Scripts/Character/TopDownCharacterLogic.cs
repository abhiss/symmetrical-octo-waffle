using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCharacterLogic : MonoBehaviour
{
    [Header("Settings")]
    public float moveSpeed = 3.0f;
    public float rotateSpeed = 20.0f;
    public float rotateDeadZone = 1.75f;

    [Header("Character Info")]
    public Vector3 inputVelocity;
    public Vector3 externalVelocity;
    public Vector3 velocity;
    private float gravity;
    private CharacterController controller;
    private ControllerColliderHit controllerHit;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        gravity = Physics.gravity.y;
    }

    public void Tick()
    {
        GravitySimulation();
        velocity = externalVelocity + (inputVelocity * moveSpeed);
        controller.Move(velocity * Time.deltaTime);
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

    public Vector3 ProcessRawInput(Vector3 inputDir)
    {
        if (!controller.isGrounded) {
            return Vector3.zero;
        }

        inputDir = Vector3.ClampMagnitude(inputDir, 1.0f);

        // Parrallel to surface normal (doesnt work)
        // Vector3 adjustedDir = Vector3.ProjectOnPlane(
        //     inputDir,
        //     controllerHit.normal
        // ).normalized;

        // float slope = Mathf.Abs(adjustedDir.y);
        // if(slope > 0 && slope < 1) {
        //     return adjustedDir * inputDir.magnitude;
        // }

        return new Vector3(inputDir.x, 0, inputDir.z);
    }

    private void GravitySimulation()
    {
        externalVelocity.y += Time.deltaTime * gravity;
        if (controller.isGrounded) {
            externalVelocity.y = -0.5f;
        }
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(!hit.collider.isTrigger)
            controllerHit = hit;
    }
}
