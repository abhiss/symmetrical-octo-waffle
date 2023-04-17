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

    public void Init()
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
        inputDir = Vector3.ClampMagnitude(inputDir, 1.0f);

        // Input parrallel to surface
        if (controllerHit != null) {
            Vector3 adjustedDir = Vector3.ProjectOnPlane(
                inputDir,
                controllerHit.normal
            ).normalized;

            float slope = adjustedDir.y;
            if(slope < 0) {
                return adjustedDir * inputDir.magnitude;
            }
        }

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
