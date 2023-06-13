using UnityEngine;

public class CharacterCursor : MonoBehaviour
{
    public Transform Player;
    private Plane _cursorPlane = new Plane(Vector3.down, Vector3.zero);

    private void Start()
    {
        Cursor.visible = false;
    }

    private void LookAt2D(Vector3 lookAtPosition)
    {
        Vector3 dir = lookAtPosition - transform.position;
        float angle = Mathf.Atan2(dir.y,dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    private void Update()
    {
        if (Player == null)
            return;

        transform.position = Input.mousePosition;
        Vector3 playerPos2D = Camera.main.WorldToScreenPoint(Player.position);
        LookAt2D(playerPos2D);
    }
}
