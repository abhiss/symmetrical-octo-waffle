using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TopDownCharacterMotor : MonoBehaviour
{
    // Start is called before the first frame update
    public Vector2 inputVector;
    public Vector3 velocity;
    private float gravity;
    private CharacterController controller;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        gravity = Physics.gravity.y;
    }

    // Update is called once per frame
    void Update() {
        inputVector = GetInput();
        GravitySimulation();
        controller.Move(velocity);
    }

    private Vector2 GetInput() {
        return new Vector2(0,0);
    }

    private void InputParallelToSurface(Vector2 input){

    }

    private void GravitySimulation() {
        velocity.y += Time.deltaTime * gravity;
    }

    private void RotateCharacter() {

    }
}
