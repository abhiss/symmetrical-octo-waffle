using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterCamera : MonoBehaviour
{
    public GameObject PlayerObject;
    private InputListener _inputListener;

    [Header("Camera Settings")]
    public float MaxCameraRadius = 3.0f;
    public float CameraSmoothing = 0.3f;
    public float rotateDeadZoneRadius = 1.75f;

    [Header("Misc Settings")]
    public bool LockHeight = false;
    public float HeightLock = 0.0f;
    private Vector3 _camVelocity = Vector3.zero;

    [Header("Obstructions")]
    public LayerMask WallLayer;
    private List<GameObject> _fadedObstructions = new List<GameObject>();

    [Header("Info")]
    public Vector3 CursorWorldPosition;
    public bool DrawCameraLogic = false;
    private Vector3 _cameraOffset;
    private Vector3 _cameraPoint;
    private Vector3 _playerPosition;
    private Plane _cursorPlane = new Plane(Vector3.down, Vector3.zero);

    public void Start()
    {
        _cameraOffset = transform.localPosition;
        _inputListener = PlayerObject.GetComponent<InputListener>();
    }

    public void LateUpdate()
    {
        if (PlayerObject == null)
        {
            Debug.LogError("Player object is null");
            return;
        }

        FadeObstructions();

        // Get Cursor position in world space
        _playerPosition = PlayerObject.transform.position;
        _cursorPlane.SetNormalAndPosition(Vector3.up, _playerPosition);

        // Get point on plane
        Ray ray = Camera.main.ScreenPointToRay(_inputListener.MousePosition);
        if (_cursorPlane.Raycast(ray, out float dist))
        {
            CursorWorldPosition = ray.GetPoint(dist);
        }

        _cameraPoint = GetCameraPosition();
        Vector3 finalPosition = _cameraPoint + _cameraOffset;
        CameraShake(ref finalPosition);

        transform.position = Vector3.SmoothDamp(
            transform.position,
            finalPosition,
            ref _camVelocity,
            CameraSmoothing
        );
    }

    private void CameraShake(ref Vector3 cameraPosition)
    {

    }

    private Vector3 GetCameraPosition()
    {
        Vector3 diff = (CursorWorldPosition - _playerPosition) / 2;
        Vector3 dir = Vector3.Normalize(diff);
        float len = Mathf.Min(MaxCameraRadius, diff.magnitude);

        // Lock the camera at the offset
        Vector3 camPos = _playerPosition + dir * len;
        if (LockHeight)
        {
            camPos.y = HeightLock;
        }
        return camPos;
    }

    public bool CursorWithinDeadzone()
    {
        float len = Vector3.Distance(CursorWorldPosition, _playerPosition);
        return len <= rotateDeadZoneRadius;
    }

    private void FadeObstructions()
    {
        float cameraDistance = Vector3.Distance(transform.position, PlayerObject.transform.position);
        Vector3 cameraDir = (PlayerObject.transform.position - transform.position).normalized;
        RaycastHit[] hits = Physics.RaycastAll(transform.position, cameraDir, cameraDistance, WallLayer);

        // Fade each obstruction found
        foreach (var hitCollider in hits)
        {
            GameObject currentObject = hitCollider.transform.gameObject;
            if (!_fadedObstructions.Contains(currentObject))
            {
                currentObject.AddComponent<Obstructable>().isObstructing = true;
                _fadedObstructions.Add(currentObject);
            }
        }

        // Convert the raycast hits to game object list
        List<GameObject> mylist = new List<GameObject>();
        foreach (var hitCollider in hits)
        {
            mylist.Add(hitCollider.transform.gameObject);
        }

        // Unfade objects found in the exception list
        IEnumerable<GameObject> exceptionList = _fadedObstructions.Except(mylist);
        foreach (var hitCollider in exceptionList)
        {
            GameObject currentObject = hitCollider.transform.gameObject;
            currentObject.GetComponent<Obstructable>().isObstructing = false;
            _fadedObstructions.Remove(currentObject);
        }
    }

    private void OnDrawGizmos()
    {
        if (PlayerObject == null || DrawCameraLogic == false)
        {
            return;
        }

        // Draws a blue line from this transform to the target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(_playerPosition, CursorWorldPosition);
        Gizmos.DrawSphere(_cameraPoint, 0.5f);
    }
}
