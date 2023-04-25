using UnityEngine;
using System.Collections;

public class TopDownCamera : MonoBehaviour
{
    public GameObject PlayerObject;

    [Header("Settings")]
    public float MinCameraRadius;
    public float MaxCameraRadius;
    public float CameraSmoothing;

    [Header("Info")]
    private Vector3 CameraOffset;
    public Vector3 CursorWorldSpacePosition;
    private Vector3 CameraPoint;
    private Vector3 PlayerPosition;
    private Plane CursorPlane = new Plane(Vector3.down, 0);

    public void Awake()
    {
        CameraOffset = transform.localPosition;
    }

    public void LateUpdate()
    {
        if (PlayerObject == null)
        {
            return;
        }

        // Get Cursor position in world space
        Vector3 ScreenPosition = Input.mousePosition;
        Ray ray = Camera.main.ScreenPointToRay(ScreenPosition);
        if (CursorPlane.Raycast(ray, out float dist))
        {
            CursorWorldSpacePosition = ray.GetPoint(dist);
        }

        PlayerPosition = PlayerObject.transform.position;
        PlayerPosition.y = 0;

        Vector3 diff = (CursorWorldSpacePosition - PlayerPosition) / 2;
        Vector3 dir = Vector3.Normalize(diff);
        float len = Mathf.Min(MaxCameraRadius, diff.magnitude);

        CameraPoint = PlayerPosition + dir * len;
        transform.position = Vector3.Lerp(
            transform.position,
            CameraPoint + CameraOffset,
            Time.deltaTime * CameraSmoothing
        );
    }

    void OnDrawGizmosSelected()
    {
        if (PlayerObject == null)
        {
            return;
        }

        // Draws a blue line from this transform to the target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(PlayerPosition, CursorWorldSpacePosition);
        Gizmos.DrawSphere(CameraPoint, 0.5f);
    }
}
