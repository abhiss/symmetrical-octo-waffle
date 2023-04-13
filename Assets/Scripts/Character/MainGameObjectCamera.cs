using UnityEngine;
using System.Collections;

public class TemporaryCameraScript : MonoBehaviour
{
    // TODO: Temp code, should be split up later
    // Note - The camera later will be ECS
    public GameObject PlayerObject;

    [Header("Settings")]
    public Vector3 CameraOffset;
    public float MaxCameraRadius;
    public float CameraStep;
    public float CameraSmoothing;
    private Camera CameraComponent;
    private Vector3 CameraPoint;
    private Vector3 CursorWorldSpacePosition;
    private Vector3 PlayerPosition;

    void Start()
    {
        CameraComponent = transform.GetComponent<Camera>();
    }

    /*
    * TODO: Note: this code should be part of the character
    */
    private void CharacterLookAtCursor()
    {
        Vector3 dir = Vector3.Normalize(CursorWorldSpacePosition - PlayerPosition);
        PlayerObject.transform.rotation = Quaternion.LookRotation(dir,Vector3.up);

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
        Ray ray = CameraComponent.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            CursorWorldSpacePosition = hit.point;
            CursorWorldSpacePosition.y = 0;
        }

        PlayerPosition = PlayerObject.transform.position;
        PlayerPosition.y = 0;

        // Camera point between cursor and player
        CameraPoint = CursorWorldSpacePosition + PlayerPosition;

        // Constrain Camera to radius
        Vector3 dir = Vector3.Normalize(CursorWorldSpacePosition - PlayerPosition);
        float len = Vector3.Distance(CursorWorldSpacePosition, PlayerPosition);
        if (len > MaxCameraRadius)
        {
            CameraPoint = PlayerPosition + dir * MaxCameraRadius;
        }

        CameraPoint = CameraPoint / CameraStep;
        CameraPoint.y = 0;

        CharacterLookAtCursor();

        Vector3 targetPostion = CameraPoint + CameraOffset;
        transform.position = Vector3.Lerp(
            transform.position,
            targetPostion,
            CameraSmoothing * Time.deltaTime
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
        Gizmos.DrawLine(PlayerObject.transform.position, CursorWorldSpacePosition);
        Gizmos.DrawSphere(CameraPoint, 0.5f);
    }
}
