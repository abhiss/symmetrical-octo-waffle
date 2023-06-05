using UnityEngine;
using System.Collections;

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

    [Header("Rendering")]
    public LayerMask WallLayer;
    private RaycastHit _currHit;
    private Collider _prevHit;

    [Header("Info")]
    public Vector3 CursorWorldPosition;
    public bool DrawCameraLogic = false;
    private Vector3 _cameraOffset;
    private Vector3 _cameraPoint;
    private Vector3 _playerPosition;
    private Plane _cursorPlane = new Plane(Vector3.down, Vector3.zero);

    [Header("CameraShake")]
    private float _shakeIntensity = 0;
    private float _shakeDecay = 0;

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

    // Camera shake function
    public void Explosion(float intensity, float duration)
    {
        _shakeIntensity = intensity;
        _shakeDecay = intensity / duration;
        StartCoroutine(StopShake(duration));
    }

    private void CameraShake(ref Vector3 cameraPosition)
    {
        if (_shakeIntensity > 0)
        {
            cameraPosition += Random.insideUnitSphere * _shakeIntensity;
            _shakeIntensity -= _shakeDecay * Time.deltaTime;
        }
    }

    private IEnumerator StopShake(float duration)
    {
        yield return new WaitForSeconds(duration);
        _shakeIntensity = 0;
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
        // TODO: Fix logic
        // Detect obstructions
        Vector3 dir = PlayerObject.transform.position - transform.position;
        if (Physics.Raycast(transform.position, dir.normalized, out _currHit, dir.magnitude, WallLayer))
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
