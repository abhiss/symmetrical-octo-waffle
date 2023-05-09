using UnityEngine;
using System.Collections.Generic;

public class TopDownCamera : MonoBehaviour
{
    public GameObject PlayerObject;

    [Header("Camera Settings")]
    public float maxCameraRadius;
    public float cameraSmoothing;
    public bool lockHeight = false;
    public float heightLock = 0.0f;
    public float rotateDeadZoneRadius = 1.75f;
    private Vector3 _camVelocity = Vector3.zero;

    [Header("Rendering")]
    public LayerMask wallLayer;
    private RaycastHit _currHit;
    private Collider _prevHit;

    [Header("Info")]
    public Vector3 cursorWorldPosition;
    private Vector3 _cameraOffset;
    private Vector3 _cameraPoint;
    private Vector3 _playerPosition;
    private Plane _cursorPlane;

    public void Awake()
    {
        _cameraOffset = transform.localPosition;
    }

    public void LateUpdate()
    {
        if (PlayerObject == null)
        {
            return;
        }

        FadeObstructions();

        // Get Cursor position in world space
        _playerPosition = PlayerObject.transform.position;
        _cursorPlane = new Plane(Vector3.down, _playerPosition.y);

        // Get point on plane
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (_cursorPlane.Raycast(ray, out float dist))
        {
            cursorWorldPosition = ray.GetPoint(dist);
        }

        Vector3 diff = (cursorWorldPosition - _playerPosition) / 2;
        Vector3 dir = Vector3.Normalize(diff);
        float len = Mathf.Min(maxCameraRadius, diff.magnitude);

        // Lock the camera at the offset
        _cameraPoint = _playerPosition + dir * len;
        if (lockHeight)
        {
            _cameraPoint.y = heightLock;
        }

        transform.position = Vector3.SmoothDamp(
            transform.position,
            _cameraPoint + _cameraOffset,
            ref _camVelocity,
            cameraSmoothing
        );
    }

    public bool IsInDeadZone()
    {
        float len = Vector3.Distance(cursorWorldPosition, _playerPosition);
        return len <= rotateDeadZoneRadius;
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
        Gizmos.DrawLine(_playerPosition, cursorWorldPosition);
        Gizmos.DrawSphere(_cameraPoint, 0.5f);
    }
}
