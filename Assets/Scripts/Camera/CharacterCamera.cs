using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CharacterCamera : MonoBehaviour
{
  public GameObject PlayerObject;
  private InputListener _inputListener;
  private List<GameObject> _fadedObstructions = new List<GameObject>();

  [Header("Camera Settings")]
  public float MaxCameraRadius = 3.0f;
  public float CameraSmoothing = 0.3f;
  public float rotateDeadZoneRadius = 1.75f;

  [Header("Misc Settings")]
  public bool LockHeight = false;
  public float HeightLock = 0.0f;
  private Vector3 _camVelocity = Vector3.zero;

  [Header("CameraShake")]
  private float _shakeIntensity = 0;
  private float _shakeDecay = 0;

  [Header("Live Variables")]
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
    ProcessShake(ref finalPosition);

    transform.position = Vector3.SmoothDamp(
        transform.position,
        finalPosition,
        ref _camVelocity,
        CameraSmoothing
    );
  }

  public void ShakeCamera(float intensity, float duration)
  {
    _shakeIntensity = intensity;
    _shakeDecay = intensity / duration;
    StartCoroutine(StopShake(duration));
  }

  private IEnumerator StopShake(float duration)
  {
    yield return new WaitForSeconds(duration);
    _shakeIntensity = 0;
  }

  private void ProcessShake(ref Vector3 cameraPosition)
  {
    if (_shakeIntensity > 0)
    {
      cameraPosition += Random.insideUnitSphere * _shakeIntensity;
      _shakeIntensity -= _shakeDecay * Time.deltaTime;
    }
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

  public Vector3 GetLookAtPosition()
  {
    return _cameraPoint;
  }

  public bool CursorWithinDeadzone()
  {
    float len = Vector3.Distance(CursorWorldPosition, _playerPosition);
    return len <= rotateDeadZoneRadius;
  }

  private void FadeObstructions()
  {
    LayerMask targetMask = _inputListener.WallMask | _inputListener.ObstructionMask;
    float cameraDistance = Vector3.Distance(transform.position, PlayerObject.transform.position);
    Vector3 cameraDir = (PlayerObject.transform.position - transform.position).normalized;
    RaycastHit[] hits = Physics.RaycastAll(transform.position, cameraDir, cameraDistance, targetMask);

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

    // Convert the raycast hits array to game object list
    List<GameObject> mylist = new List<GameObject>();
    foreach (var hitCollider in hits)
    {
      mylist.Add(hitCollider.transform.gameObject);
    }

    // Unfade objects found in the exception list
    _fadedObstructions = _fadedObstructions.Where(obstruction =>
    {
      if (mylist.Contains(obstruction))
      {
        GameObject currentObject = obstruction.transform.gameObject;
        currentObject.GetComponent<Obstructable>().isObstructing = false;
        return false;
      }
      return true;
    }).ToList();
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
