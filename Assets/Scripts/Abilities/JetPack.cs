using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JetPack : MonoBehaviour
{
    [Header("Settings")]
    public float MaxJumpDistance = 20.0f;
    public float MaxHeight = 5.0f;

    [Header("Conditionals")]
    public bool HasLaunched = false;
    private bool _launchQueued = false;

    [Header("Core")]
    private CharacterMotor _character;
    private InputListener _inputListener;
    private AudioSource _loopSrc;

    [Header("Sound")]
    public AudioClip LaunchClip;
    public AudioClip LoopClip;
    public AudioClip LandClip;
    private AudioSource _audioSrc;

    private void Start()
    {
        _character = GetComponent<CharacterMotor>();
        _inputListener = GetComponent<InputListener>();
        _audioSrc = GetComponent<AudioSource>();
    }

    private void Update()
    {
        // Lets us skip a frame so isGrounded updates
        if (_launchQueued)
        {
            HasLaunched = true;
            _launchQueued = false;
            return;
        }

        // Landing
        if (HasLaunched && _character.isGrounded)
        {
            HasLaunched = false;
            Destroy(_loopSrc);
            _audioSrc.PlayOneShot(LandClip);
        }

        // Input
        // TODO: Clearance checks, distance checks, etc.
        if (_inputListener.SpaceDown && _character.isGrounded)
        {
            Vector3 cursorPosition = CursorWorldPosition();
            float jumpDistance = GetJumpDistanceXZ(cursorPosition, transform.position);
            bool invalidJump = cursorPosition == Vector3.zero || jumpDistance > MaxJumpDistance || HeadClearance();
            if (invalidJump)
            {
                // Invalid jump
                return;
            }

            // Launch the player
            Vector3 initialVelocity = TrajectoryVelocity(cursorPosition, MaxHeight);
            _character.SetForce(initialVelocity);
            _launchQueued = true;

            _audioSrc.PlayOneShot(LaunchClip);
            _loopSrc = gameObject.AddComponent<AudioSource>();
            _loopSrc.loop = true;
            _loopSrc.clip = LoopClip;
            _loopSrc.pitch = 0.25f;
            _loopSrc.Play();
        }
    }

    private bool HeadClearance()
    {
        return Physics.SphereCast(transform.position, 0.5f, Vector3.up, out RaycastHit hit, MaxHeight);
    }

    private float GetJumpDistanceXZ(Vector3 p1, Vector3 p2)
    {
        p1.y = 0;
        p2.y = 0;
        return Vector3.Distance(p1, p2);
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

        // Want the players feet position not their center: -1
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
