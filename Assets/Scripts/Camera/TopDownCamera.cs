using UnityEngine;
using System.Collections.Generic;

public class TopDownCamera : MonoBehaviour
{
    public GameObject PlayerObject;

    [Header("Movement Settings")]
    public float MinCameraRadius;
    public float MaxCameraRadius;
    public float CameraSmoothing;
    private Vector3 _camVelocity = Vector3.zero;

    [Header("Rendering")]
    public LayerMask wallLayer;
    private RaycastHit _currHit;
    private Collider _prevHit;

    [Header("Info")]
    public Vector3 CursorWorldSpacePosition;
    private Vector3 _cameraOffset;
    private Vector3 _cameraPoint;
    private Vector3 _playerPosition;
    private Plane _cursorPlane = new Plane(Vector3.down, 0);
    public void Awake()
    {
        _cameraOffset = transform.localPosition;
    }

    public void Update()
    {
        if (PlayerObject == null)
        {
            return;
        }

        FadeObstructions();

        // Get Cursor position in world space
        Vector3 ScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(ScreenPosition);
        if (_cursorPlane.Raycast(ray, out float dist))
        {
            CursorWorldSpacePosition = ray.GetPoint(dist);
        }

        _playerPosition = PlayerObject.transform.position;
        _playerPosition.y = 0;

        Vector3 diff = (CursorWorldSpacePosition - _playerPosition) / 2;
        Vector3 dir = Vector3.Normalize(diff);
        float len = Mathf.Min(MaxCameraRadius, diff.magnitude);

        _cameraPoint = _playerPosition + dir * len;
        transform.position = Vector3.SmoothDamp(
            transform.position,
            _cameraPoint + _cameraOffset,
            ref _camVelocity,
            CameraSmoothing
        );
    }

    private void FadeObstructions()
    {
        // TODO: Fix logic
        // Detect obstructions
        Vector3 dir = PlayerObject.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dir.normalized, out _currHit,dir.magnitude, wallLayer))
        {
            GameObject current = _currHit.transform.gameObject;
            if (current.GetComponent<Obstructable>() == null)
            {
                current.AddComponent<Obstructable>().isObstructing = true;
            }

            _prevHit = _currHit.collider;
        }
        else if (_prevHit != null)
        {
            _prevHit.transform.gameObject.GetComponent<Obstructable>().isObstructing = false;
            _prevHit = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (PlayerObject == null)
        {
            return;
        }

        // Draws a blue line from this transform to the target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_playerPosition, CursorWorldSpacePosition);
        Gizmos.DrawSphere(_cameraPoint, 0.5f);
    }
}
