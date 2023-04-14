using UnityEngine;
using System.Collections;

public class TemporaryCameraScript : MonoBehaviour
{
    // TODO: Temp code, should be split up later
    // Note - The camera later will be ECS
    public GameObject PlayerObject;

    [Header("Settings")]
    public Vector3 CameraOffset;
    public float MinCameraRadius;
    public float MaxCameraRadius;
    public float CameraSmoothing;
    private Camera CameraComponent;
    private Vector3 CameraPoint;
    private Vector3 CursorWorldSpacePosition;
    private Vector3 PlayerPosition;
    private Plane CursorPlane = new Plane(Vector3.down, 0);

    void Start()
    {
        CameraComponent = GetComponent<Camera>();
    }

    /*
    * TODO: Note: this code should be part of the character
    */
    private void CharacterLookAtCursor()
    {
        Vector3 dir = Vector3.Normalize(CursorWorldSpacePosition - PlayerPosition);
        Quaternion toRotation = Quaternion.LookRotation(dir,Vector3.up);
        PlayerObject.transform.rotation = Quaternion.Slerp(PlayerObject.transform.rotation, toRotation, Time.deltaTime * 20);

        // Lock the axis
        Vector3 lockedAxis = PlayerObject.transform.eulerAngles;
        lockedAxis.x = 0;
        PlayerObject.transform.eulerAngles = lockedAxis;
    }

    void Update()
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

        // Deadzone
        if (len > MinCameraRadius) {
            CharacterLookAtCursor();
        }

        CameraPoint = PlayerPosition + dir * len;
        transform.position = CameraPoint + CameraOffset;
        // transform.position = Vector3.Lerp(
        //     transform.position,
        //     targetPostion,
        //     CameraSmoothing * Time.deltaTime
        // );
    }

    void OnDrawGizmosSelected()
    {
        if (PlayerObject == null)
        {
            return;
        }

        // Draws a blue line from this transform to the target
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(PlayerObject.transform.position, CursorWorldSpacePosition);
        Gizmos.DrawSphere(CameraPoint, 0.5f);
    }
}
