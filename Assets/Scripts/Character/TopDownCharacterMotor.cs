using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCharacterMotor : MonoBehaviour
{
    // TODO: Get instance of input manager
    public GameObject cameraObject;
    private TemporaryCameraScript cam;
    private TopDownCharacterLogic controller;

    void Start()
    {
        controller = GetComponent<TopDownCharacterLogic>();
        cam = cameraObject.GetComponent<TemporaryCameraScript>();
    }

    void Update()
    {
        controller.inputVelocity = controller.ProcessRawInput(GetInput());
        controller.RotateCharacter(cam.CursorWorldSpacePosition);
        controller.Tick();
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
}
