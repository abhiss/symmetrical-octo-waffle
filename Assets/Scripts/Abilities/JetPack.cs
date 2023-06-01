using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPack : MonoBehaviour
{
    public float MaxHeight = 5.0f;
    private CharacterMotor _character;
    private InputListener _inputListener;
    private bool _hasLaunched = false;

    [Header("Sound")]
    public AudioClip LaunchClip;
    public AudioClip LoopClip;
    public AudioClip LandClip;
    private AudioSource _audioSrc;

    [Header("Debugging")]
    private Vector3 _targetPosition;

    private void Start()
    {
        _character = GetComponent<CharacterMotor>();
        _inputListener = GetComponent<InputListener>();
        _audioSrc = gameObject.AddComponent<AudioSource>();
    }

    private void Update()
    {
        Debug.Log("PLAYER: " + transform.position + " TARGET: " + _targetPosition);
        if (_hasLaunched && _character.isGrounded)
        {
            _hasLaunched = false;
            _audioSrc.PlayOneShot(LandClip);
        }

        if (_inputListener.SpaceDown && _character.isGrounded)
        {
            Vector3 cursorPosition = CursorWorldPosition();
            if (cursorPosition == Vector3.zero)
            {
                // Invalid jump
                return;
            }

            _targetPosition = cursorPosition;
            // Vector3 velocity = OldWay(cursorPosition);
            Vector3 velocity = TrajectoryVelocity(cursorPosition, MaxHeight);
            _character.DisableGrounding = true;
            _character.SetForce(velocity);

            _hasLaunched = true;
            _audioSrc.PlayOneShot(LaunchClip);
        }
    }

    private Vector3 CursorWorldPosition()
    {
        Vector3 target = Vector3.zero;
        Ray cameraRay = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Hit floor
        if (Physics.Raycast(cameraRay, out RaycastHit hit))
        {
            target = hit.point;
        }

        return target;
    }

    private Vector3 TrajectoryVelocity(Vector3 targetPosition, float height)
    {
        // Using S U V A T
        // Goal: Return initial velocity
        // 3 Knowns: Height, horizontal displacement, vertical displacement
        // Find: Up, Down, Right Velocity

        // Want the players feet position not their center
        Vector3 playerPosition = transform.position;
        playerPosition.y -= 1.0f;

        Vector3 sRight = targetPosition - transform.position;
        float a = Physics.gravity.y;

        // Up (final velocity = 0)
        // vUp = sqrt(-2as)
        // tUp = sqrt(-2s/a)
        float vUp = Mathf.Sqrt(-2 * a * height);
        float tUp = Mathf.Sqrt(-2 * height / a);

        // Down (inital velocity = 0)
        // tDown = sqrt(2s/a)
        float tDown = Mathf.Sqrt(2 * (sRight.y - height) / a);

        // Time
        float t = tUp + tDown;

        // Right (acceleration = 0)
        // vRight = s/t
        Vector3 vRight = sRight / t;

        Vector3 initialVelocity = vRight;
        initialVelocity.y = vUp;
        return initialVelocity;
    }

    public void OnDrawGizmos()
    {

    }
}
